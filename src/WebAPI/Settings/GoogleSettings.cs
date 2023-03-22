using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Settings;

public class GoogleSettings : IGoogleSettings
{
    [ConfigurationKeyName("Google_ClientId")]
    public string ClientId { get; set; } = null!;

    [ConfigurationKeyName("Google_ClientSecret")]
    public string ClientSecret { get; set; } = null!;
}