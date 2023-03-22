using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestGoogleSettings : IGoogleSettings
{
    public TestGoogleSettings()
    {
        ClientId  = "GoogleClientId";
        ClientSecret = "GoogleClientSecret";
    }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}