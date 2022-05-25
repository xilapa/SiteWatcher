using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Filters;

public class TokenValidationFilter : IAsyncResourceFilter
{
    private readonly ISessao _sessao;
    private readonly ITokenService _tokenService;

    public TokenValidationFilter(ISessao sessao, ITokenService tokenService)
    {
        _sessao = sessao;
        _tokenService = tokenService;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (string.Empty.Equals(_sessao.AuthTokenPayload))
        {
            await next();
            return;
        }

        var valid = await _tokenService.IsValid(_sessao.AuthTokenPayload);
        if(!valid)
        {
            var result = new ObjectResult(null) { StatusCode = (int)HttpStatusCode.Forbidden };
            context.Result = result;
            return;
        }

        await next();
    }
}