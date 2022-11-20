using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.EmailSending;

namespace SiteWatcher.Worker.Utils;

public static class SetupEmailSending
{
    public static IServiceCollection SetupEmail(this IServiceCollection services, EmailSettings settings)
    {
        services.AddSingleton<IEmailServiceSingleton>(new EmailServiceSingleton(settings));
        return services;
    }
}