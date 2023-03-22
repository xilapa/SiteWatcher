namespace SiteWatcher.Application.Interfaces;

public interface IGoogleSettings
{
    string ClientId { get; set; }
    string ClientSecret { get; set; }
}