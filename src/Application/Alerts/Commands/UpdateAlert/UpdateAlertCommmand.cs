﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

public class UpdateAlertCommmand : IRequest<CommandResult>
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

public class UpdateAlertCommandHandler : IRequestHandler<UpdateAlertCommmand, CommandResult>
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

    public async Task<CommandResult> Handle(UpdateAlertCommmand request, CancellationToken cancellationToken)
    {
        var updateInfo = request.ToUpdateAlertInput(_idHasher);

        if (AlertId.Empty.Equals(updateInfo.AlertId) || updateInfo.AlertId.Value == 0)
            return CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        var alertDto = await _context.Alerts
            .Where(a => a.Id == updateInfo.AlertId && a.UserId == _session.UserId && a.Active)
            .Select(alert => new UpdateAlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                CreatedAt = alert.CreatedAt,
                Name = alert.Name,
                Frequency = alert.Frequency,
                SiteName = alert.Site.Name,
                SiteUri = alert.Site.Uri,
                Rule = alert.Rule,
                LastVerification = alert.LastVerification
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (alertDto is null)
            return CommandResult.FromError(ApplicationErrors.ALERT_DO_NOT_EXIST);

        var alert = Alert.GetModelForUpdate(alertDto);
        // Attach alert and site, because the rule is already tracked
        _context.Attach(alert);

        alert.Update(updateInfo, _session.Now);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CommandResult.FromValue(DetailedAlertView.FromAlert(alert, _idHasher));
    }
}