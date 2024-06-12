using Mediator;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;

namespace SiteWatcher.Application.Alerts.Commands.CreateAlert;

public class CreateAlertCommand : ICommand<DetailedAlertView>
{
    public string Name { get; set; } = null!;
    public Frequencies Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public RuleType Rule { get; set; }

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
            command.Rule,
            command.Term,
            command.NotifyOnDisappearance,
            command.RegexPattern);
}

public class CreateAlertCommandHandler : ICommandHandler<CreateAlertCommand, DetailedAlertView>
{
    private readonly ISession _session;
    private readonly ISiteWatcherContext _context;
    private readonly IIdHasher _idHasher;

    public CreateAlertCommandHandler(ISession session, ISiteWatcherContext context, IIdHasher idHasher)
    {
        _session = session;
        _context = context;
        _idHasher = idHasher;
    }

    public async ValueTask<DetailedAlertView> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = AlertFactory.Create(request, _session.UserId!.Value, _session.Now);
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return DetailedAlertView.FromAlert(alert, _idHasher);
    }
}