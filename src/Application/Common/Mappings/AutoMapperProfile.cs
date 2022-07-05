using AutoMapper;
using Domain.DTOs.Alert;
using Domain.Events;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
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
            .ConstructUsing(src => DetailedWatchModeView.FromModel(src))
            .AfterMap<HashIdMapping>();

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
    }
}