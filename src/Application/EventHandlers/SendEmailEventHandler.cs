using MediatR;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.DeleteUser;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;

namespace SiteWatcher.Application.Users.EventHandlers;

public class SendEmailConfirmationEventHandler :INotificationHandler<AccountDeletedNotification>,
    INotificationHandler<AccountDeactivatedNotification>, INotificationHandler<AccountReactivationEmailNotification>
{
    public Task Handle(AccountDeletedNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - " +
                                                $"{notification.Email} - {notification.Email}"), cancellationToken);
    }

    public Task Handle(AccountDeactivatedNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - " +
                                                $"{notification.Email} - {notification.Email}"), cancellationToken);
    }

    public Task Handle(AccountReactivationEmailNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - " +
                                                $"{notification.Email} - {notification.Email}"), cancellationToken);
    }
}