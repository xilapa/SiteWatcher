using MediatR;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;

namespace SiteWatcher.Application.Users.EventHandlers;

public class SendEmailConfirmationEventHandler : INotificationHandler<UserRegisteredNotification>,
    INotificationHandler<UserUpdatedNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - " +
                                                $"{notification.Email} - {notification.Email}"), cancellationToken);
    }

    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - " +
                                                $"{notification.Email} - {notification.Email}"), cancellationToken);
    }
}