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
using SiteWatcher.WebAPI.Extensions;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly GoogleSettings googleSettings;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IUserDapperRepository userDapperRepository;
    private readonly ILogger<GoogleAuthController> logger;
    private readonly ITokenService tokenService;
    private readonly ICache cache;

    // ctor for benchs
    protected GoogleAuthController() { }

    public GoogleAuthController(
        GoogleSettings googleSettings, 
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleAuthController> logger,
        IUserDapperRepository userDapperRepository,
        ITokenService tokenService,
        ICache cache)
    {
        this.googleSettings = googleSettings;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.userDapperRepository = userDapperRepository;
        this.tokenService = tokenService;
        this.cache = cache;
    }

    [HttpGet]
    [Route("login")]
    [Route("register")]
    public async Task<IActionResult> StartAuth([FromQuery] string returnUrl = null) 
    {        
        Response.Cookies.Delete("returnUrl");

        if(!string.IsNullOrEmpty(returnUrl))
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
 
        var state = HttpContext.GenerateStateFromRequest();
        await cache.SaveBytesAsync(state, googleSettings.StateValue, TimeSpan.FromMinutes(5));       

        var authUrl = $"{googleSettings.AuthEndpoint}?scope={HttpUtility.UrlEncode(googleSettings.Scopes)}&response_type=code&include_granted_scopes=false&state={state}&redirect_uri={HttpUtility.UrlEncode(googleSettings.RedirectUri)}&client_id={googleSettings.ClientId}";

        return Redirect(authUrl);
    }

    [HttpPost("authenticate")] 
    public async Task<IActionResult> Authenticate([FromBody] AuthCallBackData authData)
    {
        var response = new WebApiResponse<AuthenticationResult>();

        if(IsMissingScope(googleSettings.Scopes, authData.Scope))
        {
            logger.LogError("Invalid Scopes passed at {date} \n Scopes: {scopes}", DateTime.Now, authData.Scope);
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));
        }  

        var storedState = await cache.GetAndRemoveBytesAsync(authData.State);
        if(storedState is null || !storedState.SequenceEqual(googleSettings.StateValue))
        {
            logger.LogError("Invalid State passed at {date} \n State from request: {state} \nCached state {cachedState}", DateTime.Now, authData.State, storedState);
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));       
        }

        var tokenResult = await ExchangeCode(authData.Code);
        if(!tokenResult.Success)
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));
        
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenResult.IdToken);
        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        var user = await userDapperRepository.GetActiveUserAsync(googleId);

        var authResult = new AuthenticationResult();

        if(user.Id == Guid.Empty)
        {   
            var registerToken = tokenService.GenerateRegisterToken(token.Claims, googleId);
            authResult.Set(EAuthTask.Register, registerToken);
        }
        else
        {
            var loginToken = tokenService.GenerateLoginToken(user);
            authResult.Set(EAuthTask.Login, loginToken);
        }   

        return Ok(response.SetResult(authResult));
    }

    protected static bool IsMissingScope(string defaultScopes, string scopesToBeChecked)
    {
        if(string.IsNullOrEmpty(scopesToBeChecked))
            return true;

        var scopesToBeCheckedSpan = scopesToBeChecked.AsSpan();
        var scopeSpan = defaultScopes.AsSpan();

        var crrIdx = 0;
        var sepIdx = 0;
        var sepDist = scopeSpan.IndexOf(' ');

        while (true)
        {
            var scope = scopeSpan.Slice(crrIdx, sepDist);
            
            if (scopesToBeCheckedSpan.IndexOf(scope) == -1)
                return true;

            if (sepIdx == -1)
                break;

            crrIdx =  crrIdx + sepDist + 1;
            sepIdx = scopeSpan.Slice(crrIdx).IndexOf(' ');
            sepDist = sepIdx == -1 ? scopeSpan.Length - crrIdx : sepIdx;
        }

        return false;
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
                logger.LogError("Error on exchanging code at {date}.\nErrorResponse: {error}", DateTime.Now, error);
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