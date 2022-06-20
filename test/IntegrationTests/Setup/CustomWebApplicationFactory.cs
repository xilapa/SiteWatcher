using System.Runtime.CompilerServices;
using IntegrationTests.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReflectionMagic;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using ISession = SiteWatcher.Application.Interfaces.ISession;

namespace SiteWatcher.IntegrationTests.Setup;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private IDictionary<Type, object>? _servicesToReplace;
    private string _connectionString;
    private readonly SqliteConnection? _sqliteConnection;
    public readonly Mock<IEmailService> EmailServiceMock;
    public readonly IAuthService AuthServiceForTokens;
    public DateTime CurrentTime { get; set; }
    public IAppSettings TestSettings { get; }
    public IGoogleSettings TestGoogleSettings { get; }
    public FakeCache FakeCache { get; }

    public CustomWebApplicationFactory(Action<CustomWebApplicationOptions>? options = null)
    {
        EmailServiceMock = EmailServiceMock = new Mock<IEmailService>();
        ConfigureTest(options);
        _sqliteConnection = new SqliteConnection(_connectionString);
        _sqliteConnection.Open();
        TestSettings = new TestAppSettings();
        TestGoogleSettings = new TestGoogleSettings();
        FakeCache = new FakeCache();
        ApplyEnvironmentVariables(TestSettings);
        ApplyEnvironmentVariables(TestGoogleSettings);
        AuthServiceForTokens = CreateAuthServiceForLogin();
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
        _connectionString = optionsInstance.DatabaseType switch
        {
            DatabaseType.SqliteInMemory => "DataSource=:memory:",
            DatabaseType.SqliteOnDisk => $"DataSource={DateTime.Now.Ticks}.db",
            _ => "DataSource=:memory:"
        };
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

        services.AddScoped<IUnitOfWork>(_ => _.GetRequiredService<SiteWatcherContext>());

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

    private IAuthService CreateAuthServiceForLogin()
    {
        var authService = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        if (authService is not AuthService authServiceInstance)
            throw new Exception($"The {typeof(AuthService)} was not created");

        authServiceInstance.AsDynamic()._appSettings = TestSettings;
        return authServiceInstance;
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await using var context = GetContext();
        await _sqliteConnection?.CloseAsync()!;

        // Connections are pooled by SQLite in order to improve performance.
        // It means when you call Close method on a connection object,
        // connection to database may still be alive (in the background)
        // so that next Open method become faster.
        // When you known that you don't want a new connection anymore,
        // calling ClearAllPools closes all the connections
        // which are alive in the background and file handle(s?)
        // to the db file get released.Then db file may get removed,
        // deleted or used by another process.
        // https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
        SqliteConnection.ClearAllPools();

        await context.Database.EnsureDeletedAsync();
        await _sqliteConnection.DisposeAsync();
        await base.DisposeAsync();
    }
}