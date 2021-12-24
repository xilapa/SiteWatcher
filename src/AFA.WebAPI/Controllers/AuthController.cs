using System;
using Microsoft.AspNetCore.Mvc;

namespace AFA.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController
{

    [HttpPost]
    [Route("Login")]
    public void Login() 
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("Logout")]
    public void Logout() 
    {
        throw new NotImplementedException();
    }

}
