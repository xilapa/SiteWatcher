using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Infra;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace IntegrationTests.Setup;

public abstract class BaseTest
{
    private readonly BaseTestFixture _fixture;
    public Mock<IEmailService> EmailServiceMock => _fixture.AppFactory.EmailServiceMock;
    protected FakeCache FakeCache => _fixture.AppFactory.FakeCache;
    protected DateTime CurrentTime
    {
        get => _fixture.AppFactory.CurrentTime;
        set => _fixture.AppFactory.CurrentTime = value;
    }
    protected IAppSettings TestSettings => _fixture.AppFactory.TestSettings;
    protected IGoogleSettings TestGoogleSettings => _fixture.AppFactory.TestGoogleSettings;

    protected BaseTest(BaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    protected void LoginAs(UserViewModel userViewModel)
    {
        var token = _fixture.AppFactory.AuthServiceForLogin.GenerateLoginToken(userViewModel);
        _fixture.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void RemoveLoginToken()
    {
        _fixture.Client.DefaultRequestHeaders.Authorization = null;
    }

    #region HttpClient Helper Methods

    protected async Task<HttpResult> GetAsync(string url)
    {
        var response = await _fixture.Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var result = new HttpResult(response, content);
        return result;
    }

    protected async Task<HttpResult> PutAsync(string url, object? data = null)
    {
        var response = await _fixture.Client.PutAsync(url, Serialize(data));
        var content = await response.Content.ReadAsStringAsync();
        var result = new HttpResult(response, content);
        return result;
    }

    protected async Task<HttpResult> PostAsync(string url, object? data = null)
    {
        var response = await _fixture.Client.PostAsync(url, Serialize(data));
        var content = await response.Content.ReadAsStringAsync();
        var result = new HttpResult(response, content);
        return result;
    }

    protected async Task<HttpResult> DeleteAsync(string url)
    {
        var response = await _fixture.Client.DeleteAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var result = new HttpResult(response, content);
        return result;
    }

    private static StringContent Serialize(object? data)
    {
        var jsonData = data is null ? string.Empty : JsonSerializer.Serialize(data);
        return new StringContent(jsonData, Encoding.UTF8, "application/json");
    }

    #endregion

    protected async Task WithDbContext(Func<SiteWatcherContext, Task> action)
    {
        await using var context = _fixture.AppFactory.GetContext();
        await action(context);
    }

    protected async Task<T> WithDbContext<T>(Func<SiteWatcherContext, Task<T>> action)
    {
        await using var context = _fixture.AppFactory.GetContext();
        var result = await action(context);
        return result;
    }

    protected async Task WithServiceProvider(Func<IServiceProvider, Task> func)
    {
        await using var scope = _fixture.AppFactory.Services.CreateAsyncScope();
        await func(scope.ServiceProvider);
    }

    protected async Task<T> WithServiceProvider<T>(Func<IServiceProvider, Task<T>> func)
    {
        await using var scope = _fixture.AppFactory.Services.CreateAsyncScope();
        var result = await func(scope.ServiceProvider);
        return result;
    }
}