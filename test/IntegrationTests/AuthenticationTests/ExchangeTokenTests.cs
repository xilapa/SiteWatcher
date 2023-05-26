using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
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
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AuthenticationTests;

public sealed class ExchangeTokenTestsBase : BaseTestFixture
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    internal const string XilapaProfilePic = "http://xilapa.io/profile.jpg";
    private const string _key = "key";
    internal string CookieKey = _key;
    internal readonly int ExpectedAuthResExpiration;

    public ExchangeTokenTestsBase()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        var authService = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        ExpectedAuthResExpiration = (authService
            .GetType()
            .GetField("AuthResExpiration", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(authService) as int?)!.Value;
    }

    public override Action<CustomWebApplicationOptions> Options => opts =>
    {
        // mock cookie auth
        _authServiceMock
            .Setup(s =>
                s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(CreateAuthenticateResult);

        opts.ReplaceService(typeof(IAuthenticationService), _authServiceMock.Object);

        opts.DatabaseType = DatabaseType.SqliteOnDisk;
    };

    private AuthenticateResult CreateAuthenticateResult()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Users.Xilapa.GetGoogleId()), // GoogleId
            new Claim(AuthenticationDefaults.ClaimTypes.ProfilePicUrl, XilapaProfilePic),
            new Claim(ClaimTypes.Email, Users.Xilapa.Email),
            new Claim(AuthenticationDefaults.ClaimTypes.Locale, "en-us"),
            new Claim(_key, CookieKey) // value used on exchange token
        };
        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationDefaults.Schemes.Google);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authTicket = new AuthenticationTicket(claimsPrincipal, AuthenticationDefaults.Schemes.Google);
        return AuthenticateResult.Success(authTicket);
    }
}

public class ExchangeTokenTests : BaseTest, IClassFixture<ExchangeTokenTestsBase>
{
    private readonly ExchangeTokenTestsBase _fixture;

    public ExchangeTokenTests(ExchangeTokenTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        FakeCache.Cache.Clear();
    }

    [Fact]
    public async Task GoogleAuthCallbackStoresAuthResult()
    {
        // arrange
        var partialRedirectUri = $"{TestSettings.FrontEndAuthUrl}?token=";

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
    public async Task ExchangeTokenWorks(bool delay, HttpStatusCode expectedHttpCode)
    {
        // arrange

        // get the auth res token
        var googleCallbackRes = await GetAsync("auth/google");
        var redirectUri = googleCallbackRes.HttpResponse!.Headers.Location!.AbsoluteUri;
        var token = redirectUri[(redirectUri.IndexOf("=", StringComparison.Ordinal) + 1)..];
        var exchangeTokenCmmd = new ExchangeCodeCommand { Token = token };

        // set the cookie key claim
        _fixture.CookieKey = FakeCache.Cache.First().Key;

        // token should not be valid after expiration
        if (delay) await Task.Delay(TimeSpan.FromSeconds(_fixture.ExpectedAuthResExpiration));

        // act
        var res = await PostAsync("auth/exchange-token", exchangeTokenCmmd);

        // assert
        res.HttpResponse!.StatusCode.Should().Be(expectedHttpCode);
        if (delay) return;
        var authRes = res.GetTyped<AuthenticationResult?>();
        authRes!.Task.Should().Be(AuthTask.Login);
        authRes.ProfilePicUrl.Should().Be(ExchangeTokenTestsBase.XilapaProfilePic);
        authRes.Token.Should().NotBeNull();
    }
}