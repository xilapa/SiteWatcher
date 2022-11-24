﻿using Domain.Events.Alerts;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Alerts.Commands.DeleteAlert;

public class DeleteAlertCommand : IRequest<CommandResult>
{
    public string? AlertId { get; set; }
}

public class DeleteAlertCommandHandler : IRequestHandler<DeleteAlertCommand, CommandResult>
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

    public async Task<CommandResult> Handle(DeleteAlertCommand request, CancellationToken cancellationToken)
    {
        if(request.AlertId == null)
            return ReturnError();

        var alertId = _idHasher.DecodeId(request.AlertId);
        if (alertId == 0)
            return ReturnError();

        var deleted = await _alertDapperRepository
            .DeleteUserAlert(alertId, _session.UserId!.Value, cancellationToken);

        if (deleted)
            await _mediator.Publish(new AlertsChangedEvent(_session.UserId.Value), CancellationToken.None);

        return deleted ? CommandResult.Empty() : ReturnError();
    }

    private static CommandResult ReturnError() =>
        CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
}