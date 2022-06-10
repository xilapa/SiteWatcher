using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.IntegrationTests.Utils;

public static class Users
{
    public static UserViewModel Xilapa = new()
    {
        Id = new UserId(new Guid("00000000-0000-0000-0000-000000000001")),
        Active = true,
        Email = "xilapa@email.com",
        Language = ELanguage.English,
        Name = nameof(Xilapa),
        Theme = ETheme.Dark,
        EmailConfirmed = true
    };

    public static UserViewModel Xulipa = new ()
    {
        Id = new UserId(new Guid("00000000-0000-0000-0000-000000000002")),
        Active = true,
        Email = "xulipa@email.com",
        Language = ELanguage.English,
        Name = nameof(Xulipa),
        Theme = ETheme.Dark,
        EmailConfirmed = false
    };
}