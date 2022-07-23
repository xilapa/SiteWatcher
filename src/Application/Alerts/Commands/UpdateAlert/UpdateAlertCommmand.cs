using AutoMapper;
using Domain.DTOs.Alert;
using Domain.DTOs.Common;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

public class UpdateAlertCommmand : IRequest<ICommandResult<DetailedAlertView>>
{
    public string AlertId { get; set; }
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<EFrequency>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<EWatchMode>?WatchMode { get; set; }
    public UpdateInfo<string?>? Term { get; set; }
}

public class UpdateAlertCommmandHandler : IRequestHandler<UpdateAlertCommmand, ICommandResult<DetailedAlertView>>
{
    private readonly IMapper _mapper;
    private readonly IAlertRepository _alertRepository;
    private readonly ISession _session;
    private readonly IUnitOfWork _uow;

    public UpdateAlertCommmandHandler(IMapper mapper, IAlertRepository alertRepository, ISession session, IUnitOfWork uow)
    {
        _mapper = mapper;
        _alertRepository = alertRepository;
        _session = session;
        _uow = uow;
    }

    public async Task<ICommandResult<DetailedAlertView>> Handle(UpdateAlertCommmand request, CancellationToken cancellationToken)
    {
        var updateInfo = _mapper.Map<UpdateAlertInput>(request);

        if (AlertId.Empty.Equals(updateInfo.AlertId) || updateInfo.AlertId.Value == 0)
            return new CommandResult<DetailedAlertView>().WithError(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        var alert = await _alertRepository.GetAlertForUpdate(updateInfo.AlertId, _session.UserId!.Value);
        if (alert is null)
            return new CommandResult<DetailedAlertView>().WithError(ApplicationErrors.ALERT_DO_NOT_EXIST);

        alert.Update(updateInfo, _session.Now);
        await _uow.SaveChangesAsync(CancellationToken.None);

        return new CommandResult<DetailedAlertView>(_mapper.Map<DetailedAlertView>(alert));
    }
}