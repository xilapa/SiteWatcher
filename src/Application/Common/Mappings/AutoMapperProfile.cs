using AutoMapper;
using Domain.DTOs.Alert;
using Domain.Events;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;

namespace SiteWatcher.Application.Common.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, RegisterUserInput>();
        CreateMap<UpdateUserCommand, UpdateUserInput>();

        CreateMap<ISession, AccountDeletedEvent>()
            .ForMember(opt => opt.Name, opt => opt.MapFrom(src => src.UserName));

        CreateMap<CreateAlertCommand, CreateAlertInput>();
        CreateMap<Alert, DetailedAlertView>()
            .AfterMap<HashIdMapping>();

        CreateMap<WatchMode, DetailedWatchModeView>()
            .ConstructUsing(src => DetailedWatchModeView.FromModel(src))
            .AfterMap<HashIdMapping>();

        CreateMap<Site, SiteView>();
    }
}