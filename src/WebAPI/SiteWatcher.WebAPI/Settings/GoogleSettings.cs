using System.Web;
using Microsoft.Extensions.Configuration;

namespace SiteWatcher.WebAPI.Settings;

public class GoogleSettings
{
    [ConfigurationKeyName("Google_ClientId")]
    public string ClientId { get; set; }

    [ConfigurationKeyName("Google_ClientSecret")]
    public string ClientSecret { get; set; }

    [ConfigurationKeyName("Google_AuthEndpoint")]
    public string AuthEndpoint { get; set; }

    [ConfigurationKeyName("Google_TokenEndpoint")]
    public string TokenEndpoint { get; set; }

    [ConfigurationKeyName("Google_Scopes")]
    public string Scopes { get; set; }

    [ConfigurationKeyName("Google_RedirectUri")]
    public string RedirectUri { get; set; }
}