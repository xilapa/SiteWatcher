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

    public static async Task<(Stream html, bool success)> GetStreamAsyncWithRetries(this HttpClient httpClient,
    Uri uri, ILogger logger,
    Func<ILogger, string, string, IAsyncPolicy<HttpResponseMessage>> policyFunc,
    CancellationToken cancellationToken)
    {
        var policy = policyFunc(logger, uri.ToString(), string.Empty);
        try
        {
            using var httpResponse = await policy
                .ExecuteAsync(() => httpClient.GetAsync(uri, cancellationToken));

            if (!httpResponse.IsSuccessStatusCode)
                return (Stream.Null, false);

            using var content = httpResponse.Content;
            using var htmlResult = await content.ReadAsStreamAsync(cancellationToken);

            // Copy the value to a new stream and return it
            htmlResult.Position = 0;
            var memoryStream = new MemoryStream();
            await htmlResult.CopyToAsync(memoryStream, cancellationToken);
            return (memoryStream, true);
        }
        catch
        {
            return (Stream.Null, false);
        }
    }
}