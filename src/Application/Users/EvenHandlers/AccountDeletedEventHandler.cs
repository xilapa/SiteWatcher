using Domain.Events;
using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Users.EventHandlers;

public class AccountDeletedEventHandler : INotificationHandler<AccountDeletedEvent>
{
    private readonly IFireAndForgetService _fireAndForgetService;

    public AccountDeletedEventHandler(IFireAndForgetService fireAndForgetService)
    {
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(AccountDeletedEvent notification, CancellationToken cancellationToken)
    {
        var message = MailMessageGenerator.AccountDeleted(notification.Name, notification.Email, notification.Language);
        _fireAndForgetService.ExecuteWith<IEmailService>(emailService =>
                emailService.SendEmailAsync(message, CancellationToken.None));
        return Task.CompletedTask;
    }
}