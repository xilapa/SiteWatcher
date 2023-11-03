using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Alerts.EventHandlers;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Errors;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.DeleteAlert;

public class DeleteAlertCommand
{
    public string? AlertId { get; set; }
}

public class DeleteAlertCommandHandler : IApplicationHandler
{
    private readonly ISiteWatcherContext _context;
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly AlertsChangedEventHandler _alertsChangedEventHandler;

    public DeleteAlertCommandHandler(ISiteWatcherContext context, IIdHasher idHasher, ISession session,
        AlertsChangedEventHandler alertsChangedEventHandler)
    {
        _context = context;
        _idHasher = idHasher;
        _session = session;
        _alertsChangedEventHandler = alertsChangedEventHandler;
    }

    public async Task<Result> Handle(DeleteAlertCommand request, CancellationToken cancellationToken)
    {
        if(request.AlertId == null)
            return ReturnError();

        var alertIdInt = _idHasher.DecodeId(request.AlertId);
        if (alertIdInt == 0)
            return ReturnError();

        var alertId = new AlertId(alertIdInt);

        var deleted = await _context.Alerts
            .Where(a => a.Id == alertId && a.UserId == _session.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted != 0)
            await _alertsChangedEventHandler.Handle(new AlertsChangedEvent(_session.UserId!.Value), CancellationToken.None);

        return deleted != 0 ? Result.Empty : ReturnError();
    }

    private static Error ReturnError() =>
      Error.Validation(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
}