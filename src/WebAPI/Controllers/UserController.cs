using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Application.Constants;
using SiteWatcher.Application.Users.Commands.RegisterUser;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;

    public UserController(IMediator mediator, ITokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    [HttpGet]
    [Route("confirm-email")]
    public void ConfirmEmail()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("register")]
    [Authorize(AuthenticationSchemes = AuthenticationDefaults.RegisterScheme)]
    public async Task<IActionResult> Register(RegisterUserCommand registerUserCommand)
    {
        var response = new WebApiResponse<string>();
        registerUserCommand.AuthEmail = User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        registerUserCommand.GoogleId = User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;

        if (registerUserCommand.AuthEmail is null || registerUserCommand.GoogleId is null)
            return BadRequest(response.AddMessages(ApplicationErrors.INVALID_REGISTER_DATA));

        var result = await _mediator.Send(registerUserCommand);

        if (result.Success)
        {
            var authTokenPayload = HttpContext.GetAuthTokenPayload();
            await _tokenService.InvalidateToken(authTokenPayload, ETokenPurpose.Register);
        }

        if (!result.Success)
            return Conflict(response.AddMessages(result.Errors));

        return Created(string.Empty, response.SetResult(result.Value));
    }
}