using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace SiteWatcher.Infra.Http;

internal static class HttpClientExtensions
{
    internal static async Task<T?> PostAsync<T>(this System.Net.Http.HttpClient httpClient, string uri,
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

    internal static async Task<Stream?> GetStreamAsyncWithRetries(this System.Net.Http.HttpClient httpClient,
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
                return Stream.Null;

            using var content = httpResponse.Content;
            using var htmlResult = await content.ReadAsStreamAsync(cancellationToken);

            // Copy the value to a new stream and return it
            var memoryStream = new MemoryStream();
            await htmlResult.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch
        {
            return Stream.Null;
        }
    }
}