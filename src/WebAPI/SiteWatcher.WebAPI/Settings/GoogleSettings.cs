namespace SiteWatcher.WebAPI.Settings;

public class GoogleSettings
{
    [ConfigurationKeyName("Google_ClientId")]
    public string ClientId { get; set; } = null!;

    [ConfigurationKeyName("Google_ClientSecret")]
    public string ClientSecret { get; set; } = null!;

    [ConfigurationKeyName("Google_AuthEndpoint")]
    public string AuthEndpoint { get; set; } = null!;

    [ConfigurationKeyName("Google_TokenEndpoint")]
    public string TokenEndpoint { get; set; } = null!;

    [ConfigurationKeyName("Google_Scopes")]
    public string Scopes { get; set; } = null!;

    [ConfigurationKeyName("Google_RedirectUri")]
    public string RedirectUri { get; set; } = null!;

    [ConfigurationKeyName("Google_StateValue")]
    public byte[] StateValue { get; set; } = null!;
}