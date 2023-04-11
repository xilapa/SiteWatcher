using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Services;
using ISession = SiteWatcher.Domain.Authentication.ISession;

namespace SiteWatcher.WebAPI.Filters.Cache;

/// <summary>
/// To cache the result, the method must have an parameter implementing <see cref="ICacheable"/>
/// and it's name must be "command".
/// Also a cache header will be set on response, to cache the result on client for 30s.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CacheFilterAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter
{
    private const string CacheInfoKey = nameof(CacheInfoKey);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheableCommand = context.ActionArguments["command"] as ICacheable;

        var cache = context.HttpContext.RequestServices.GetRequiredService<ICache>();
        var session = context.HttpContext.RequestServices.GetRequiredService<ISession>();
        var key = cacheableCommand!.GetKey(session);
        // check if value is on cache
        var cachedData = await cache.GetHashFieldAsStringAsync(key, cacheableCommand.HashFieldName);
        if (cachedData is null)
        {
            // save caching info on request
            var cacheInfo = new CacheInfo
            {
                Key = key,
                HashFieldName = cacheableCommand.HashFieldName,
                Expiration = cacheableCommand.Expiration
            };
            context.HttpContext.Items[CacheInfoKey] = cacheInfo;
            await next();
            return;
        }

        var result = new ObjectResult(cachedData)
        {
            StatusCode = (int) HttpStatusCode.OK
        };

        context.Result = result;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var hasCacheInfo = context.HttpContext.Items.TryGetValue(CacheInfoKey, out var cacheInfoObj);

        // add value to cache if caching info exists
        if (hasCacheInfo)
        {
            var cacheInfo = cacheInfoObj as CacheInfo;
            var cache = context.HttpContext.RequestServices.GetRequiredService<ICache>();
            await cache
                .SaveHashAsync(cacheInfo!.Key, cacheInfo.HashFieldName,
                    (context.Result as ObjectResult)!.Value!, cacheInfo.Expiration);
        }

        // 30s cache on client, based on official ResponseCacheFilterExecutor
        // https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Filters/ResponseCacheFilterExecutor.cs
        context.HttpContext.Response.Headers.CacheControl = "private,max-age=30";
        await next();
    }
}