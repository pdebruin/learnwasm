using System.Text.Json;
using System.Text.Json.Serialization;

namespace learnwasm.Services;

public class McpService
{
    private readonly HttpClient _httpClient;
    private const string McpEndpoint = "https://learn.microsoft.com/api/mcp";

    public McpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<McpSearchResponse?> SearchDocsAsync(string question)
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/call",
            @params = new
            {
                name = "microsoft_docs_search",
                arguments = new
                {
                    question
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(McpEndpoint, request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var mcpResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(jsonResponse);

            if (mcpResponse?.Result?.Content != null && mcpResponse.Result.Content.Count > 0)
            {
                var content = mcpResponse.Result.Content[0];
                if (content.Type == "text" && !string.IsNullOrEmpty(content.Text))
                {
                    var results = JsonSerializer.Deserialize<List<McpSearchResult>>(content.Text);
                    return new McpSearchResponse
                    {
                        Results = results ?? new List<McpSearchResult>()
                    };
                }
            }

            return new McpSearchResponse { Results = new List<McpSearchResult>() };
        }
        catch (Exception)
        {
            // Return empty results on error
            return new McpSearchResponse { Results = new List<McpSearchResult>() };
        }
    }
}

public class McpJsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string? JsonRpc { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("result")]
    public McpResult? Result { get; set; }
}

public class McpResult
{
    [JsonPropertyName("content")]
    public List<McpContent>? Content { get; set; }
}

public class McpContent
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class McpSearchResponse
{
    public List<McpSearchResult> Results { get; set; } = new();
}

public class McpSearchResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("excerpt")]
    public string? Excerpt { get; set; }
}
