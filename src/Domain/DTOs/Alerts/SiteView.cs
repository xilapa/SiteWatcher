using SiteWatcher.Domain.Models.Alerts;

namespace Domain.DTOs.Alerts;

public class SiteView
{
    public SiteView(string name, string uri)
    {
        Name = name;
        Uri = uri;
    }

    public string Name { get; set; }
    public string Uri { get; set; }

    public static implicit operator SiteView(Site site) =>
        new (site.Name, site.Uri.ToString());
}