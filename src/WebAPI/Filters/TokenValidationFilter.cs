using System.Net;
using SiteWatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.WebAPI.Extensions;

namespace SiteWatcher.WebAPI.Filters;

public class TokenValidationFilter : IAsyncResourceFilter
{
    private readonly ITokenService _tokenService;

    public TokenValidationFilter(ITokenService tokenService) =>
        this._tokenService = tokenService;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var requestTokenPayload = context.HttpContext.GetAuthTokenPayload();
        if (string.Empty.Equals(requestTokenPayload))
        {
            await next();
            return;
        }

        var valid = await _tokenService.IsValid(requestTokenPayload);
        if(!valid)
        {
            var result = new ObjectResult(null) { StatusCode = (int)HttpStatusCode.Forbidden };
            context.Result = result;
            return;
        }

        await next();
    }
}