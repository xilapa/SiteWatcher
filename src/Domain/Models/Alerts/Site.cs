namespace SiteWatcher.Domain.Models.Alerts;

public record Site
{
    // ctor for EF
    protected Site()
    { }

    public Site(string uri, string name)
    {
        Uri = new Uri(uri);
        Name = name;
    }

    public Site(Uri uri, string name)
    {
        Uri = uri;
        Name = name;
    }

    public Uri Uri { get; }
    public string Name { get; }
}