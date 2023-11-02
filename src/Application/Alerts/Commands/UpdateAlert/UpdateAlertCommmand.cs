using FluentValidation;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Common.Results;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.Errors;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

public class UpdateAlertCommmand
{
    public string AlertId { get; set; } = null!;
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<Frequencies>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<Rules>? Rule { get; set; }

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

public class UpdateAlertCommandHandler : BaseHandler<UpdateAlertCommmand, Result<DetailedAlertView>>
{
    private readonly IIdHasher _idHasher;
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public UpdateAlertCommandHandler(IIdHasher idHasher, ISiteWatcherContext context, ISession session,
        IValidator<UpdateAlertCommmand> validator) : base(validator)
    {
        _idHasher = idHasher;
        _context = context;
        _session = session;
    }

    protected override async Task<Result<DetailedAlertView>> HandleCommand(UpdateAlertCommmand command, CancellationToken ct)
    {
        var updateInfo = command.ToUpdateAlertInput(_idHasher);

        if (AlertId.Empty.Equals(updateInfo.AlertId) || updateInfo.AlertId.Value == 0)
            return Error.Validation(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        var alert = await _context.GetAlertForUpdateAsync(updateInfo.AlertId, _session.UserId!.Value, ct);
        if (alert is null) return Error.Validation(ApplicationErrors.ALERT_DO_NOT_EXIST);

        alert.Update(updateInfo, _session.Now);
        await _context.SaveChangesAsync(CancellationToken.None);

        return DetailedAlertView.FromAlert(alert, _idHasher);
    }
}