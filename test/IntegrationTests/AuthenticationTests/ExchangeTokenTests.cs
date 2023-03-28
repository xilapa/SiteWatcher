using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using Domain.Common.Services;
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
    internal string Unprotected = _key;

    public ExchangeTokenTestsBase()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
    }

    public override Action<CustomWebApplicationOptions> Options => opts =>
    {
        // mock cookie auth
        _authServiceMock
            .Setup(s =>
                s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(CreateAuthenticateResult);

        opts.ReplaceService(typeof(IAuthenticationService), _authServiceMock.Object);

        // mock data protection
        var dataProtectorMock = new Mock<IDataProtectorService>();
        dataProtectorMock
            .Setup(d => d.Protect(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(_key);

        dataProtectorMock
            .Setup(d => d.Unprotect(It.IsAny<string>()))
            .Returns(Unprotect);

        opts.ReplaceService(typeof(IDataProtectorService), dataProtectorMock.Object);

        opts.DatabaseType = DatabaseType.SqliteOnDisk;
    };

    private string Unprotect() => Unprotected;

    private AuthenticateResult CreateAuthenticateResult()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Users.Xilapa.GetGoogleId()), // GoogleId
            new Claim(AuthenticationDefaults.ClaimTypes.ProfilePicUrl, XilapaProfilePic),
            new Claim(ClaimTypes.Email, Users.Xilapa.Email),
            new Claim(AuthenticationDefaults.ClaimTypes.Locale, "en-us"),
            new Claim(_key, Unprotect()) // value used on exchange token
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

        var authService = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        var expectedExpiration = authService
            .GetType()
            .GetField("AuthResExpiration", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(authService) as int?;

        // act
        var res = await GetAsync("auth/google");

        // assert
        res.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.HttpResponse.Headers.Location!.AbsoluteUri.Should().Contain(partialRedirectUri);

        var cachedAuthRes = FakeCache.Cache.First().Value;
        cachedAuthRes.Expiration.Should().Be(TimeSpan.FromSeconds(expectedExpiration!.Value));

        var authRes = JsonSerializer.Deserialize<AuthenticationResult>((cachedAuthRes.Value as string)!);
        authRes!.Task.Should().Be(AuthTask.Login);
        authRes.ProfilePicUrl.Should().Be(ExchangeTokenTestsBase.XilapaProfilePic);
        authRes.Token.Should().NotBeNull();
    }

    [Fact]
    public async Task ExchangeTokenWorks()
    {
        // arrange

        // set the authRes on cache
        await GetAsync("auth/google");

        // set the authRes key as return of protector mock
        _fixture.Unprotected = FakeCache.Cache.First().Key;

        // get token from previous request
        var exchangeTokenCmmd = new ExchangeTokenCommand { Token = _fixture.Unprotected };

        // act
        var res = await PostAsync("auth/exchange-token", exchangeTokenCmmd);

        // assert
        res.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        var authRes = res.GetTyped<AuthenticationResult?>();
        authRes!.Task.Should().Be(AuthTask.Login);
        authRes.ProfilePicUrl.Should().Be(ExchangeTokenTestsBase.XilapaProfilePic);
        authRes.Token.Should().NotBeNull();
    }
}