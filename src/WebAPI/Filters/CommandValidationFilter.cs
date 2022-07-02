using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Filters;

/// <summary>
/// Command must implement <see cref="IValidable"/> to be validated.
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
        var requestParam = context.ActionDescriptor.Parameters
            .SingleOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

        if (requestParam is null || context.ActionArguments[requestParam.Name] is not IValidable command)
            return Array.Empty<string>();

        var serviceProvider = context.HttpContext.RequestServices;
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var obtainedValidator = serviceProvider.GetRequiredService(validatorType);
        if (obtainedValidator is not IValidator validator)
            throw new Exception($"The validator is not an {nameof(IValidator)}");

        var errs = await command.ValidateAsyncWith(validator);

        return errs.Length == 0 ? Array.Empty<string>() : errs;
    }
}