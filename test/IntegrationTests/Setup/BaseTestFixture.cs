using Microsoft.AspNetCore.Mvc.Testing;
using SiteWatcher.IntegrationTests.Setup;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace IntegrationTests.Setup;

public class BaseTestFixture : IAsyncLifetime
{
    public CustomWebApplicationFactory<Program> AppFactory = null!;
    public HttpClient Client = null!;

    // TODO: transform in method
    /// <summary>
    /// Configure the test server.
    /// </summary>
    public virtual Action<CustomWebApplicationOptions>? Options { get; }

    public virtual async Task InitializeAsync()
    {
        AppFactory = new CustomWebApplicationFactory<Program>(Options);
        Client = AppFactory.CreateClient(new WebApplicationFactoryClientOptions {AllowAutoRedirect = false});
        await using var context = AppFactory.GetContext();
        await context.Database.EnsureCreatedAsync();
        await Database.SeedDatabase(context, AppFactory.CurrentTime, AppFactory.DatabaseType);
    }

    public async Task DisposeAsync()
    {
        Client.CancelPendingRequests();
        Client.Dispose();
        await AppFactory.DisposeAsync();
    }
}