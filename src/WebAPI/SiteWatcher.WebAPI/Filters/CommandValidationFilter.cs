using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SiteWatcher.Domain.Interfaces;
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

        foreach(var p in context.ActionDescriptor.Parameters.Where(p => p.BindingInfo.BindingSource == BindingSource.Body))
        {
            var command = context.ActionArguments[p.Name] as IValidable;
            if (command is not null)
                errors.AddRange(command.Validate());
        }

        if(errors.Any())
        {
            var result = new ObjectResult(new WebApiResponse<object>(null, errors))
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest
                            };
            context.Result = result;
        } 
    }
}