using System.Data.Common;
using System.Runtime.CompilerServices;
using DotNetCore.CAP;
using Infra.Persistence.Repositories;
using IntegrationTests.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using ReflectionMagic;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Notifications.Commands.ProcessNotifications;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Domain.Emails.Repositories;
using SiteWatcher.Domain.Notifications.Repositories;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Persistence.Repositories;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using HttpClient = SiteWatcher.Infra.Http.HttpClient;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;
using ISession = SiteWatcher.Domain.Authentication.ISession;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private IDictionary<Type, object>? _servicesToReplace;

    private string _connectionString;
    private DbConnection? _dbConnection;
    private Func<IAppSettings, IMediator, SiteWatcherContext> _contextFactory;
    private PostgreSqlContainer? _postgresContainer;
    public DatabaseType DatabaseType { get; private set; }

    private readonly ILoggerFactory _loggerFactory;
    public readonly Mock<IEmailService> EmailServiceMock;
    public readonly Mock<IHttpClientFactory> HttpClientFactoryMock;
    public readonly IAuthService AuthServiceForTokens;
    public DateTime CurrentTime { get; set; }
    public IAppSettings TestSettings { get; }
    public IGoogleSettings TestGoogleSettings { get; }
    public FakeCache FakeCache { get; }
    public FakePublisher FakePublisher { get; }
    public Mock<ILogger> LoggerMock { get; }

    public CustomWebApplicationFactory(CustomWebApplicationOptions options)
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        LoggerMock = new Mock<ILogger>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(LoggerMock.Object);
        _loggerFactory = loggerFactoryMock.Object;

        EmailServiceMock = EmailServiceMock = new Mock<IEmailService>();
        HttpClientFactoryMock = new Mock<IHttpClientFactory>();

        var testSettings = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json")
            .Build()
            .Get<TestSettings>();

        ApplyEnvironmentVariables(testSettings!);

        ConfigureTest(options).Wait();

        TestSettings = new TestAppSettings();
        TestGoogleSettings = new TestGoogleSettings();
        FakeCache = new FakeCache();
        FakePublisher = new FakePublisher();
        ApplyEnvironmentVariables(TestSettings);
        ApplyEnvironmentVariables(TestGoogleSettings);
        AuthServiceForTokens = CreateAuthServiceForTokens();
    }

    private async Task ConfigureTest(CustomWebApplicationOptions options)
    {
        CurrentTime = options.InitalDate ?? DateTime.UtcNow;
        _servicesToReplace = options.ReplacementServices;

        switch (options.DatabaseType)
        {
            case DatabaseType.SqliteInMemory:
                DatabaseType = DatabaseType.SqliteInMemory;
                _connectionString = "DataSource=:memory:";
                _dbConnection = new SqliteConnection(_connectionString);
                // handling the connection on factory to avoid the database being destroyed on connection close
                _dbConnection.Open();
                _contextFactory = (appSettings, mediator) => new SqliteContext(appSettings, mediator, _dbConnection);
                break;
            case DatabaseType.SqliteOnDisk:
                DatabaseType = DatabaseType.SqliteOnDisk;
                _connectionString = $"DataSource={Guid.NewGuid()}.db";
                _dbConnection = new SqliteConnection(_connectionString);
                _contextFactory = (appSettings, mediator) => new SqliteContext(appSettings, mediator, _dbConnection);
                break;
            case DatabaseType.PostgresOnDocker:
                DatabaseType = DatabaseType.PostgresOnDocker;
                _postgresContainer = new PostgreSqlBuilder()
                    .WithDatabase($"testDb{Guid.NewGuid()}")
                    .Build();
                await _postgresContainer.StartAsync();
                _connectionString = _postgresContainer.GetConnectionString();
                _contextFactory = (appSettings, mediator) => new PostgresTestContext(appSettings, mediator, _connectionString);
                break;
            default:
                throw new ArgumentException(nameof(options.DatabaseType));
        }
    }

    private static void ApplyEnvironmentVariables(object testSettings)
    {
        foreach (var prop in testSettings.GetType().GetProperties())
        {
            var value = prop.PropertyType == typeof(byte[])
                ? Convert.ToBase64String((prop.GetValue(testSettings) as byte[])!)
                : prop.GetValue(testSettings)!.ToString();
            if(value == null) continue;
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
        // Services to remove
        var servicesToRemove = new[]
        {
            typeof(IConnectionMultiplexer),
            typeof(ICache),
            typeof(DbContext),
            typeof(IUnitOfWork),
            typeof(ISession),
            typeof(IAppSettings),
            typeof(IGoogleSettings),
            typeof(IEmailSettings),
            typeof(IEmailService),
            typeof(IDapperContext),
            typeof(ILoggerFactory),
            typeof(IHttpClientFactory),
            typeof(IPublisher),
            typeof(IPublishService),
            typeof(ICapPublisher)
        };

        // Only replace DapperQueries if using Sqlite
        if(DatabaseType is DatabaseType.SqliteInMemory or DatabaseType.SqliteOnDisk)
            servicesToRemove = servicesToRemove.Append(typeof(IDapperQueries)).ToArray();

        var descriptorsToRemove = services
            .Where(desc => servicesToRemove.Contains(desc.ServiceType))
            .ToArray();

        foreach (var serviceDescriptor in descriptorsToRemove)
            services.Remove(serviceDescriptor);

        // Mock EmailService, LoggerFactory and HttpClientFactory
        services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);
        services.AddTransient<ILoggerFactory>(_ => _loggerFactory);
        services.AddSingleton<IHttpClientFactory>(HttpClientFactoryMock.Object);

        // Services to replace
        // Cache, DbContext, UnitOfWork, Session,
        // AppSettings, GoogleSettings, DapperContext,
        // DapperQueries, Publisher
        services.AddSingleton<ICache>(FakeCache);
        services.AddSingleton<IPublisher>(FakePublisher);
        services.AddScoped<IPublishService, FakePublishService>();

        services.AddScoped<SiteWatcherContext>(srvc =>
        {
            var appSettings = srvc.GetRequiredService<IAppSettings>();
            var mediator = srvc.GetRequiredService<IMediator>();
            return _contextFactory(appSettings, mediator);
        });

        services.AddScoped<IUnitOfWork>(_ => _.GetRequiredService<SiteWatcherContext>());

        services.AddScoped<ISession>(srvc =>
        {
            var httpContextAccessor = srvc.GetRequiredService<IHttpContextAccessor>();
            return new TestSession(httpContextAccessor, CurrentTime);
        });

        services.AddSingleton<IAppSettings>(TestSettings);
        services.AddSingleton<IGoogleSettings>(TestGoogleSettings);
        services.AddScoped<IDapperContext>(_ => new TestDapperContext(TestSettings, _connectionString, DatabaseType));

        // Only replace DapperQueries if using Sqlite
        if(DatabaseType is DatabaseType.SqliteInMemory or DatabaseType.SqliteOnDisk)
            services.AddSingleton<IDapperQueries, SqliteDapperQueries>();

        // Execute AlertServices
        services.AddScoped<IUserAlertsService, UserAlertsService>();
        services.AddScoped<ExecuteAlertsCommandHandler>();
        services.AddScoped<IHttpClient, HttpClient>();
        services.AddScoped<IEmailRepository, EmailRepository>();

        // Process notification services
        services.AddScoped<ProcessNotificationCommandHandler>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
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
        return _contextFactory(TestSettings, mediatorMock.Object);
    }

    private IAuthService CreateAuthServiceForTokens()
    {
        var authService = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        if (authService is not AuthService authServiceInstance)
            throw new Exception($"The {typeof(AuthService)} was not created");

        authServiceInstance.AsDynamic()._appSettings = TestSettings;

        var testSession = RuntimeHelpers.GetUninitializedObject(typeof(TestSession));
        if (testSession is not TestSession testSessionInstace)
            throw new Exception($"The {typeof(AuthService)} was not created");

        testSessionInstace.AsDynamic()._currentTime = CurrentTime;

        return authServiceInstance;
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await using var context = GetContext();
        await context.Database.CloseConnectionAsync();

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
        if(_dbConnection is SqliteConnection) SqliteConnection.ClearAllPools();
        if (_postgresContainer is not null) NpgsqlConnection.ClearAllPools();

        await context.Database.EnsureDeletedAsync();
        if(_dbConnection is not null) await _dbConnection.DisposeAsync();
        if (_postgresContainer is not null) await _postgresContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}