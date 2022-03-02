using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SiteWatcher.WebAPI.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{

    
    public UserController(IUserAppService userAppService)
    {

    }



    [HttpGet]
    [Route("confirm-email")]
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