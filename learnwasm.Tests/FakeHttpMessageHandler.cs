using System.Net;

namespace learnwasm.Tests;

/// <summary>
/// A fake HttpMessageHandler that returns a canned response and optionally captures the request.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    private readonly Action<HttpRequestMessage>? _onRequest;

    public FakeHttpMessageHandler(HttpResponseMessage response, Action<HttpRequestMessage>? onRequest = null)
    {
        _response = response;
        _onRequest = onRequest;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _onRequest?.Invoke(request);
        return Task.FromResult(_response);
    }
}

/// <summary>
/// A fake IHttpClientFactory that creates clients using a given handler.
/// </summary>
public class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public FakeHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name) => new HttpClient(_handler);
}
