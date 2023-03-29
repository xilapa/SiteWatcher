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
    [InlineData(AuthenticationDefaults.Schemes.Google, HttpStatusCode.Redirect)]
    [InlineData("invalid", HttpStatusCode.BadRequest)]
    [InlineData("", HttpStatusCode.NotFound)]
    [InlineData(null, HttpStatusCode.NotFound)]
    public async Task StartAuthWorks(string? schema, HttpStatusCode expectedHttpCode)
    {
        // act
        var res = await GetAsync($"auth/start/{schema}");

        // assert
        if (HttpStatusCode.Redirect.Equals(expectedHttpCode))
        {
            res.HttpResponse!.StatusCode.Should().Be(expectedHttpCode);
            res.HttpResponse.Headers.Location.Should().NotBeNull();
            return;
        }

        res.HttpResponse!.StatusCode
            .Should()
            .Be(expectedHttpCode);
    }
}