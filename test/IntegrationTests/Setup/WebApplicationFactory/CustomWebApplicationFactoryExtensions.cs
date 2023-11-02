using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public static class CustomWebApplicationFactoryExtensions
{
    public static async Task WithDbContext(this CustomWebApplicationFactory<Program> appFactory,
        Func<SiteWatcherContext, Task> action)
    {
        await using var context = appFactory.GetContext();
        await action(context);
    }

    public static async Task<T> WithDbContext<T>(this CustomWebApplicationFactory<Program> appFactory,
        Func<SiteWatcherContext, Task<T>> action)
    {
        await using var context = appFactory.GetContext();
        var result = await action(context);
        return result;
    }

    public static async Task WithServiceProvider(this CustomWebApplicationFactory<Program> appFactory,
        Func<IServiceProvider, Task> func)
    {
        await using var scope = appFactory.Services.CreateAsyncScope();
        await func(scope.ServiceProvider);
    }

    public static async Task<T> WithServiceProvider<T>(this CustomWebApplicationFactory<Program> appFactory,
    Func<IServiceProvider, Task<T>> func)
    {
        await using var scope = appFactory.Services.CreateAsyncScope();
        var result = await func(scope.ServiceProvider);
        return result;
    }

    public static async Task<Alert> CreateAlert(this CustomWebApplicationFactory<Program> appFactory, string name,
        RuleType ruleType, UserId userId, DateTime? currentDate = null, Frequencies? frequency = null)
    {
        return await CreateAlert<Alert>(appFactory, name, ruleType, userId, currentDate, frequency: frequency);
    }

    public static async Task<T> CreateAlert<T>(this CustomWebApplicationFactory<Program> appFactory, string name,
        RuleType ruleType, UserId userId, DateTime? currentDate = null, string? siteName = null,
        string? siteUri = null, Frequencies? frequency = null) where T : class
    {
        currentDate ??= appFactory.CurrentTime;
        var createAlertInput = new CreateAlertInput
        {
            Name = name,
            RuleType = ruleType,
            Frequency = frequency ?? Frequencies.TwoHours,
            Term = "test term",
            SiteName = siteName ?? "test site",
            SiteUri = siteUri ?? "http://mytest.net",
            RegexPattern = "[a-z]+",
            NotifyOnDisappearance = false
        };
        var alert = AlertFactory.Create(createAlertInput, userId, currentDate.Value);
        await WithDbContext(appFactory, async ctx =>
        {
            ctx.Add(alert);
            await ctx.SaveChangesAsync();
        });

        if(typeof(T) == typeof(Alert))
            return (alert as T)!;

        var idHasher = await WithServiceProvider(appFactory, provider =>
            Task.FromResult(provider.GetRequiredService<IIdHasher>()));

        if (typeof(T) == typeof(DetailedAlertView))
            return (DetailedAlertView.FromAlert(alert, idHasher) as T)!;

        if (typeof(T) == typeof(SimpleAlertView))
            return (SimpleAlertView.FromAlert(alert, idHasher) as T)!;

        throw new NotImplementedException($"Conversion from Alert to {typeof(T).Name}");
    }
}