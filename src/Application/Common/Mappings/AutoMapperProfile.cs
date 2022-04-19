using AutoMapper;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, User>()
            .ConstructUsing(src => new User(src.GoogleId!, src.Name!, src.Email!, src.AuthEmail!, src.Language));

        CreateMap<User, UserRegisteredNotification>();
    }
}