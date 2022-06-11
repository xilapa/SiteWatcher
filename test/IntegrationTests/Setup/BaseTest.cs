using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Infra;
using SiteWatcher.IntegrationTests.Setup;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.WebAPI;

namespace IntegrationTests.Setup;

public class BaseTest : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Startup> _appFactory;
    private readonly HttpClient _client;
    public Mock<IEmailService> EmailServiceMock => _appFactory.EmailServiceMock;
    public FakeCache FakeCache => _appFactory.FakeCache;
    public DateTime CurrentTime
    {
        get => _appFactory.CurrentTime;
        set => _appFactory.CurrentTime = value;
    }
    public IAppSettings TestSettings => _appFactory.TestSettings;
    public IGoogleSettings TestGoogleSettings => _appFactory.TestGoogleSettings;

    public BaseTest(Action<CustomWebApplicationOptions>? options = null)
    {
        _appFactory = new CustomWebApplicationFactory<Startup>(options);
        _client = _appFactory.CreateClient(new WebApplicationFactoryClientOptions {AllowAutoRedirect = false});
    }

    public async Task InitializeAsync()
    {
        await WithDbContext(async ctx =>
        {
            await ctx.Database.EnsureCreatedAsync();
            await Database.SeedDatabase(ctx, _appFactory.CurrentTime);
        });
    }

    public async Task LoginAs(UserViewModel userViewModel)
    {
        var authService = await _appFactory.GetAuthService();
        var token = authService.GenerateLoginToken(userViewModel);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #region HttpClient Helper Methods

    public async Task<(HttpResponseMessage, string?)> GetAsync(string url)
    {
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await _client.GetAsync(url);
        return await DeserializeAsync<T>(response);
    }

    public async Task<(HttpResponseMessage, string?)> PutAsync(string url)
    {
        var response = await _client.PutAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    public async Task<T?> PutAsync<T>(string url, T data)
    {
        var response = await _client.PutAsync(url, Serialize(data));
        return await DeserializeAsync<T>(response);
    }

    public async Task<(HttpResponseMessage, string?)> PostAsync(string url)
    {
        var response = await _client.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    public async Task<T?> PostAsync<T>(string url, T data)
    {
        var response = await _client.PostAsync(url, Serialize(data));
        return await DeserializeAsync<T>(response);
    }

    public async Task<(HttpResponseMessage, string?)> DeleteAsync(string url)
    {
        var response = await _client.DeleteAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    public async Task<T?> DeleteAsync<T>(string url)
    {
        var response = await _client.DeleteAsync(url);
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

    public async Task WithDbContext(Func<SiteWatcherContext, Task> action)
    {
        await using var context = _appFactory.GetContext();
        await action(context);
    }

    public async Task<T> WithDbContext<T>(Func<SiteWatcherContext, Task<T>> action)
    {
        await using var context = _appFactory.GetContext();
        var result = await action(context);
        return result;
    }

    public async Task DisposeAsync()
    {
        _client?.CancelPendingRequests();
        _client?.Dispose();
        await _appFactory.DisposeAsync();
    }
}