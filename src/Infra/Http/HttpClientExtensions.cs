using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;

namespace SiteWatcher.Infra.Http;

public static class HttpClientExtensions
{
    public static async Task<T?> PostAsync<T>(this HttpClient httpClient, string uri,
        object requestBody, ILogger logger,
        Func<ILogger, string, string, IAsyncPolicy<HttpResponseMessage>> policyFunc,
        CancellationToken cancellationToken)
    {
        var stringBody = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(stringBody);

        var policy = policyFunc(logger, uri, stringBody);
        using var httpResponse = await policy
            .ExecuteAsync(() => httpClient.PostAsync(uri, stringContent, cancellationToken));

        if (!httpResponse.IsSuccessStatusCode)
            return default;

        using var content = httpResponse.Content;
        var result = await content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return result;
    }
}