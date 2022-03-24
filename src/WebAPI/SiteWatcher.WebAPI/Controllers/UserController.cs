using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SiteWatcher.WebAPI.Constants;
using MediatR;
using SiteWatcher.Application.Commands;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Application.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ITokenService tokenService;
    
    public UserController(IMediator mediator, ITokenService tokenService)
    {
        this.mediator = mediator;
        this.tokenService = tokenService;
    }

    [HttpGet]
    public void ConfirmEmail()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = AuthenticationDefaults.RegisterScheme)]
    public async Task<IActionResult> Register(RegisterUserCommand registerUserCommand)
    {       
        var response = new WebApiResponse<string>();
        
        registerUserCommand.AuthEmail = User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        registerUserCommand.GoogleId = User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;

        if (registerUserCommand.AuthEmail is null || registerUserCommand.GoogleId is null)
            return BadRequest(response.AddMessages(ApplicationErrors.INVALID_REGISTER_DATA));

        var result = await mediator.Send(registerUserCommand);

        if (result.Success)
        {
            var authTokenPayload = HttpContext.GetAuthTokenPayload();
            await tokenService.InvalidateToken(authTokenPayload, ETokenPurpose.Register);
        }

        if (!result.Success)
            return Conflict(response.AddMessages(result.Errors));    

        return Created(string.Empty, response.SetResult(result.Value));
    }
}