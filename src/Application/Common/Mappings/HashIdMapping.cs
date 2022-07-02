using AutoMapper;
using Domain.DTOs.Alert;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;

namespace SiteWatcher.Application.Common.Mappings;

public class HashIdMapping :
    IMappingAction<Alert, DetailedAlertView>,
    IMappingAction<WatchMode, DetailedWatchModeView>,
    IMappingAction<SimpleAlertViewDto, SimpleAlertView>
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
}