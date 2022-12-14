namespace SiteWatcher.Common.Services;

public interface IHttpClient
{
    Task<Stream?> GetStreamAsync(Uri uri, CancellationToken ct);
}