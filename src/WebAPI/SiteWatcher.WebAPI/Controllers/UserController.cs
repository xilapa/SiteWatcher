using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.WebAPI.DTOs;
using System.Net;
using AFA.Domain.Enums;

namespace AFA.WebAPI.Controllers;

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
}