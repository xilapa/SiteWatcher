using System.Net;
using System.Text.Json;
using SiteWatcher.Application.Constants;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using Microsoft.AspNetCore.Diagnostics;

namespace SiteWatcher.WebAPI.Extensions;

public static class EceptionHandlerExtensions
{
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
                logger.LogError("Exception ocurred at {date}\n\t{type}: {msg}\n\tRoute: {route}\n\tTraceId: {traceId}\n\n\tStackTrace: {stackTrace}", 
                    DateTime.Now, exception.GetType().Name, exception.Message, route, traceId, exception.StackTrace);

                object response;

                if(env.IsDevelopment())        
                    response = new { Exception = ExceptionDevResponse.From(exception, traceId)};
                else        
                    response = new WebApiResponse<object>(null, ApplicationErrors.INTERNAL_ERROR, $"traceId: {traceId}");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await JsonSerializer.SerializeAsync(context.Request.HttpContext.Response.Body, response, opts);
            });
        });
    }
}