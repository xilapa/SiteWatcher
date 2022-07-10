using AutoMapper;
using Domain.DTOs.Alert;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;

namespace SiteWatcher.Application.Common.Mappings;

public class HashIdMapping :
    IMappingAction<Alert, DetailedAlertView>,
    IMappingAction<WatchMode, DetailedWatchModeView>,
    IMappingAction<SimpleAlertViewDto, SimpleAlertView>,
    IMappingAction<Alert, SimpleAlertView>,
    IMappingAction<AlertDetailsDto, AlertDetails>,
    IMappingAction<Alert, AlertDetails>
{
    private readonly IIdHasher _idHasher;

    public HashIdMapping(IIdHasher idHasher)
    {
        _idHasher = idHasher;
    }

    public void Process(WatchMode source, DetailedWatchModeView destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id.Value);
    }

    public void Process(Alert source, DetailedAlertView destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id.Value);
    }

    public void Process(SimpleAlertViewDto source, SimpleAlertView destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id);
    }

    public void Process(Alert source, SimpleAlertView destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id.Value);
    }

    public void Process(AlertDetailsDto source, AlertDetails destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id);
        destination.WatchModeId = _idHasher.HashId(source.WatchModeId);
    }

    public void Process(Alert source, AlertDetails destination, ResolutionContext context)
    {
        destination.Id = _idHasher.HashId(source.Id.Value);
        destination.WatchModeId = _idHasher.HashId(source.WatchMode.Id.Value);
    }
}