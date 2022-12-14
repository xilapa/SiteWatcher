using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Repositories;
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
    private readonly IAlertRepository _alertRepository;
    private readonly ISession _session;
    private readonly IUnitOfWork _uow;

    public UpdateAlertCommandHandler(IIdHasher idHasher, IAlertRepository alertRepository, ISession session,
        IUnitOfWork uow)
    {
        _idHasher = idHasher;
        _alertRepository = alertRepository;
        _session = session;
        _uow = uow;
    }

    public async Task<CommandResult> Handle(UpdateAlertCommmand request, CancellationToken cancellationToken)
    {
        var updateInfo = request.ToUpdateAlertInput(_idHasher);

        if (AlertId.Empty.Equals(updateInfo.AlertId) || updateInfo.AlertId.Value == 0)
            return CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        var alert = await _alertRepository.GetAlertForUpdate(updateInfo.AlertId, _session.UserId!.Value);
        if (alert is null)
            return CommandResult.FromError(ApplicationErrors.ALERT_DO_NOT_EXIST);

        alert.Update(updateInfo, _session.Now);
        await _uow.SaveChangesAsync(CancellationToken.None);

        return CommandResult.FromValue(DetailedAlertView.FromAlert(alert, _idHasher));
    }
}