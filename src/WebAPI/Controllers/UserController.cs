using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.WebAPI.DTOs.ViewModels;
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
        var result = await _mediator.Send(registerUserCommand);

        if (!result.Success)
            return Conflict(response.AddMessages(result.Errors));

        return Created(string.Empty, response.SetResult(result.Value));
    }
}