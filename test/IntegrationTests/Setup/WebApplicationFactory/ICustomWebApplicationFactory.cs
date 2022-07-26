using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public interface ICustomWebApplicationFactory
{
    SiteWatcherContext GetContext();
    IServiceProvider Services { get; }
}