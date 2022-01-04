using System;
using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Domain.DTOS.InputModels;
using Microsoft.AspNetCore.Mvc;

namespace AFA.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController 
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