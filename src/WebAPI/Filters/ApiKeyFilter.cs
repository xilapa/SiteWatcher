using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IResourceFilter, IAllowAnonymous
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // Do nothing
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var settings = context.HttpContext.RequestServices.GetService<IAppSettings>();
        if(!context.HttpContext.Request.Headers.TryGetValue(settings!.ApiKeyName, out var key))
        {
            context.Result = new ContentResult { StatusCode = (int)HttpStatusCode.Unauthorized };
            return;
        }

        if(key != settings.ApiKey)
            context.Result = new ContentResult { StatusCode = (int)HttpStatusCode.Forbidden };
    }
}