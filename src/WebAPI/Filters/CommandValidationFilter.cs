using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Filters;

public class CommandValidationFilter : IAsyncActionFilter
{
    private readonly IValidatorFactory _validatorFactory;

    public CommandValidationFilter(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
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

    private async Task<string[]> ValidateInputAsync(ActionExecutingContext context)
    {
        var requestParam = context.ActionDescriptor.Parameters
            .SingleOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

        if (requestParam is null || context.ActionArguments[requestParam.Name] is not IValidable command)
            return Array.Empty<string>();

        var validator = _validatorFactory.GetValidator(command.GetType());
        var errs = await command.ValidateAsyncWith(validator as dynamic);

        return errs.Length == 0 ? Array.Empty<string>() : errs;
    }
}