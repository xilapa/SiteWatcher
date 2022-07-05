using System.Text.Json;

namespace SiteWatcher.IntegrationTests.Setup;

public class HttpResult
{
    public HttpResult(HttpResponseMessage? httpResponse, string httpMessageContent)
    {
        HttpResponse = httpResponse;
        HttpMessageContent = httpMessageContent;
    }

    public HttpResponseMessage? HttpResponse { get; }
    public string HttpMessageContent { get; }

    public T? GetTyped<T>() =>
        string.IsNullOrEmpty(HttpMessageContent) ? default : JsonSerializer.Deserialize<T>(HttpMessageContent);
}