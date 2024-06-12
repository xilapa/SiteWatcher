using Mediator;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

public class UpdateAlertCommmand : ICommand<CommandResult>
{
    public string AlertId { get; set; } = null!;
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<Frequencies>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<RuleType>? Rule { get; set; }

    // Term watch
    public UpdateInfo<string>? Term { get; set; }

    // Regex watch
    public UpdateInfo<bool>? NotifyOnDisappearance { get; set; }
    public UpdateInfo<string>? RegexPattern { get; set; }

    public UpdateAlertInput ToUpdateAlertInput(IIdHasher idHasher)
    {
        var id = new AlertId(idHasher.DecodeId(AlertId));
        return new UpdateAlertInput(
            id,
            Name,
            Frequency,
            SiteName,
            SiteUri,
            Rule,
            Term,
            NotifyOnDisappearance,
            RegexPattern
        );
    }
}

public class UpdateAlertCommandHandler : ICommandHandler<UpdateAlertCommmand, CommandResult>
{
    private readonly IIdHasher _idHasher;
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public UpdateAlertCommandHandler(IIdHasher idHasher, ISiteWatcherContext context, ISession session)
    {
        _idHasher = idHasher;
        _context = context;
        _session = session;
    }

    public async ValueTask<CommandResult> Handle(UpdateAlertCommmand request, CancellationToken cancellationToken)
    {
        var updateInfo = request.ToUpdateAlertInput(_idHasher);

        if (AlertId.Empty.Equals(updateInfo.AlertId) || updateInfo.AlertId.Value == 0)
            return CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        var alert = await _context.GetAlertForUpdateAsync(updateInfo.AlertId, _session.UserId!.Value, cancellationToken);
        if (alert is null) return CommandResult.FromError(ApplicationErrors.ALERT_DO_NOT_EXIST);

        alert.Update(updateInfo, _session.Now);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CommandResult.FromValue(DetailedAlertView.FromAlert(alert, _idHasher));
    }
}