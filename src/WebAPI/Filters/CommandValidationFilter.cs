using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Filters;

/// <summary>
/// Command must implement <see cref="IValidable"/> to be validated and it's name must be "command".
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
        var validableCommand = context.ActionArguments["command"] as IValidable;
        var validatorType = typeof(IValidator<>).MakeGenericType(validableCommand!.GetType());
        var validator = context.HttpContext.RequestServices.GetRequiredService(validatorType) as IValidator;

        var errs = await validableCommand.ValidateAsyncWith(validator!);

        return errs.Length == 0 ? Array.Empty<string>() : errs;
    }
}