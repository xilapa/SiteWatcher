using System.Net;
using SiteWatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Filters;

public class CommandValidationFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Do nothing
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var errors = new List<string>();

        foreach(var p in context.ActionDescriptor.Parameters.Where(p => p.BindingInfo?.BindingSource == BindingSource.Body))
        {
            if (context.ActionArguments[p.Name] is IValidable command)
                errors.AddRange(command.Validate());
        }

        if (errors.Count == 0)
            return;

        var result = new ObjectResult(new WebApiResponse<object>(null!, errors))
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };
        context.Result = result;
    }
}