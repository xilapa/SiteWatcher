using System;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]/[action]")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleSettings googleSettings;

    public GoogleAuthController(GoogleSettings googleSettings)
    {
        this.googleSettings = googleSettings;
    }

    [HttpGet]
    public IActionResult LoginWithGoogle([FromQuery] string returnUrl = null) 
    {
        AddReturnUrlCookie(returnUrl);
        var redirectUrl = Url.ActionLink(nameof(GoogleLoginCallBack));
        return StartGoogleAuth(redirectUrl);
    }

    [HttpGet]
    public IActionResult RegisterWithGoogle([FromQuery] string returnUrl = null) 
    {
        AddReturnUrlCookie(returnUrl);
        var redirectUrl = Url.ActionLink(nameof(GoogleRegisterCallBack));
        return StartGoogleAuth(redirectUrl);
    }

    private IActionResult StartGoogleAuth(string redirect)
    {
        // TODO: Criar e salvar state no redis
        var state = "STATE";

        var authUrl = $"{googleSettings.AuthEndpoint}?scope={googleSettings.Scopes}&response_type=code&include_granted_scopes=false&state={state}&redirect_uri={HttpUtility.UrlEncode(redirect)}&client_id={googleSettings.ClientId}";

        return Redirect(authUrl);
    }

    private void AddReturnUrlCookie(string returnUrl)
    {
        if(returnUrl is null)
        {
            Response.Cookies.Delete("returnUrl");
            return;
        }

        var cookieOptions = new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = false,
            MaxAge = TimeSpan.FromSeconds(90)
        };

        Response.Cookies.Append("returnUrl", returnUrl, cookieOptions);
    }

    [HttpGet]
    public IActionResult GoogleLoginCallBack(string state, string code, string scope, string authuser, string prompt)
    {
        // TODO: checar se scopes são os mesmos solicitados
        // TODO: checar state
        throw new NotImplementedException();

    }

    [HttpGet]
    public IActionResult GoogleRegisterCallBack(string state, string code, string scope, string authuser, string prompt)
    {
        // TODO: checar se scopes são os mesmos solicitados
        // TODO: checar state
        throw new NotImplementedException();

    }

    [HttpPost]
    public void Logout() 
    {
        /* 
            TODO: inserir token atual na blacklist do redis e sua validade na blacklist deve ser o tempo de expiração do token
            Método de blacklist deve ser implementado em outra classe
            Pois deve ser chamado quando:
            - Quando usuário deletar conta;
            - Quando usuário mudar senha do google? Como fazer? Verificar sempre a validade do token do google
            - Quando for deslogado pelo admin;

            o token do google tbm deve ser revogado

            Criar filtro para recusar qualquer token na blacklist ou access_token do google invalido (checar se ele é valido na user info)
        */
        throw new NotImplementedException();
    }

}
