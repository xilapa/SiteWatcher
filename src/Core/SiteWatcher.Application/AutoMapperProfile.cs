using AutoMapper;
using SiteWatcher.Application.Commands;
using SiteWatcher.Application.Notifications;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterUserCommand, User>()
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.Email == src.AuthEmail));

        CreateMap<User, UserRegisteredNotification>();
    }
}