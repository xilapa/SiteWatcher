using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.IntegrationTests.Utils;

public static class Users
{
    public static UserViewModel Xilapa = new()
    {
        Id = new UserId(new Guid("00000000-0000-0000-0000-000000000001")),
        Active = true,
        Email = "xilapa@email.com",
        Language = Language.English,
        Name = nameof(Xilapa),
        Theme = Theme.Dark,
        EmailConfirmed = true
    };

    public static UserViewModel Xulipa = new ()
    {
        Id = new UserId(new Guid("00000000-0000-0000-0000-000000000002")),
        Active = true,
        Email = "xulipa@email.com",
        Language = Language.English,
        Name = nameof(Xulipa),
        Theme = Theme.Dark,
        EmailConfirmed = false
    };
}