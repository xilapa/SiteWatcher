using Domain.Events.Alerts;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Alerts.Commands.DeleteAlert;

public class DeleteAlertCommand : Validable<DeleteAlertCommand>, IRequest<ICommandResult<object>>
{
    public string AlertId { get; set; }
}

public class DeleteAlertCommandHandler : IRequestHandler<DeleteAlertCommand, ICommandResult<object>>
{
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly IIdHasher _idHasher;
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public DeleteAlertCommandHandler(IAlertDapperRepository alertDapperRepository, IIdHasher idHasher, ISession session,
        IMediator mediator)
    {
        _alertDapperRepository = alertDapperRepository;
        _idHasher = idHasher;
        _session = session;
        _mediator = mediator;
    }

    public async Task<ICommandResult<object>> Handle(DeleteAlertCommand request, CancellationToken cancellationToken)
    {
        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return new CommandResult<object>().WithError(
                ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));

        var deleted = await _alertDapperRepository
            .DeleteUserAlert(alertId, _session.UserId!.Value, cancellationToken);

        if (deleted)
            await _mediator.Publish(new AlertsChangedEvent(_session.UserId.Value));

        return deleted
            ? new CommandResult<object>()
            : new CommandResult<object>().WithError(
                ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
    }
}