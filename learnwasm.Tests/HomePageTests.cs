using System.Net;
using System.Text.Json;
using Bunit;
using learnwasm.Components.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace learnwasm.Tests;

public class HomePageTests : BunitContext
{
    /// <summary>
    /// Helper to build a fake MCP SSE response with the given results.
    /// </summary>
    private static string BuildMcpResponse(params (string title, string content, string url)[] items)
    {
        var results = items.Select(i => new { title = i.title, content = i.content, contentUrl = i.url });
        var payload = new { result = new { structuredContent = new { results } } };
        return $"data: {JsonSerializer.Serialize(payload)}\n\n";
    }

    private void RegisterMockHttpClient(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var factory = new FakeHttpClientFactory(handler);
        Services.AddSingleton<IHttpClientFactory>(factory);
    }

    [Fact]
    public void HomePage_Renders_SearchInput_And_Button()
    {
        RegisterMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));

        var cut = Render<Home>();

        Assert.NotNull(cut.Find("input[type='text']"));
        Assert.NotNull(cut.Find("button"));
        Assert.Contains("LearnBlazor", cut.Markup);
    }

    [Fact]
    public async Task Search_SendsQuery_And_DisplaysResults()
    {
        var sseBody = BuildMcpResponse(("What is Blazor?", "Blazor is a framework...", "https://learn.microsoft.com/aspnet/core/blazor"));
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(sseBody)
        };
        RegisterMockHttpClient(response);

        var cut = Render<Home>();

        // Type a sample question and click Search
        cut.Find("input[type='text']").Input("What is Blazor");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Verify results are shown
        cut.WaitForState(() => cut.Markup.Contains("Found 1 results"));
        Assert.Contains("What is Blazor?", cut.Markup);
        Assert.Contains("Blazor is a framework", cut.Markup);
    }

    [Fact]
    public async Task Search_EmptyQuery_DoesNothing()
    {
        RegisterMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));

        var cut = Render<Home>();

        // Click search with empty query
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Should not show results section
        Assert.DoesNotContain("Found", cut.Markup);
        Assert.DoesNotContain("No results found", cut.Markup);
    }

    [Fact]
    public async Task Search_ServerError_DisplaysErrorMessage()
    {
        var errorPayload = JsonSerializer.Serialize(new { error = new { message = "Internal server error" } });
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(errorPayload)
        };
        RegisterMockHttpClient(response);

        var cut = Render<Home>();

        cut.Find("input[type='text']").Input("test query");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        cut.WaitForState(() => cut.Markup.Contains("Internal server error"));
        Assert.Contains("Internal server error", cut.Markup);
    }

    [Fact]
    public async Task Search_NoResults_ShowsNoResultsMessage()
    {
        var sseBody = BuildMcpResponse(); // empty results array
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(sseBody)
        };
        RegisterMockHttpClient(response);

        var cut = Render<Home>();

        cut.Find("input[type='text']").Input("xyznonexistent");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        cut.WaitForState(() => cut.Markup.Contains("No results found"));
        Assert.Contains("Found 0 results", cut.Markup);
    }

    /// <summary>
    /// Verifies the JSON-RPC request body sent to the MCP server is well-formed.
    /// </summary>
    [Fact]
    public async Task Search_SendsCorrectMcpRequestFormat()
    {
        string? capturedBody = null;
        var sseBody = BuildMcpResponse(("Result", "Content", "https://example.com"));
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(sseBody) },
            request =>
            {
                capturedBody = request.Content!.ReadAsStringAsync().Result;
            });

        var factory = new FakeHttpClientFactory(handler);
        Services.AddSingleton<IHttpClientFactory>(factory);

        var cut = Render<Home>();
        cut.Find("input[type='text']").Input("sample Blazor question");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        cut.WaitForState(() => cut.Markup.Contains("Found"));

        Assert.NotNull(capturedBody);
        using var doc = JsonDocument.Parse(capturedBody!);
        var root = doc.RootElement;

        Assert.Equal("2.0", root.GetProperty("jsonrpc").GetString());
        Assert.Equal("tools/call", root.GetProperty("method").GetString());
        Assert.Equal("microsoft_docs_search", root.GetProperty("params").GetProperty("name").GetString());
        Assert.Equal("sample Blazor question", root.GetProperty("params").GetProperty("arguments").GetProperty("query").GetString());
    }
}
