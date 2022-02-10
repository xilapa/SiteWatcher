using System;
using System.Net;
using System.Text.Json;
using AFA.Application.Constants;
using AFA.WebAPI.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AFA.WebAPI.Extensions;

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
                logger.LogError("Exception ocurred at {date}\n\tTraceId: {traceId}\n\t{type}: {ex}\n\tRoute: {routeData}", 
                    DateTime.Now, traceId, exception.GetType().Name, exception.Message, route);

                object response;

                if(!env.IsDevelopment())        
                    response = new { Exception = ExceptionDevResponse.From(exception, traceId)};
                else        
                    response = new WebApiResponse(null, ApplicationErrors.INTERNAL_ERROR, $"traceId: {traceId}");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await JsonSerializer.SerializeAsync(context.Request.HttpContext.Response.Body, response, opts);
            });
        });
    }
}