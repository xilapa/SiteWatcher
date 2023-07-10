using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Infra.EmailSending;

namespace SiteWatcher.Worker.Utils;

public static class SetupEmailSending
{
    public static IServiceCollection SetupEmail(this IServiceCollection services, EmailSettings settings)
    {
        services
        .AddSingleton<IEmailSettings>(settings)
        .AddSingleton<IEmailServiceSingleton, EmailServiceSingleton>();
        return services;
    }
}