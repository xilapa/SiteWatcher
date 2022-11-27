using Domain.Alerts.DTOs;
using MediatR;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Application.Alerts.Commands.CreateAlert;

public class CreateAlertCommand : IRequest<DetailedAlertView>
{
    public string Name { get; set; } = null!;
    public Frequencies Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public WatchModes WatchMode { get; set; }

    // Term watch
    public string? Term { get; set; }

    // Regex watch
    public bool? NotifyOnDisappearance { get; set; }
    public string? RegexPattern { get; set; }

    public static implicit operator CreateAlertInput(CreateAlertCommand command) =>
        new(command.Name,
            command.Frequency,
            command.SiteName,
            command.SiteUri,
            command.WatchMode,
            command.Term,
            command.NotifyOnDisappearance,
            command.RegexPattern);
}

public class CreateAlertCommandHandler : IRequestHandler<CreateAlertCommand, DetailedAlertView>
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

    public async Task<DetailedAlertView> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = AlertFactory.Create(request, _session.UserId!.Value, _session.Now);
        _alertRepository.Add(alert);
        await _uow.SaveChangesAsync(cancellationToken);
        return DetailedAlertView.FromAlert(alert, _idHasher);
    }
}