using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Http;

public static class HttpRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> TransientErrorsRetryPolicy(IServiceProvider services,
        HttpRequestMessage request) =>
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(
                new[] {TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)},
                onRetryAsync: LogRetries(services, request, AuthenticationDefaults.GoogleAuthClient));

    private static Func<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context, Task> LogRetries(
        IServiceProvider services,
        HttpRequestMessage httpRequestMessage, string clientName) =>
        async (outcome, _, _, _) =>
        {
            var statusCode = outcome.Result?.StatusCode;
            var error = outcome.Exception?.Message;
            var innerError = outcome.Exception?.InnerException?.Message;
            if (string.IsNullOrEmpty(error))
                error = outcome.Result is null ? null : await outcome.Result.Content.ReadAsStringAsync();

            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger(clientName);
            logger.LogError("Uri : {Uri}, StatusCode: {Status}, Error: {Error}, InnerError: {Inner}",
                httpRequestMessage.RequestUri, statusCode, error, innerError);
        };
}