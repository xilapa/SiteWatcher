using System.Data;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using StackExchange.Redis;
using ISession = SiteWatcher.Application.Interfaces.ISession;

namespace SiteWatcher.IntegrationTests.Setup;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly SqliteConnection _sqliteConnection;

    public DateTime CurrentTime { get; set; }
    public IAppSettings TestSettings { get; }
    public IGoogleSettings TestGoogleSettings { get; }
    public FakeCache FakeCache { get; }

    public CustomWebApplicationFactory(Mock<IEmailService> emailServiceMock)
    {
        _emailServiceMock = emailServiceMock;
        _sqliteConnection = new SqliteConnection($"DataSource={DateTime.Now.Ticks}.db");
        _sqliteConnection.Open();
        TestSettings = new TestAppSettings();
        TestGoogleSettings = new TestGoogleSettings();
        FakeCache = new FakeCache();
        ApplyEnvironmentVariables(TestSettings);
        ApplyEnvironmentVariables(TestGoogleSettings);
    }

    private static void ApplyEnvironmentVariables(object testSettings)
    {
        foreach (var prop in testSettings.GetType().GetProperties())
        {
            var value = prop.PropertyType == typeof(byte[]) ?
                Convert.ToBase64String((prop.GetValue(testSettings) as byte[])!) :
                prop.GetValue(testSettings)!.ToString();
            Environment.SetEnvironmentVariable(prop.Name, value);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Services to remove
        // ConnectionMultiplexer

        // Services to replace
        // Cache, DbContext, UnitOfWork, Session,
        // AppSettings, GoogleSettings, DapperContext,
        // DapperQueries

        // Services to mock
        // EmailService
        var servicesToModify = new[]
        {
            typeof(IConnectionMultiplexer),
            typeof(ICache),
            typeof(SiteWatcherContext),
            typeof(IUnityOfWork),
            typeof(IEmailService),
            typeof(ISession),
            typeof(IAppSettings),
            typeof(IGoogleSettings),
            typeof(IDapperContext)
        };

        builder.ConfigureServices(services =>
        {
            var servicesDescriptorsToRemove = services
                .Where(s => servicesToModify.Contains(s.ServiceType))
                .ToArray();

            foreach (var serviceDescriptor in servicesDescriptorsToRemove)
                services.Remove(serviceDescriptor);

            ReplaceServices(services);
            MockServices(services);
        });

        base.ConfigureWebHost(builder);
    }

    private void ReplaceServices(IServiceCollection services)
    {
        services.AddSingleton<ICache>(FakeCache);

        services.AddScoped<SiteWatcherContext>(srvc =>
        {
            var appSettings = srvc.GetRequiredService<IAppSettings>();
            var mediator = srvc.GetRequiredService<IMediator>();
            return new SqliteContext(appSettings, mediator, _sqliteConnection);
        });

        services.AddScoped<IUnityOfWork>(_ => _.GetRequiredService<SiteWatcherContext>());

        services.AddScoped<ISession>(srvc =>
        {
            var httpContextAccessor = srvc.GetRequiredService<IHttpContextAccessor>();
            return new TestSession(httpContextAccessor, CurrentTime);
        });

        services.AddSingleton<IAppSettings>(TestSettings);
        services.AddSingleton<IGoogleSettings>(TestGoogleSettings);
        services.AddScoped<IDapperContext>( _ => new SqliteDapperContext(TestSettings, _sqliteConnection));
        services.AddSingleton<IDapperQueries, SqliteDapperQueries>();
    }

    public SiteWatcherContext GetContext()
    {
        var mediatorMock = new Mock<IMediator>();
        var context = new SqliteContext(TestSettings, mediatorMock.Object, _sqliteConnection);
        return context;
    }

    private void MockServices(IServiceCollection services)
    {
        services.AddScoped<IEmailService>(_ => _emailServiceMock.Object);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _sqliteConnection?.State != ConnectionState.Closed)
        {
            using var scope = Services?.CreateScope();
            var context = scope?.ServiceProvider.GetRequiredService<SiteWatcherContext>();
            context?.Database.EnsureDeleted();
            _sqliteConnection?.Close();
            _sqliteConnection?.Dispose();
        }

        base.Dispose(disposing);
    }
}