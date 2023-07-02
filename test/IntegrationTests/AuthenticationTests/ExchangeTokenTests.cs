using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using SiteWatcher.Application.Authentication.Commands.ExchangeToken;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AuthenticationTests;

public sealed class ExchangeTokenTestsBase : BaseTestFixture
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    internal const string XilapaProfilePic = "http://xilapa.io/profile.jpg";
    internal readonly int ExpectedAuthResExpiration;
    internal const string CodeVerifier = "codeverifier";
    internal readonly string CodeChallenge;

    public ExchangeTokenTestsBase()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        var authService = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        ExpectedAuthResExpiration = (authService
            .GetType()
            .GetField("AuthResExpiration", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(authService) as int?)!.Value;

        // generate code challenge
        var codeVerifierHash = SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier));
        CodeChallenge = Convert.ToBase64String(codeVerifierHash);
    }

    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        // mock microsoft's auth service
        _authServiceMock
            .Setup(s =>
                s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(CreateAuthenticateResult);

        optionsBuilder.ReplaceService(typeof(IAuthenticationService), _authServiceMock.Object);

        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }

    private AuthenticateResult CreateAuthenticateResult()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Users.Xilapa.GetGoogleId()), // GoogleId
            new Claim(AuthenticationDefaults.ClaimTypes.ProfilePicUrl, XilapaProfilePic),
            new Claim(ClaimTypes.Email, Users.Xilapa.Email),
            new Claim(AuthenticationDefaults.ClaimTypes.Locale, "en-us")
        };
        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationDefaults.Schemes.Google);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authTicket = new AuthenticationTicket(claimsPrincipal, AuthenticationDefaults.Schemes.Google);
        authTicket.Properties.Items.Add(AuthenticationDefaults.CodeChallengeKey, CodeChallenge);
        return AuthenticateResult.Success(authTicket);
    }
}

public class ExchangeTokenTests : BaseTest, IClassFixture<ExchangeTokenTestsBase>
{
    private readonly ExchangeTokenTestsBase _fixture;
    private const string Code = "?code=";

    public ExchangeTokenTests(ExchangeTokenTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        FakeCache.Cache.Clear();
    }

    [Fact]
    public async Task GoogleAuthCallbackStoresAuthResult()
    {
        // arrange
        var partialRedirectUri = $"{TestSettings.FrontEndAuthUrl}?code=";

        // act
        var res = await GetAsync("auth/google");

        // assert
        res.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.HttpResponse.Headers.Location!.AbsoluteUri.Should().Contain(partialRedirectUri);

        var cachedAuthRes = FakeCache.Cache.First().Value;
        cachedAuthRes.Expiration.Should().Be(TimeSpan.FromSeconds(_fixture.ExpectedAuthResExpiration));

        var authRes = JsonSerializer.Deserialize<AuthenticationResult>((cachedAuthRes.Value as string)!);
        authRes!.Task.Should().Be(AuthTask.Login);
        authRes.ProfilePicUrl.Should().Be(ExchangeTokenTestsBase.XilapaProfilePic);
        authRes.Token.Should().NotBeNull();
    }

    [Theory]
    [InlineData(false, HttpStatusCode.OK)]
    [InlineData(true, HttpStatusCode.Unauthorized)]
    public async Task ExchangeCodeWorks(bool delay, HttpStatusCode expectedHttpCode)
    {
        // arrange

        // get the authentication code
        var googleCallbackRes = await GetAsync("auth/google");
        var redirectUri = googleCallbackRes.HttpResponse!.Headers.Location!.AbsoluteUri;
        var code = redirectUri[(redirectUri.IndexOf(Code, StringComparison.Ordinal) + Code.Length)..];
        var exchangeCodeCmmd = new ExchangeCodeCommand { Code = code, CodeVerifier = ExchangeTokenTestsBase.CodeVerifier};

        // token should not be valid after expiration
        if (delay) await Task.Delay(TimeSpan.FromSeconds(_fixture.ExpectedAuthResExpiration));

        // act
        var res = await PostAsync("auth/exchange-code", exchangeCodeCmmd);

        // assert
        res.HttpResponse!.StatusCode.Should().Be(expectedHttpCode);
        if (delay) return;
        var authRes = res.GetTyped<AuthenticationResult?>();
        authRes!.Task.Should().Be(AuthTask.Login);
        authRes.ProfilePicUrl.Should().Be(ExchangeTokenTestsBase.XilapaProfilePic);
        authRes.Token.Should().NotBeNull();
    }
}