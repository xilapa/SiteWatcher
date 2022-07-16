using AutoMapper;
using Domain.DTOs.Alert;
using Domain.DTOs.Common;
using Domain.Events;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Common.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, RegisterUserInput>();
        CreateMap<UpdateUserCommand, UpdateUserInput>();

        CreateMap<ISession, AccountDeletedEvent>()
            .ForMember(opt => opt.Name, opt =>
                opt.MapFrom(src => src.UserName));

        CreateMap<CreateAlertCommand, CreateAlertInput>();
        CreateMap<Alert, DetailedAlertView>()
            .AfterMap<HashIdMapping>();

        CreateMap<WatchMode, DetailedWatchModeView>()
            .ConstructUsing(src => DetailedWatchModeView.FromModel(src));

        CreateMap<Site, SiteView>();

        CreateMap<SimpleAlertViewDto, SimpleAlertView>()
            .ForMember(opt => opt.WatchMode, opt =>
                opt.MapFrom(src => Utils.GetWatchModeEnumByTableDiscriminator(src.WatchMode)))
            .AfterMap<HashIdMapping>();

        CreateMap<Alert, SimpleAlertView>()
            .ForMember(opt => opt.SiteName, opt =>
                opt.MapFrom(src => src.Site.Name))
            .ForMember(opt => opt.WatchMode, opt =>
                opt.MapFrom(src => Utils.GetWatchModeEnumByType(src.WatchMode)))
            .AfterMap<HashIdMapping>();

        CreateMap(typeof(PaginatedList<>), typeof(PaginatedList<>));

        CreateMap<AlertDetailsDto, AlertDetails>()
            .AfterMap<HashIdMapping>();

        CreateMap<Alert, AlertDetails>()
            .ForMember(a => a.SiteUri,
                opt => opt.MapFrom(src => src.Site.Uri))
            .AfterMap<HashIdMapping>()
            .AfterMap((alert, alertDetails) => alertDetails.Term = (alert.WatchMode as TermWatch)?.Term);

        CreateMap<UpdateAlertCommmand, UpdateAlertInput>()
            .ForMember(m => m.AlertId, opt => opt.Ignore())
            .AfterMap<HashIdMapping>();
    }
}