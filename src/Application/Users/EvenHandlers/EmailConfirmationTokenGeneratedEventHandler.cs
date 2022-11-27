using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.Events;

namespace SiteWatcher.Application.Users.EventHandlers;

public class EmailConfirmationGeneratedEventHandler : INotificationHandler<EmailConfirmationTokenGeneratedEvent>
{
    private readonly IAppSettings _appSettings;
    private readonly IFireAndForgetService _fireAndForgetService;

    public EmailConfirmationGeneratedEventHandler(IAppSettings appSettings, IFireAndForgetService fireAndForgetService)
    {
        _appSettings = appSettings;
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(EmailConfirmationTokenGeneratedEvent notification, CancellationToken cancellationToken)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/confirm-email?t={notification.ConfirmationToken}";
        var message =
            MailMessageGenerator.EmailConfirmation(notification.Name, notification.Email, link,
                notification.Language);
        _fireAndForgetService.ExecuteWith<IAuthService, IEmailService>(async (authService, emailService) =>
        {
            await authService.SetEmailConfirmationTokenExpiration(notification.ConfirmationToken, notification.UserId);
            await emailService.SendEmailAsync(message, CancellationToken.None);
        });
        return Task.CompletedTask;
    }
}