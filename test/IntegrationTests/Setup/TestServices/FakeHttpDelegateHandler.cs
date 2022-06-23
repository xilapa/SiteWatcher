using System.Net;
using System.Text.Json;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakeHttpDelegateHandler : DelegatingHandler
{
    private readonly Queue<FakeHttpResponse> _responses;

    public FakeHttpDelegateHandler(params FakeHttpResponse[] responseSequence)
    {
        _responses = new Queue<FakeHttpResponse>(responseSequence);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var fakeResponse = _responses.Dequeue();
        if (fakeResponse.Exception != null)
            throw fakeResponse.Exception;

        var stringContent = fakeResponse.Response != null ?
            new StringContent(JsonSerializer.Serialize(fakeResponse.Response)) : new StringContent(string.Empty);

        return Task.FromResult(new HttpResponseMessage(fakeResponse.StatusCode) {Content = stringContent});
    }
}

public struct FakeHttpResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public object? Response { get; set; }
    public HttpRequestException? Exception { get; set; }
}