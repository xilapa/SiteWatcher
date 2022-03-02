using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.DTOs.InputModels;
using System.Net;
using SiteWatcher.Domain.Enums;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using Microsoft.AspNetCore.Authorization;
using SiteWatcher.WebAPI.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserAppService userAppService;
    
    public UserController(IUserAppService userAppService)
    {
        this.userAppService = userAppService;
    }

    [HttpPost]
    [Route("Subscribe")]
    public async Task<ActionResult<WebApiResponse>> Susbscribe(UserSubscribeIM userSubInput)
    {
        var appResponse = await this.userAppService.Subscribe(userSubInput);

        var response = new ObjectResult(new WebApiResponse(appResponse))
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        if (appResponse.Success && appResponse.InternalResult == ESubscriptionResult.SubscribedSuccessfully)
            response.StatusCode = (int)HttpStatusCode.Created;
        
        if(appResponse.Success && appResponse.InternalResult == ESubscriptionResult.AlreadySubscribed)
            response.StatusCode = (int)HttpStatusCode.Redirect;

        return response;
    }

    [HttpPost]
    [Route("Unsubscribe")]
    public void Unsubscribe()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("ConfirmEmail")]
    public void ConfirmEmail()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("register")]
    [Authorize(AuthenticationSchemes = AuthenticationDefaults.RegisterScheme)]
    public IActionResult Register()
    {
        return Ok("autenticado com token de registro");
    }
}