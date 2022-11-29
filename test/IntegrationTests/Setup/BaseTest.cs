using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.IntegrationTests.Setup;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.Setup;

public abstract class BaseTest
{
    private readonly BaseTestFixture _fixture;
    protected Mock<IEmailService> EmailServiceMock => _fixture.AppFactory.EmailServiceMock;
    public Mock<IHttpClientFactory> HttpClientFactoryMock => _fixture.AppFactory.HttpClientFactoryMock;
    protected FakeCache FakeCache => _fixture.AppFactory.FakeCache;
    protected DateTime CurrentTime
    {
        get => _fixture.AppFactory.CurrentTime;
        set => _fixture.AppFactory.CurrentTime = value;
    }
    protected IAppSettings TestSettings => _fixture.AppFactory.TestSettings;
    protected IGoogleSettings TestGoogleSettings => _fixture.AppFactory.TestGoogleSettings;
    protected ICustomWebApplicationFactory AppFactory => _fixture.AppFactory;

    protected BaseTest(BaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    protected string LoginAs(UserViewModel userViewModel)
    {
        var token = _fixture.AppFactory.AuthServiceForTokens.GenerateLoginToken(userViewModel);
        LoginWithToken(token);
        return token;
    }

    protected void LoginWithToken(string token) =>
        _fixture.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected string SetRegisterToken(UserViewModel userViewModel)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Name, userViewModel.Name),
            new(AuthenticationDefaults.ClaimTypes.Email, userViewModel.Email),
            new(AuthenticationDefaults.ClaimTypes.Locale, "en-US"),
        };
        var token = _fixture.AppFactory.AuthServiceForTokens.GenerateRegisterToken(claims, userViewModel.GetGoogleId());
        _fixture.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Utils.GetTokenPayload(token);
    }

    protected void RemoveLoginToken()
    {
        _fixture.Client.DefaultRequestHeaders.Authorization = null;
    }

    #region HttpClient Helper Methods

    protected async Task<HttpResult> GetAsync(string url, object? queryParams = null)
    {
        if (queryParams != null)
        {
            var urlStringBuilder = new StringBuilder(url);
            urlStringBuilder.Append('?');

            var propertiesArray = queryParams.GetType().GetProperties();
            for (int i = 0; i < propertiesArray.Length; i++)
            {
                urlStringBuilder.Append(propertiesArray[i].Name);
                urlStringBuilder.Append('=');
                urlStringBuilder.Append(propertiesArray[i].GetValue(queryParams) ?? string.Empty);
                if (i != propertiesArray.Length - 1)
                    urlStringBuilder.Append('&');
            }

            url = urlStringBuilder.ToString();
        }

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
}