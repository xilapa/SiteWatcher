using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;

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
    public async Task Susbscribe(UserSubscribeIM userSubInput)
    {
        await this.userAppService.Subscribe(userSubInput);
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