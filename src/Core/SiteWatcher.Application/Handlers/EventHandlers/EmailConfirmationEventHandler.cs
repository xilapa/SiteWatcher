using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SiteWatcher.Application.Notifications;

namespace SiteWatcher.Application.Handlers.EventHandlers;

public class EmailConfirmationEventHandler : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        //TODO: enviar email para a fila utilizando outro escopo
        return Task.Run(() => Console.WriteLine($"ConfirmationEmail Event Handler: {notification.Name} - {notification.Email} - {notification.Email}"), cancellationToken);
    }
}