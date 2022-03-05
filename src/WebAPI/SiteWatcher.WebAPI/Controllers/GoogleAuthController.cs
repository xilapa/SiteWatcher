using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.DTOs.InputModels;
using SiteWatcher.WebAPI.DTOs.Metadata;
using SiteWatcher.WebAPI.Settings;
using SiteWatcher.WebAPI.Constants;
using SiteWatcher.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SiteWatcher.WebAPI.Services;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleSettings googleSettings;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IUserRepository userRepository;
    private readonly ILogger<GoogleAuthController> logger;
    private readonly ITokenService tokenService;

    public GoogleAuthController(
        GoogleSettings googleSettings, 
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleAuthController> logger,
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        this.googleSettings = googleSettings;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.userRepository = userRepository;
        this.tokenService = tokenService;
    }

    [HttpGet]
    [Route("login")]
    [Route("register")]
    public IActionResult StartAuth([FromQuery] string returnUrl = null) 
    {        
        Response.Cookies.Delete("returnUrl");

        if(returnUrl is not null)
        {
            var cookieOptions = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = false,
                MaxAge = TimeSpan.FromSeconds(90)
            };
            Response.Cookies.Append("returnUrl", returnUrl, cookieOptions);
        }

        // TODO: Criar e salvar state no redis
        var state = "STATE";

        var authUrl = $"{googleSettings.AuthEndpoint}?scope={HttpUtility.UrlEncode(googleSettings.Scopes)}&response_type=code&include_granted_scopes=false&state={state}&redirect_uri={HttpUtility.UrlEncode(googleSettings.RedirectUri)}&client_id={googleSettings.ClientId}";

        return Redirect(authUrl);
    }

    [HttpPost]
    [HttpPost("authenticate")] 
    public async Task<IActionResult> Authenticate([FromBody] AuthCallBackData authData)
    {
        var response = new WebApiResponse<AuthenticationResult>();
        // TODO: checar state
       
        var scopesMissing = googleSettings.Scopes.Split(" ").Any(s => !authData.Scope.Contains(s));
        if(scopesMissing)        
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));        

        var tokenResult = await ExchangeCode(authData.Code);
        if(!tokenResult.Success)
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));
        
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenResult.IdToken);

        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        // TODO: get as notracking ou usar Dapper logo
        var user = await userRepository.GetAsync(u => u.GoogleId == googleId);

        if(user is null)
        {   
            var locale = token.Claims.DefaultIfEmpty(new Claim(AuthenticationDefaults.ClaimTypes.Locale, string.Empty))
                                     .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Locale).Value
                                     .Split("-").First();

            var localeClaim = new Claim(AuthenticationDefaults.ClaimTypes.Language, ((int)locale.GetEnumValue<ELanguage>()).ToString());

            var googleIdClaim = new Claim(AuthenticationDefaults.ClaimTypes.GoogleId, googleId);

            var registerToken = tokenService.GenerateRegisterToken(token.Claims, localeClaim, googleIdClaim);

            var authResult = new AuthenticationResult(EAuthTask.Register, registerToken);
            return Ok(response.SetResult(authResult));
        }       


        return Ok("Ok!");
    }

    private async Task<GoogleTokenResult> ExchangeCode(string code)
    {
        var httpClient = httpClientFactory.CreateClient();

        var requestBody = new
        {
            code,
            client_id = googleSettings.ClientId,
            client_secret = googleSettings.ClientSecret,
            redirect_uri = googleSettings.RedirectUri,
            grant_type = "authorization_code"
        };

        using(var response = await httpClient.PostAsJsonAsync(googleSettings.TokenEndpoint, requestBody))
        {
            if (!response.IsSuccessStatusCode){
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("Error on exchanging code.\nErrorResponse: {error}", error);
                return new GoogleTokenResult(success: false);
            }            

            var tokenResult = await response.Content.ReadFromJsonAsync<GoogleTokenResult>();
            return tokenResult;
        }
    }
    
    [HttpPost]
    [Route("logout")]
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
    [Route("logout-all-devices")]
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