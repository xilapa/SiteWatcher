using AutoMapper;
using Domain.Events;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.DTOs.User;

namespace SiteWatcher.Application.Common.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, RegisterUserInput>();
        CreateMap<UpdateUserCommand, UpdateUserInput>();

        CreateMap<ISession, AccountDeletedEvent>()
            .ForMember(opt => opt.Name, opt => opt.MapFrom(src => src.UserName));

        CreateMap<ISession, AccountDeactivatedNotification>()
            .ForMember(opt => opt.Name, opt => opt.MapFrom(src => src.UserName));

        CreateMap<UserViewModel, AccountReactivationEmailNotification>();
    }
}