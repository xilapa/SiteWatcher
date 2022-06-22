using System.Net.Http.Json;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Infra.Http;

public class HttpHandler : IHttpHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private HttpClient _httpClient = null!;

    public HttpHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void InitializeHttpClient(string clientName)
    {
        _httpClient = _httpClientFactory.CreateClient(clientName);
    }

    public async Task<T?> PostAsync<T>(string url, object requestBody, CancellationToken cancellationToken)
    {
        var httpResponse = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        var result = await httpResponse.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return result;
    }
}