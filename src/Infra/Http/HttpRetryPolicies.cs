﻿using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net.Http;

namespace SiteWatcher.Infra.Http;

public static class HttpRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> AnyErrorRetryWithTimeout(ILogger logger,
        string uri, string requestBody) =>
        AnyErrorRetryPolicy(logger, uri, requestBody)
            .WrapAsync(TimeoutPolicy());

    public static IAsyncPolicy<HttpResponseMessage> AnyErrorRetryPolicy(ILogger logger,
        string uri, string requestBody) =>
        Policy
            .Handle<HttpRequestException>()
            .Or<Exception>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(
                new[] {TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)},
                onRetryAsync: async (outcome, _, retryCount, _) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    var error = outcome.Exception?.Message;
                    var innerError = outcome.Exception?.InnerException?.Message;
                    if (string.IsNullOrEmpty(error))
                        error = outcome.Result is null ? null : await outcome.Result.Content.ReadAsStringAsync();
                    logger
                        .LogError(
                            "RetryCount: {RetryCount}, Uri : {Uri}, RequestBody: {RequestBody}, StatusCode: {Status}, Error: {Error}, InnerError: {Inner}",
                            retryCount, uri, requestBody, statusCode, error, innerError);
                });

    public static IAsyncPolicy<HttpResponseMessage> TransientErrorsRetryWithTimeout(ILogger logger,
        string uri, string requestBody) =>
        TransientErrorsRetryPolicy(logger, uri, requestBody)
            .WrapAsync(TimeoutPolicy());

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
                    logger
                        .LogError(
                            "RetryCount: {RetryCount}, Uri : {Uri}, RequestBody: {RequestBody}, StatusCode: {Status}, Error: {Error}, InnerError: {Inner}",
                            retryCount, uri, requestBody, statusCode, error, innerError);
                });

    public static IAsyncPolicy TimeoutPolicy() =>
        Policy.TimeoutAsync(TimeSpan.FromSeconds(5));
}