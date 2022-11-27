using Domain.Alerts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public static class CustomWebApplicationFactoryExtensions
{
    public static async Task WithDbContext(this ICustomWebApplicationFactory appFactory,
        Func<SiteWatcherContext, Task> action)
    {
        await using var context = appFactory.GetContext();
        await action(context);
    }

    public static async Task<T> WithDbContext<T>(this ICustomWebApplicationFactory appFactory,
        Func<SiteWatcherContext, Task<T>> action)
    {
        await using var context = appFactory.GetContext();
        var result = await action(context);
        return result;
    }

    public static async Task WithServiceProvider(this ICustomWebApplicationFactory appFactory,
        Func<IServiceProvider, Task> func)
    {
        await using var scope = appFactory.Services.CreateAsyncScope();
        await func(scope.ServiceProvider);
    }

    public static async Task<T> WithServiceProvider<T>(this ICustomWebApplicationFactory appFactory,
    Func<IServiceProvider, Task<T>> func)
    {
        await using var scope = appFactory.Services.CreateAsyncScope();
        var result = await func(scope.ServiceProvider);
        return result;
    }

    public static async Task<Alert> CreateAlert(this ICustomWebApplicationFactory appFactory, string name,
        WatchModes watchMode, UserId userId, DateTime? currentDate = null)
    {
        return await CreateAlert<Alert>(appFactory, name, watchMode, userId, currentDate);
    }

    public static async Task<T> CreateAlert<T>(this ICustomWebApplicationFactory appFactory, string name,
        WatchModes watchMode, UserId userId, DateTime? currentDate = null, string? siteName = null,
        string? siteUri = null) where T : class
    {
        currentDate ??= appFactory.CurrentTime;
        var createAlertInput = new CreateAlertInput
        {
            Name = name,
            WatchMode = watchMode,
            Frequency = Frequencies.EightHours,
            Term = "test term",
            SiteName = siteName ?? "test site",
            SiteUri = siteUri ?? "http://mytest.net"
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