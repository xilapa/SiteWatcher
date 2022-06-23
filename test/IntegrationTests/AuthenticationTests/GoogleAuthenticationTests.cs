using System.Net;
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
    public override Action<CustomWebApplicationOptions> Options =>
        opt => opt.DatabaseType = DatabaseType.SqliteOnDisk;
}

public class GoogleAuthenticationTests : BaseTest, IClassFixture<GoogleAuthenticationTestsBase>
{
    private readonly GoogleAuthenticationTestsBase _fixture;
    private readonly GoogleTokenMetadata _googleTokenMetadataResponse;

    public GoogleAuthenticationTests(GoogleAuthenticationTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        _googleTokenMetadataResponse = new GoogleTokenMetadata
            {IdToken = TokenUtils.GenerateFakeGoogleAuthToken(Users.Xilapa)};
    }

    [Fact]
    public async Task UserCanLoginWithValidCommand()
    {
        // Arrange
        _fixture.AppFactory.HttpClientFactoryMock.Invocations.Clear();

        // Setup the fake http response
        var fakehttpResponse = new FakeHttpResponse
        {StatusCode = HttpStatusCode.OK, Response = _googleTokenMetadataResponse};
        _fixture.AppFactory.HttpClientFactoryMock
            .Setup(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient))
            .Returns(new HttpClient(new FakeHttpDelegateHandler(fakehttpResponse)));

        // Save a fake state on cache
        FakeCache.Cache["state"] = new FakeCacheEntry(TestGoogleSettings.StateValue, TimeSpan.Zero);

        var command = new GoogleAuthenticationCommand
        {
            Scope = TestGoogleSettings.Scopes,
            State = "state",
            Code = "code"
        };

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
        _fixture.AppFactory.HttpClientFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Once);

        // State should be removed from cache
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task UserCantLoginWithInvalidCommand()
    {
        // Arrange
        _fixture.AppFactory.HttpClientFactoryMock.Invocations.Clear();
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
        _fixture.AppFactory.HttpClientFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Never);

        // State always should be removed from cache if invalid state was passed
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task UserCantLoginIfGoogleResponseIsInvalid()
    {
        // Arrange
        _fixture.AppFactory.HttpClientFactoryMock.Invocations.Clear();

        // Setup the fake http response
        var fakehttpResponse = new FakeHttpResponse {StatusCode = HttpStatusCode.BadRequest};
        _fixture.AppFactory.HttpClientFactoryMock
            .Setup(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient))
            .Returns(new HttpClient(new FakeHttpDelegateHandler(fakehttpResponse)));

        // Save a fake state on cache
        FakeCache.Cache["state"] = new FakeCacheEntry(TestGoogleSettings.StateValue, TimeSpan.Zero);

        var command = new GoogleAuthenticationCommand
        {
            Scope = TestGoogleSettings.Scopes,
            State = "state",
            Code = "code"
        };

        // Act
        var result = await PostAsync("google-auth/authenticate", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.BadRequest);

        var typedResult = result.GetTyped<WebApiResponse<AuthenticationResult>>();
        typedResult!.Result.Should().BeNull();
        typedResult.Messages
            .Should().BeEquivalentTo(ApplicationErrors.GOOGLE_AUTH_ERROR);

        // HttpFactory should be called one time
        _fixture.AppFactory.HttpClientFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Once);

        // State should be removed from cache
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task UserCanLoginEvenIfGoogleHasThreeTransientFailures()
    {
        // Arrange
        _fixture.AppFactory.HttpClientFactoryMock.Invocations.Clear();

        // Setup the fake http response
        var responses = new FakeHttpResponse[]
        {
            new() {StatusCode = HttpStatusCode.InternalServerError},
            new() {StatusCode = HttpStatusCode.RequestTimeout},
            new() {Exception = new HttpRequestException()},
            new() {StatusCode = HttpStatusCode.OK, Response = _googleTokenMetadataResponse}
        };

        _fixture.AppFactory.HttpClientFactoryMock
            .SetupSequence(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient))
            .Returns(new HttpClient(new FakeHttpDelegateHandler(responses)));

        // Save a fake state on cache
        FakeCache.Cache["state"] = new FakeCacheEntry(TestGoogleSettings.StateValue, TimeSpan.Zero);

        var command = new GoogleAuthenticationCommand
        {
            Scope = TestGoogleSettings.Scopes,
            State = "state",
            Code = "code"
        };

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
        _fixture.AppFactory.HttpClientFactoryMock
            .Verify(_ => _.CreateClient(AuthenticationDefaults.GoogleAuthClient), Times.Once);

        // State should be removed from cache
        FakeCache.Cache.Should().BeEmpty();
    }
}