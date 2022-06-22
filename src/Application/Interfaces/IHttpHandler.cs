namespace SiteWatcher.Application.Interfaces;

public interface IHttpHandler
{
    void InitializeHttpClient(string clientName);
    Task<T?> PostAsync<T>(string url, object requestBody, CancellationToken cancellationToken);
}