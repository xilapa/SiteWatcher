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

    public ExchangeTokenTestsBase()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
    }

    public override Action<CustomWebApplicationOptions> Options => opts =>
    {
        // mock google cookie signin
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
        var authenticateResult = AuthenticateResult.Success(authTicket);

        _authServiceMock
            .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), AuthenticationDefaults.Schemes.Google))
            .ReturnsAsync(authenticateResult);

        opts.ReplaceService(typeof(IAuthenticationService), _authServiceMock.Object);

        opts.DatabaseType = DatabaseType.SqliteOnDisk;
    };
}

public class ExchangeTokenTests : BaseTest, IClassFixture<ExchangeTokenTestsBase>
{
    public ExchangeTokenTests(ExchangeTokenTestsBase fixture) : base(fixture)
    {
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
}