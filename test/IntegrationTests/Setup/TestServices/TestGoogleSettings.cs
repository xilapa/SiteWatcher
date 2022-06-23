using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestGoogleSettings : IGoogleSettings
{
    public TestGoogleSettings()
    {
        ClientId  = "GoogleClientId";
        ClientSecret = "GoogleClientSecret";
        AuthEndpoint = "GoogleAuthEndpoint";
        TokenEndpoint = "https://faketoken.endpoint.com";
        Scopes = "Google Scopes";
        RedirectUri = "GoogleRedirectUri";

        StateValue = new byte[128];
        foreach (var i in Enumerable.Range(0, 128))
            StateValue[i] = (byte)(i + 3);
    }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AuthEndpoint { get; set; }
    public string TokenEndpoint { get; set; }
    public string Scopes { get; set; }
    public string RedirectUri { get; set; }
    public byte[] StateValue { get; set; }
}