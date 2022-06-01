using AutoMapper;
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
    }
}