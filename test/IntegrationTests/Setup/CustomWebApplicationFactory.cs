using IntegrationTests.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using ISession = SiteWatcher.Application.Interfaces.ISession;

namespace SiteWatcher.IntegrationTests.Setup;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public readonly Mock<IEmailService> EmailServiceMock;
    private IDictionary<Type, object>? _servicesToReplace;
    private readonly SqliteConnection? _sqliteConnection;

    public DateTime CurrentTime { get; set; }
    public IAppSettings TestSettings { get; }
    public IGoogleSettings TestGoogleSettings { get; }
    public FakeCache FakeCache { get; }

    public CustomWebApplicationFactory(Action<CustomWebApplicationOptions>? options = null)
    {
        EmailServiceMock = EmailServiceMock = new Mock<IEmailService>();
        ConfigureTest(options);
        _sqliteConnection = new SqliteConnection($"DataSource={DateTime.Now.Ticks}.db");
        _sqliteConnection.Open();
        TestSettings = new TestAppSettings();
        TestGoogleSettings = new TestGoogleSettings();
        FakeCache = new FakeCache();
        ApplyEnvironmentVariables(TestSettings);
        ApplyEnvironmentVariables(TestGoogleSettings);
    }

    private void ConfigureTest(Action<CustomWebApplicationOptions>? options)
    {
        if (options is null)
        {
            CurrentTime = DateTime.Now;
            return;
        }

        var optionsInstance = new CustomWebApplicationOptions();
        options(optionsInstance);
        CurrentTime = optionsInstance.InitalDate ?? DateTime.Now;
        _servicesToReplace = optionsInstance.ReplacementServices;
    }

    private static void ApplyEnvironmentVariables(object testSettings)
    {
        foreach (var prop in testSettings.GetType().GetProperties())
        {
            var value = prop.PropertyType == typeof(byte[])
                ? Convert.ToBase64String((prop.GetValue(testSettings) as byte[])!)
                : prop.GetValue(testSettings)!.ToString();
            Environment.SetEnvironmentVariable(prop.Name, value);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceServices(services);
            ConfigureOptionsReplacementServices(services);
        });

        base.ConfigureWebHost(builder);
    }

    private void ReplaceServices(IServiceCollection services)
    {
        // Mock EmailService
        services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);

        // Services to replace
        // Cache, DbContext, UnitOfWork, Session,
        // AppSettings, GoogleSettings, DapperContext,
        // DapperQueries
        services.AddSingleton<ICache>(FakeCache);

        services.AddScoped<SiteWatcherContext>(srvc =>
        {
            var appSettings = srvc.GetRequiredService<IAppSettings>();
            var mediator = srvc.GetRequiredService<IMediator>();
            return new SqliteContext(appSettings, mediator, _sqliteConnection!);
        });

        services.AddScoped<IUnityOfWork>(_ => _.GetRequiredService<SiteWatcherContext>());

        services.AddScoped<ISession>(srvc =>
        {
            var httpContextAccessor = srvc.GetRequiredService<IHttpContextAccessor>();
            return new TestSession(httpContextAccessor, CurrentTime);
        });

        services.AddSingleton<IAppSettings>(TestSettings);
        services.AddSingleton<IGoogleSettings>(TestGoogleSettings);
        services.AddScoped<IDapperContext>(_ => new SqliteDapperContext(TestSettings, _sqliteConnection!));
        services.AddSingleton<IDapperQueries, SqliteDapperQueries>();
    }

    private void ConfigureOptionsReplacementServices(IServiceCollection services)
    {
        if (_servicesToReplace is null || _servicesToReplace.Count == 0)
            return;

        foreach (var (serviceType, replacementService) in _servicesToReplace)
        {
            var descriptor = services.FirstOrDefault(s => s.ServiceType == serviceType);
            if (descriptor is null)
                continue;
            services.Remove(descriptor);
            var replacementServiceDescriptor =
                new ServiceDescriptor(serviceType, _ => replacementService, descriptor.Lifetime);
            services.Add(replacementServiceDescriptor);
        }
    }

    public SiteWatcherContext GetContext()
    {
        var mediatorMock = new Mock<IMediator>();
        var context = new SqliteContext(TestSettings, mediatorMock.Object, _sqliteConnection!);
        return context;
    }

    public async Task<IAuthService> GetAuthService()
    {
        await using var scope = Services.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<ISession>();
        var authService = new AuthService(TestSettings, FakeCache, session);
        return authService;
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await using var context = GetContext();
        await context.Database.EnsureDeletedAsync();
        await _sqliteConnection?.CloseAsync()!;
        await _sqliteConnection.DisposeAsync();
        await base.DisposeAsync();
    }
}