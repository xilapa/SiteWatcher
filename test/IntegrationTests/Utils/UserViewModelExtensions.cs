using ReflectionMagic;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.IntegrationTests.Utils;

public static class UserViewModelExtensions
{
    public static User ToModel(this UserViewModel userViewModel, DateTime currentDate)
    {
        var user = new User(userViewModel.GetGoogleId(), userViewModel.Name,
            userViewModel.Email, userViewModel.EmailConfirmed ? userViewModel.Email : "anotherEmail",
            userViewModel.Language, userViewModel.Theme, currentDate);

        user.AsDynamic().Id = userViewModel.Id;
        return user;
    }

    public static string GetGoogleId(this UserViewModel userViewModel) =>
        $"{userViewModel.Name}GoogleId";
}