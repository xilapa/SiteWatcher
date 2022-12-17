using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SiteWatcher.Common.Services;
using static SiteWatcher.Infra.Http.HttpRetryPolicies;

namespace SiteWatcher.Infra.Http;

public sealed class HttpClient : IHttpClient, IDisposable
{
    private readonly System.Net.Http.HttpClient _client;
    private readonly ILogger<HttpClient> _logger;

    public HttpClient(IHttpClientFactory httpClientFactory, ILogger<HttpClient> logger)
    {
        _client = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public Task<Stream?> GetStreamAsync(Uri uri, CancellationToken ct) =>
        _client.GetStreamAsyncWithRetries(uri, _logger, TransientErrorsRetryWithTimeout, ct);

    public void Dispose() => _client.Dispose();
}