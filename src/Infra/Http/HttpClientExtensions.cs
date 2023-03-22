using Microsoft.Extensions.Logging;
using Polly;

namespace SiteWatcher.Infra.Http;

internal static class HttpClientExtensions
{
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
            await using var htmlResult = await content.ReadAsStreamAsync(cancellationToken);

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