using Domain.DTOs.Alerts;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Application.Alerts.Commands.CreateAlert;

public class CreateAlertCommand : IRequest<CommandResult>
{
    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; }
    public string SiteUri { get; set; }
    public EWatchMode WatchMode { get; set; }

    // Term watch
    public string? Term { get; set; }

    public static implicit operator CreateAlertInput(CreateAlertCommand command) =>
        new(command.Name,
            command.Frequency,
            command.SiteName,
            command.SiteUri,
            command.WatchMode,
            command.Term);
}

public class CreateAlertCommandHandler : IRequestHandler<CreateAlertCommand, CommandResult>
{
    private readonly ISession _session;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _uow;
    private readonly IIdHasher _idHasher;

    public CreateAlertCommandHandler(ISession session, IAlertRepository alertRepository, IUnitOfWork uow,
        IIdHasher idHasher)
    {
        _session = session;
        _alertRepository = alertRepository;
        _uow = uow;
        _idHasher = idHasher;
    }

    public async Task<CommandResult> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = AlertFactory.Create(request, _session.UserId!.Value, _session.Now);
        _alertRepository.Add(alert);
        await _uow.SaveChangesAsync(cancellationToken);
        return CommandResult.FromValue(alert.ToDetailedAlertView(_idHasher));
    }
}