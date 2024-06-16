using Mediator;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.DeleteAlert;

public class DeleteAlertCommand : ICommand<CommandResult>
{
    public string? AlertId { get; set; }
}

public class DeleteAlertCommandHandler : ICommandHandler<DeleteAlertCommand, CommandResult>
{
    private readonly ISiteWatcherContext _context;
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public DeleteAlertCommandHandler(ISiteWatcherContext context, IIdHasher idHasher, ISession session,
        IMediator mediator)
    {
        _context = context;
        _idHasher = idHasher;
        _session = session;
        _mediator = mediator;
    }

    public async ValueTask<CommandResult> Handle(DeleteAlertCommand request, CancellationToken cancellationToken)
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
            await _mediator.Publish(new AlertsChangedEvent(_session.UserId!.Value), CancellationToken.None);

        return deleted != 0 ? CommandResult.Empty() : ReturnError();
    }

    private static ErrorResult ReturnError() =>
        CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
}