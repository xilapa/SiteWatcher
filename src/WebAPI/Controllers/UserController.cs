using SiteWatcher.Infra.Authorization.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
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