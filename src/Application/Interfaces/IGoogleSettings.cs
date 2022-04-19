namespace SiteWatcher.Application.Interfaces;

public interface IGoogleSettings
{
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string AuthEndpoint { get; set; }
    string TokenEndpoint { get; set; }
    string Scopes { get; set; }
    string RedirectUri { get; set; }
    byte[] StateValue { get; set; }
}