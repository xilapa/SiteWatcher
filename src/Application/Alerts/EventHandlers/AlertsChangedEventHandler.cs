﻿using Domain.Events.Alerts;
using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsChangedEventHandler : INotificationHandler<AlertsChangedEvent>
{
    private readonly IFireAndForgetService _fireAndForgetService;

    public AlertsChangedEventHandler(IFireAndForgetService fireAndForgetService)
    {
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(AlertsChangedEvent notification, CancellationToken cancellationToken)
    {
        _fireAndForgetService.ExecuteWith<ICache>(cache =>
            cache.DeleteKeyAsync(CacheKeys.UserAlerts(notification.UserId)));
        return Task.CompletedTask;
    }
}