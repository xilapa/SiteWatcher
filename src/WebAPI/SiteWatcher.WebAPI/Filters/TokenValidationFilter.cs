using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.WebAPI.Extensions;

namespace SiteWatcher.WebAPI.Filters;

public class TokenValidationFilter : IAsyncResourceFilter
{
    private readonly ITokenService tokenService;
    public TokenValidationFilter(ITokenService tokenService) =>
        this.tokenService = tokenService;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var requestTokenPayload = context.HttpContext.GetAuthTokenPayload();
        if (requestTokenPayload is null)
        {
            await next();
            return;
        }       

        var valid = await tokenService.IsValid(requestTokenPayload);
        if(!valid)
        {
            var result = new ObjectResult(null) { StatusCode = (int)HttpStatusCode.Unauthorized };
            context.Result = result;
            return;
        }

        await next();
    }
}