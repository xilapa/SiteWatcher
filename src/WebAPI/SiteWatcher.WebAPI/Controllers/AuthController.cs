using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.DTOs;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]/[action]")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleSettings googleSettings;
    private readonly IHttpClientFactory httpClientFactory;

    public GoogleAuthController(GoogleSettings googleSettings, IHttpClientFactory httpClientFactory)
    {
        this.googleSettings = googleSettings;
        this.httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login([FromQuery] string returnUrl = null) 
    {
        AddReturnUrlCookie(returnUrl);
        var redirectUrl = Url.ActionLink(nameof(LoginCallBack));
        return StartAuth(redirectUrl);
    }

    [HttpGet]
    public IActionResult Register([FromQuery] string returnUrl = null) 
    {
        AddReturnUrlCookie(returnUrl);
        var redirectUrl = Url.ActionLink(nameof(RegisterCallBack));
        return StartAuth(redirectUrl);
    }

    private IActionResult StartAuth(string redirect)
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
    public async Task<IActionResult> LoginCallBack(string state, string code, string scope, string authuser, string prompt)
    {
        // TODO: checar se scopes são os mesmos solicitados
        // TODO: checar state
        // throw new NotImplementedException();
        await ExchangeCode(code, Url.ActionLink(nameof(LoginCallBack)));
        return Ok("");
    }

    [HttpGet]
    public IActionResult RegisterCallBack(string state, string code, string scope, string authuser, string prompt)
    {
        // TODO: checar se scopes são os mesmos solicitados
        // TODO: checar state
        throw new NotImplementedException();
    }

    private async Task<GoogleTokenResult> ExchangeCode(string code, string actualRedirectUri)
    {
        var httpClient = httpClientFactory.CreateClient();

        var requestBody = new
        {
            code,
            client_id = googleSettings.ClientId,
            client_secret = googleSettings.ClientSecret,
            redirect_uri = actualRedirectUri,
            grant_type = "authorization_code"
        };

        using(var response = await httpClient.PostAsJsonAsync(googleSettings.TokenEndpoint, requestBody))
        {
            if (!response.IsSuccessStatusCode)
                return new GoogleTokenResult(success: false);

            var tokenResult = await response.Content.ReadFromJsonAsync<GoogleTokenResult>();
            return tokenResult;
        }
    }

    [HttpPost]
    public void Logout() 
    {
        /* 
            TODO: inserir token atual na blacklist do redis e sua validade na blacklist deve ser o tempo de expiração do token.
            Método de blacklist deve ser implementado em outra classe, aceitando e/ou token ou id de usuário
            Pois deve ser chamado quando:
            - Quando usuário deletar conta;
            - Quando for deslogado pelo admin;
            - Quando usuário deslogar;
            - Quando usuário deslogar de todos os dispositivos;

            TODO: Criar filtro para recusar qualquer token na blacklist.
            TODO: No front deve have HttpInterceptor para resultados 401 - Unauthorized para que o token seja removido.
        */
        throw new NotImplementedException();
    }


    [HttpPost]
    public void LogoutOfAllDevices()
    {
        /*
            TODO: Inserir token atual na blacklist e Id do usuário também pelo tempo de expiração do token.

            TODO: Criar whitelist de tokens no redis
            Caso usuário deslogue de todos os dispositivos e queira logar novamente,
            o novo token deve ir para a whitelist para que o usuário não seja barrado até a expiração da blacklist.
            Token deve ser adicionado a whitelist caso usuário tenha seu Id na blacklist.

            TODO: Criar filtro para verificar se usuário esta na blacklist e caso ele tenha um token na white list: Liberar acesso;
        */
        throw new NotImplementedException();
    }

}