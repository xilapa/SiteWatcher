using AutoMapper;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.DeleteUser;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Common.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, User>()
            .ConstructUsing(src => new User(src.GoogleId!, src.Name!, src.Email!, src.AuthEmail!, src.Language, src.Theme));
        CreateMap<UpdateUserCommand, UpdateUserInput>();

        CreateMap<User, UserRegisteredNotification>();
        CreateMap<User, UserUpdatedNotification>();
        CreateMap<ISessao, AccountDeletedNotification>()
            .ForMember(opt => opt.Name, opt => opt.MapFrom(src => src.UserName));

        CreateMap<ISessao, AccountDeactivatedNotification>()
            .ForMember(opt => opt.Name, opt => opt.MapFrom(src => src.UserName));

        CreateMap<UserViewModel, AccountActivationNotification>();
    }
}