using System.Data.Common;
using System.Runtime.CompilerServices;
using MassTransit;
using Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReflectionMagic;
using SiteWatcher.Application;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Emails.Messages;
using SiteWatcher.Application.IdempotentConsumers;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Utils;
using StackExchange.Redis;
using HttpClient = SiteWatcher.Infra.Http.HttpClient;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;
using ISession = SiteWatcher.Domain.Authentication.ISession;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private IDictionary<Type, object>? _servicesToReplace;

    private string _connectionString;
    private Func<IAppSettings, IMediator, SiteWatcherContext> _contextFactory;
    private DatabaseType _databaseType;

    private readonly ILoggerFactory _loggerFactory;
    public readonly Mock<IEmailServiceSingleton> EmailServiceMock;
    public readonly Mock<IHttpClientFactory> HttpClientFactoryMock;
    public readonly IAuthService AuthServiceForTokens;
    private readonly IGoogleSettings _testGoogleSettings;
    private bool _enableMasstransitTestHarness;
    private bool _addMessageHandlers;

    public DateTime CurrentTime { get; set; }
    public IAppSettings TestSettings { get; }
    public FakeCache FakeCache { get; }
    public FakePublisher FakePublisher { get; }
    public Mock<ILogger> LoggerMock { get; }

    public CustomWebApplicationFactory(BaseTestFixtureOptions options, string connectionString, DbConnection? dbConnection)
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        LoggerMock = new Mock<ILogger>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(LoggerMock.Object);
        _loggerFactory = loggerFactoryMock.Object;

        EmailServiceMock = EmailServiceMock = new Mock<IEmailServiceSingleton>();
        HttpClientFactoryMock = new Mock<IHttpClientFactory>();

        SetTestSettings(options, connectionString, dbConnection);

        TestSettings = new TestAppSettings();
        _testGoogleSettings = new TestGoogleSettings();
        FakeCache = new FakeCache();
        FakePublisher = new FakePublisher();
        EnvironmentUtils.ApplyEnvironmentVariables(TestSettings, _testGoogleSettings);
        AuthServiceForTokens = CreateAuthServiceForTokens();
    }

    private void SetTestSettings(BaseTestFixtureOptions options, string connectionString, DbConnection? dbConnection)
    {
        CurrentTime = options.InitialDate ?? DateTime.UtcNow;
        _servicesToReplace = options.ReplacementServices;
        _enableMasstransitTestHarness = options.EnableMasstransitTestHarness;
        _addMessageHandlers = options.AddMessageHandlers;

        _databaseType = options.DatabaseType;
        _connectionString = connectionString;

        _contextFactory = options.DatabaseType switch
        {
            DatabaseType.SqliteInMemory => (appSettings, mediator) =>
                new SqliteContext(appSettings, mediator, dbConnection!),
            DatabaseType.SqliteOnDisk => (appSettings, mediator) =>
                new SqliteContext(appSettings, mediator, dbConnection!),
            DatabaseType.Postgres => (appSettings, mediator) =>
                new PostgresTestContext(appSettings, mediator, _connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(options.DatabaseType))
        };
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            ReplaceServices(services);
            ConfigureOptionsReplacementServices(services);
        });
    }

    private void ReplaceServices(IServiceCollection services)
    {
        // Services to remove
        var servicesToRemove = new[]
        {
            typeof(IConnectionMultiplexer),
            typeof(ICache),
            typeof(DbContext),
            typeof(ISession),
            typeof(IAppSettings),
            typeof(IGoogleSettings),
            typeof(EmailSettings),
            typeof(IEmailServiceSingleton),
            typeof(IDapperContext),
            typeof(ILoggerFactory),
            typeof(IHttpClientFactory),
            typeof(IQueries),
            typeof(SiteWatcherContext)
        };

        if (!_enableMasstransitTestHarness)
            servicesToRemove = servicesToRemove.Append(typeof(IPublisher)).ToArray();

        // Only replace DapperQueries if using Sqlite
        if(_databaseType is DatabaseType.SqliteInMemory or DatabaseType.SqliteOnDisk)
            servicesToRemove = servicesToRemove.Append(typeof(IQueries)).ToArray();

        var descriptorsToRemove = services
            .Where(desc => servicesToRemove.Contains(desc.ServiceType))
            .ToArray();

        foreach (var serviceDescriptor in descriptorsToRemove)
            services.Remove(serviceDescriptor);

        // Mock EmailService, LoggerFactory and HttpClientFactory
        services.AddTransient<ILoggerFactory>(_ => _loggerFactory);
        services.AddSingleton<IHttpClientFactory>(HttpClientFactoryMock.Object);

        // Services to replace
        // Cache, DbContext, UnitOfWork, Session,
        // AppSettings, GoogleSettings, DapperContext,
        // DapperQueries
        services.AddSingleton<ICache>(FakeCache);

        services.AddScoped<SiteWatcherContext>(srvc =>
        {
            var appSettings = srvc.GetRequiredService<IAppSettings>();
            var mediator = srvc.GetRequiredService<IMediator>();
            return _contextFactory(appSettings, mediator);
        });

        services.AddScoped<ISession>(srvc =>
        {
            var httpContextAccessor = srvc.GetRequiredService<IHttpContextAccessor>();
            return new TestSession(httpContextAccessor, CurrentTime);
        });

        services.AddSingleton<IAppSettings>(TestSettings);
        services.AddSingleton<IGoogleSettings>(_testGoogleSettings);
        services.AddScoped<IDapperContext>(_ => new TestDapperContext(TestSettings, _connectionString, _databaseType));
        services.AddSingleton<IQueries>(new Queries(_databaseType));

        // Execute AlertServices
        services.AddScoped<IUserAlertsService, UserAlertsService>();
        services.AddScoped<ExecuteAlertsCommandHandler>();
        services.AddScoped<IHttpClient, HttpClient>();

        // Email
        services.AddSingleton<IEmailServiceSingleton>(EmailServiceMock.Object);
        services.AddSingleton(new EmailSettings());

        // Messaging
        if (!_enableMasstransitTestHarness) services.AddSingleton<IPublisher>(FakePublisher);
        if (_addMessageHandlers) services.AddMessageHandlers();
        services.AddMassTransitTestHarness(c =>
        {
            if (!_enableMasstransitTestHarness) return;
            c.AddConsumers(typeof(SendEmailOnEmailCreatedMessageHandler).Assembly);
        });

        // IdempotentConsumers
        services.AddScoped<CleanIdempotentConsumers>();
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
}