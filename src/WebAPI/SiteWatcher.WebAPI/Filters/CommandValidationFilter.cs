using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SiteWatcher.Application.Validators;
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
            var validationResult = Validator.Validate(context.ActionArguments[p.Name], p.ParameterType);
                                        
            if(!validationResult.IsValid)
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage)); 
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