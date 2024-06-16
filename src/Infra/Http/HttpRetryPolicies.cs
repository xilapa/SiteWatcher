using System.Net;
using Microsoft.Extensions.Logging;
using Polly;

namespace SiteWatcher.Infra.Http;

public static partial class HttpRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> TransientErrorsRetryWithTimeout(ILogger logger,
        string uri, string requestBody) =>
        TimeoutPolicy()
            .WrapAsync(TransientErrorsRetryPolicy(logger, uri, requestBody));

    public static IAsyncPolicy<HttpResponseMessage> TransientErrorsRetryPolicy(ILogger logger,
        string uri, string requestBody) =>
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(
                new[] {TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)},
                onRetryAsync: async (outcome, _, retryCount, _) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    var error = outcome.Exception?.Message;
                    var innerError = outcome.Exception?.InnerException?.Message;
                    if (string.IsNullOrEmpty(error))
                        error = outcome.Result is null ? null : await outcome.Result.Content.ReadAsStringAsync();
                    LogErroOnRequest(logger, retryCount, uri, requestBody, statusCode, error, innerError);
                });

    [LoggerMessage(
        LogLevel.Error,
        "RetryCount: {RetryCount}, Uri : {Uri}, RequestBody: {RequestBody}, StatusCode: {Status}, Error: {Error}, InnerError: {Inner}")]
    public static partial void LogErroOnRequest(ILogger logger, int retryCount, string uri, string requestBody, HttpStatusCode? status,
        string? error, string inner);

    public static IAsyncPolicy TimeoutPolicy() =>
        Policy.TimeoutAsync(TimeSpan.FromSeconds(15));
}