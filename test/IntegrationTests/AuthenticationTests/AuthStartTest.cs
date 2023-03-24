using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using SiteWatcher.Infra.Authorization.Constants;

namespace IntegrationTests.AuthenticationTests;

public sealed class AuthStartTest : BaseTest, IClassFixture<BaseTestFixture>
{
    public AuthStartTest(BaseTestFixture fixture) : base(fixture)
    { }

    [Theory]
    [InlineData(AuthenticationDefaults.Schemes.Google, true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task StartAuthWorks(string? schema, bool redirects)
    {
        // act
        var res = await GetAsync($"auth/start/{schema}");

        // assert
        if (redirects)
        {
            res.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.Redirect);
            res.HttpResponse.Headers.Location.Should().NotBeNull();
            return;
        }

        res.HttpResponse!.StatusCode
            .Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }
}