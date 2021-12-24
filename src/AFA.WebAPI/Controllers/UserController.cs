using System;
using Microsoft.AspNetCore.Mvc;

namespace AFA.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController 
{
    [HttpPost]
    [Route("Subscribe")]
    public void Susbscribe()
    {
        throw new NotImplementedException();
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