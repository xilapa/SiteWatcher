using System.Net.Http.Json;

namespace SiteWatcher.Infra.Http;

public static class HttpClientExtensions
{
    public static async Task<T?> PostAsync<T>(this HttpClient httpClient, string url, object requestBody, 
        CancellationToken cancellationToken)
    {
        var httpResponse = await httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
            return default;
        var result = await httpResponse.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return result;
    }
}