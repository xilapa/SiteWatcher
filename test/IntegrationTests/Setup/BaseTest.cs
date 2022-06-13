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

    #region HttpClient Helper Methods

    protected async Task<(HttpResponseMessage, string?)> GetAsync(string url)
    {
        var response = await _fixture.Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await _fixture.Client.GetAsync(url);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<(HttpResponseMessage, string?)> PutAsync(string url)
    {
        var response = await _fixture.Client.PutAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    protected async Task<T?> PutAsync<T>(string url, T data)
    {
        var response = await _fixture.Client.PutAsync(url, Serialize(data));
        return await DeserializeAsync<T>(response);
    }

    protected async Task<(HttpResponseMessage, string?)> PostAsync(string url)
    {
        var response = await _fixture.Client.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    protected async Task<T?> PostAsync<T>(string url, T data)
    {
        var response = await _fixture.Client.PostAsync(url, Serialize(data));
        return await DeserializeAsync<T>(response);
    }

    protected async Task<(HttpResponseMessage, string?)> DeleteAsync(string url)
    {
        var response = await _fixture.Client.DeleteAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    protected async Task<T?> DeleteAsync<T>(string url)
    {
        var response = await _fixture.Client.DeleteAsync(url);
        return await DeserializeAsync<T>(response);
    }

    private static StringContent Serialize(object? data)
    {
        var jsonData = data is null ? string.Empty : JsonSerializer.Serialize(data);
        return new StringContent(jsonData, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage? response)
    {
        if (response is null)
            return default;
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(jsonResponse) ? default : JsonSerializer.Deserialize<T>(jsonResponse);
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

    protected async Task WithService<T>(Func<T, Task> func) where T : notnull
    {
        await using var scope = _fixture.AppFactory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        await func(service);
    }

    protected async Task<TResult> WithService<T, TResult>(Func<T, Task<TResult>> func) where T : notnull
    {
        await using var scope = _fixture.AppFactory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        var result = await func(service);
        return result;
    }
}