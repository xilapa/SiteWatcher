using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.DTOs.InputModels;
using SiteWatcher.WebAPI.DTOs.Metadata;
using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleSettings _googleSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly ILogger<GoogleAuthController> _logger;
    private readonly ITokenService _tokenService;
    private readonly ICache _cache;

    // ctor for benchs
    protected GoogleAuthController() { }

    public GoogleAuthController(
        IGoogleSettings googleSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleAuthController> logger,
        IUserDapperRepository userDapperRepository,
        ITokenService tokenService,
        ICache cache)
    {
        _googleSettings = googleSettings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _userDapperRepository = userDapperRepository;
        _tokenService = tokenService;
        _cache = cache;
    }

    [HttpGet]
    [Route("login")]
    [Route("register")]
    public async Task<IActionResult> StartAuth([FromQuery] string? returnUrl = null)
    {
        Response.Cookies.Delete("returnUrl");

        if(!string.IsNullOrEmpty(returnUrl))
        {
            var cookieOptions = new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = false,
                MaxAge = TimeSpan.FromSeconds(90),
                HttpOnly = true
            };
            Response.Cookies.Append("returnUrl", returnUrl, cookieOptions);
        }

        var state = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveBytesAsync(state, _googleSettings.StateValue, TimeSpan.FromMinutes(5));

        var authUrl = $"{_googleSettings.AuthEndpoint}?scope={HttpUtility.UrlEncode(_googleSettings.Scopes)}&response_type=code&include_granted_scopes=false&state={state}&redirect_uri={HttpUtility.UrlEncode(_googleSettings.RedirectUri)}&client_id={_googleSettings.ClientId}";

        return Redirect(authUrl);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthCallBackData authData)
    {
        var response = new WebApiResponse<AuthenticationResult>();

        if(IsMissingScope(_googleSettings.Scopes, authData.Scope))
        {
            _logger.LogError("Invalid Scopes passed at {Date} \n Scopes: {Scopes}", DateTime.Now, authData.Scope);
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));
        }

        var storedState = await _cache.GetAndRemoveBytesAsync(authData.State!);
        if(!_googleSettings.StateValue.SequenceEqual(storedState ?? Array.Empty<byte>()))
        {
            _logger.LogError("Invalid State passed at {Date} \n State from request: {State} \nCached state {CachedState}", DateTime.Now, authData.State, storedState);
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));
        }

        var tokenResult = await ExchangeCode(authData.Code!);
        if(!tokenResult.Success)
            return BadRequest(response.AddMessages(WebApiErrors.GOOGLE_AUTH_ERROR));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenResult.IdToken);
        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        var user = await _userDapperRepository.GetActiveUserAsync(googleId);

        var authResult = new AuthenticationResult();

        if(user.Id == Guid.Empty)
        {
            var registerToken = _tokenService.GenerateRegisterToken(token.Claims, googleId);
            authResult.Set(EAuthTask.Register, registerToken);
        }
        else
        {
            var loginToken = _tokenService.GenerateLoginToken(user);
            authResult.Set(EAuthTask.Login, loginToken);
        }

        return Ok(response.SetResult(authResult));
    }

    protected static bool IsMissingScope(string defaultScopes, string? scopesToBeChecked)
    {
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
            sepIdx = scopeSpan[crrIdx..].IndexOf(' ');
            sepDist = sepIdx == -1 ? scopeSpan.Length - crrIdx : sepIdx;
        }

        return false;
    }

    private async Task<GoogleTokenResult> ExchangeCode(string code)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            code,
            client_id = _googleSettings.ClientId,
            client_secret = _googleSettings.ClientSecret,
            redirect_uri = _googleSettings.RedirectUri,
            grant_type = "authorization_code"
        };

        // TODO: Substituir chamada da api por Refit e adicionar Polly para retentativas
        using var response = await httpClient.PostAsJsonAsync(_googleSettings.TokenEndpoint, requestBody);
        if (!response.IsSuccessStatusCode){
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error on exchanging code at {Date}.\nErrorResponse: {Error}", DateTime.Now, error);
            return new GoogleTokenResult(success: false);
        }

        var tokenResult = await response.Content.ReadFromJsonAsync<GoogleTokenResult>();
        return tokenResult!;
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