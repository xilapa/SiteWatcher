using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Filters;

/// <summary>
/// Command must be named "command" and have an <see cref="AbstractValidor"/> implementation to be validated.
/// </summary>
public class CommandValidationFilter : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errs = await ValidateInputAsync(context);
        if (errs.Length == 0)
        {
            await next();
            return;
        }

        var result = new ObjectResult(new WebApiResponse<object>(null!, errs))
        {
            StatusCode = (int) HttpStatusCode.BadRequest
        };
        context.Result = result;
    }

    private static async Task<string[]> ValidateInputAsync(ActionExecutingContext context)
    {
        var command = context.ActionArguments["command"];
        var validatorType = typeof(IValidator<>).MakeGenericType(command!.GetType());

        var validator = context.HttpContext.RequestServices.GetRequiredService(validatorType) as IValidator;
        var result = await validator!.ValidateAsync(new ValidationContext<dynamic>(command));

        return result.IsValid ? Array.Empty<string>(): result.Errors.Select(err => err.ErrorMessage).ToArray();
    }
}