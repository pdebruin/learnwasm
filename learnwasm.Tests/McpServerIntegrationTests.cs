using System.Net.Http.Json;
using System.Text.Json;

namespace learnwasm.Tests;

/// <summary>
/// Integration tests that verify actual communication with the Microsoft Learn MCP server.
/// These tests call the live MCP endpoint with a sample query.
/// </summary>
public class McpServerIntegrationTests
{
    private const string McpEndpoint = "https://learn.microsoft.com/api/mcp";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task McpServer_AcceptsSampleQuery_AndReturnsResults()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");

        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/call",
            @params = new
            {
                name = "microsoft_docs_search",
                arguments = new { query = "What is Blazor" }
            }
        };

        var response = await client.PostAsJsonAsync(McpEndpoint, request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content), "MCP server returned empty response");

        // Parse SSE format
        var jsonData = content;
        if (content.Contains("data: "))
        {
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("data: "))
                {
                    jsonData = line.Substring(6);
                    break;
                }
            }
        }

        using var doc = JsonDocument.Parse(jsonData);
        var root = doc.RootElement;

        // Should not contain an error
        Assert.False(root.TryGetProperty("error", out _), "MCP server returned an error");

        // Should contain results
        Assert.True(root.TryGetProperty("result", out var result), "Response missing 'result'");
        Assert.True(result.TryGetProperty("structuredContent", out var sc), "Response missing 'structuredContent'");
        Assert.True(sc.TryGetProperty("results", out var results), "Response missing 'results'");
        Assert.True(results.GetArrayLength() > 0, "Expected at least one search result");

        // First result should have title and URL
        var first = results[0];
        Assert.True(first.TryGetProperty("title", out var title), "First result missing 'title'");
        Assert.False(string.IsNullOrWhiteSpace(title.GetString()), "First result title is empty");
        Assert.True(first.TryGetProperty("contentUrl", out var url), "First result missing 'contentUrl'");
        Assert.Contains("learn.microsoft.com", url.GetString()!);
    }
}
