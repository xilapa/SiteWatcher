using System.Net;
using System.Text.Json;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using SiteWatcher.Application.Common.Constants;

namespace SiteWatcher.WebAPI.Extensions;

public static partial class ExceptionHandlerExtensions
{
    private static readonly JsonSerializerOptions _opts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static void ConfigureGlobalExceptionHandlerMiddleware(this WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if(exceptionHandlerFeature is null)
                    return;

                var route = exceptionHandlerFeature.Path;
                var traceId = context.TraceIdentifier;
                var exception = exceptionHandlerFeature.Error;

                var logger = loggerFactory.CreateLogger("GlobalExceptionHandlerMiddleware");
                LogError(logger, exception, route, DateTime.UtcNow, exception.GetType().Name, exception.Message, traceId);

                object response;

                if(env.IsDevelopment())
                    response = new { Exception = ExceptionDevResponse.From(exception, traceId)};
                else
                    response = new []{ ApplicationErrors.INTERNAL_ERROR, $"traceId: {traceId}"};

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await JsonSerializer.SerializeAsync(context.Request.HttpContext.Response.Body, response, _opts);
            });
        });
    }

    [LoggerMessage(LogLevel.Error, "Exception occurred on Route: {Route} at {Date}, {Type}: {Msg}. TraceId: {TraceId}")]
    public static partial void LogError(ILogger logger, Exception ex, string route, DateTime date, string type,
        string msg, string traceId);
}