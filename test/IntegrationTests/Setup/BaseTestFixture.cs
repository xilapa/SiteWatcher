using Microsoft.AspNetCore.Mvc.Testing;
using SiteWatcher.IntegrationTests.Setup;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace IntegrationTests.Setup;

public class BaseTestFixture : IAsyncLifetime
{
    private readonly CustomWebApplicationOptionsBuilder _optionsBuilder;
    public CustomWebApplicationFactory<Program> AppFactory = null!;
    public HttpClient Client = null!;

    public BaseTestFixture()
    {
        _optionsBuilder = new CustomWebApplicationOptionsBuilder();
    }

    protected virtual void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    { }

    public virtual async Task InitializeAsync()
    {
        OnConfiguringTestServer(_optionsBuilder);
        AppFactory = new CustomWebApplicationFactory<Program>(_optionsBuilder.Build());
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