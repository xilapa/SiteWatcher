using SiteWatcher.Application.Users.Commands.RegisterUser;
using MediatR;

namespace SiteWatcher.Application.Handlers.EventHandlers;

public class SendEmailConfirmationEventHandler : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - {notification.Email} - {notification.Email}"), cancellationToken);
    }
}