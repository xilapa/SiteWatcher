using MediatR;
using SiteWatcher.Application.Users.Commands.RegisterUser;

namespace SiteWatcher.Application.Users.EventHandlers;

public class SendEmailConfirmationEventHandler : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - {notification.Email} - {notification.Email}"), cancellationToken);
    }
}