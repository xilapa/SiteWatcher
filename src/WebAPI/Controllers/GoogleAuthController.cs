using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.DTOs.InputModels;
using SiteWatcher.WebAPI.DTOs.Metadata;
using System.IdentityModel.Tokens.Jwt;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("google-auth")]
//TODO: Melhorar essa controller, criar handlers para cada método
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleSettings _googleSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserDapperRepository _userDapperRepository;
    private readonly ILogger<GoogleAuthController> _logger;
    private readonly IAuthService _authService;
    private readonly ICache _cache;

    // ctor for benchs
#pragma warning disable CS8618
    protected GoogleAuthController() { }
#pragma warning restore CS8618

    public GoogleAuthController(
        IGoogleSettings googleSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleAuthController> logger,
        IUserDapperRepository userDapperRepository,
        IAuthService authService,
        ICache cache)
    {
        _googleSettings = googleSettings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _userDapperRepository = userDapperRepository;
        _authService = authService;
        _cache = cache;
    }

    [HttpGet]
    [Route("login")]
    [Route("register")]
    public async Task<IActionResult> StartAuth([FromQuery] string? returnUrl = null)
    {
        var state = await _authService.GenerateLoginState(_googleSettings.StateValue);
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
        var profilePic = token.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.Google.Picture)?.Value;
        var user = await _userDapperRepository.GetActiveUserAsync(googleId);

        var authResult = new AuthenticationResult();

        if(user.UserId.Equals(UserId.Empty))
        {
            var registerToken = _authService.GenerateRegisterToken(token.Claims, googleId);
            authResult.Set(EAuthTask.Register, registerToken, profilePic);
        }
        else
        {
            var loginToken = _authService.GenerateLoginToken(user);
            await _authService.WhiteListToken(user.UserId, loginToken);
            authResult.Set(EAuthTask.Login, loginToken, profilePic);
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
}