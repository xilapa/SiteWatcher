using System.Net;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Setup;
using Moq;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.GoogleAuth;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AuthenticationTests;

public class GoogleAuthenticationTestsBase : BaseTestFixture
{
    public Mock<IHttpClientFactory> HttpFactoryMock;
    public override Action<CustomWebApplicationOptions>? Options =>
        opt =>
        {
            opt.DatabaseType = DatabaseType.SqliteOnDisk;

            var googleTokenMetadata = new GoogleTokenMetadata
                {IdToken = TokenUtils.GenerateFakeGoogleAuthToken(Users.Xilapa)};
            var stringContent = new StringContent(JsonSerializer.Serialize(googleTokenMetadata));

            var httpDelegateHandler = new FakeHttpDelegateHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {Content = stringContent})
            );

            HttpFactoryMock = new Mock<IHttpClientFactory>();
            HttpFactoryMock.Setup(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient))
                .Returns(new HttpClient(httpDelegateHandler));

            opt.ReplaceService(typeof(IHttpClientFactory),HttpFactoryMock.Object);
        };
}

public class GoogleAuthenticationTests : BaseTest, IClassFixture<GoogleAuthenticationTestsBase>
{
    private readonly GoogleAuthenticationTestsBase _fixture;

    public GoogleAuthenticationTests(GoogleAuthenticationTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UserCanLoginWithValidCommand()
    {
        // Arrange
        var command = new GoogleAuthenticationCommand
        {
            Scope = TestGoogleSettings.Scopes,
            State = "state",
            Code = "code"
        };

        FakeCache.Cache["state"] = new FakeCacheEntry(TestGoogleSettings.StateValue, TimeSpan.Zero);

        // Act
        var result = await PostAsync("google-auth/authenticate", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.OK);

        var typedResult = result.GetTyped<WebApiResponse<AuthenticationResult>>();
        typedResult!.Result!.Task
            .Should().Be(EAuthTask.Login);
        typedResult.Result!.Token
            .Should().NotBeEmpty();

        // HttpFactory should be called one time
        _fixture.HttpFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Once);

        // State should be removed from cache
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task UserCantLoginWithInvalidCommand()
    {
        // Arrange
        _fixture.HttpFactoryMock.Invocations.Clear();
        var command = new GoogleAuthenticationCommand { State = "INVALID_STATE"};

        FakeCache.Cache["INVALID_STATE"] = new FakeCacheEntry(TestGoogleSettings.StateValue, TimeSpan.Zero);

        // Act
        var result = await PostAsync("google-auth/authenticate", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.BadRequest);

        var typedResult = result.GetTyped<WebApiResponse<AuthenticationResult>>();
        typedResult!.Result.Should().BeNull();

        typedResult.Messages
            .Should().BeEquivalentTo(new[]
            {
                ApplicationErrors.GOOGLE_AUTH_ERROR,
                ApplicationErrors.GOOGLE_AUTH_ERROR
            });

        // HttpFactory should not be called
        _fixture.HttpFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Never);

        // State always should be removed from cache if invalid state was passed
        FakeCache.Cache.Should().BeEmpty();
    }
}