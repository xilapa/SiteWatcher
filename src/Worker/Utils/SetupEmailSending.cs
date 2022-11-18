using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.EmailSending;

namespace SiteWatcher.Worker.Utils;

public static class SetupEmailSending
{
    public static IServiceCollection SetupEmail(this IServiceCollection services, WorkerSettings settings)
    {
        services.AddSingleton<IEmailServiceSingleton>(new EmailServiceSingleton(settings.EmailSettings));
        return services;
    }
}