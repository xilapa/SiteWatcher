using AutoMapper;
using Domain.DTOs.Alert;
using MediatR;
using SiteWatcher.Application.Common.Commands;
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
}

public class CreateAlertCommandHandler : IRequestHandler<CreateAlertCommand, CommandResult>
{
    private readonly IMapper _mapper;
    private readonly ISession _session;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _uow;

    public CreateAlertCommandHandler(IMapper mapper, ISession session, IAlertRepository alertRepository, IUnitOfWork uow)
    {
        _mapper = mapper;
        _session = session;
        _alertRepository = alertRepository;
        _uow = uow;
    }

    public async Task<CommandResult> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var alertInput = _mapper.Map<CreateAlertInput>(request);
        var alert = AlertFactory.Create(alertInput, _session.UserId!.Value, _session.Now);
        _alertRepository.Add(alert);
        await _uow.SaveChangesAsync(cancellationToken);
        return CommandResult.FromValue(_mapper.Map<DetailedAlertView>(alert));
    }
}