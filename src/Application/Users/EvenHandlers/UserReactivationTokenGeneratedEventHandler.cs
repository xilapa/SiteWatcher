using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.Events;

namespace SiteWatcher.Application.Users.EventHandlers;

public class UserReactivationTokenGeneratedEventHandler : INotificationHandler<UserReactivationTokenGeneratedEvent>
{
    private readonly IAppSettings _appSettings;
    private readonly IFireAndForgetService _fireAndForgetService;

    public UserReactivationTokenGeneratedEventHandler(IAppSettings appSettings,
        IFireAndForgetService fireAndForgetService)
    {
        _appSettings = appSettings;
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(UserReactivationTokenGeneratedEvent notification, CancellationToken cancellationToken)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/reactivate-account?t={notification.ConfirmationToken}";
        var message =
            MailMessageGenerator.AccountActivation(notification.Name, notification.Email, link,
                notification.Language);
        _fireAndForgetService.ExecuteWith<IAuthService, IEmailService>(async (authService, emailService) =>
        {
            await authService.SetAccountActivationTokenExpiration(notification.ConfirmationToken, notification.UserId);
            await emailService.SendEmailAsync(message, CancellationToken.None);
        });
        return Task.CompletedTask;
    }
}