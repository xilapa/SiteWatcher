using ReflectionMagic;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.IntegrationTests.Utils;

public static class UserViewModelExtensions
{
    public static User ToModel(this UserViewModel userViewModel, DateTime currentDate)
    {
        var registerInput = new RegisterUserInput(userViewModel.Name, userViewModel.Email, userViewModel.Language,
            userViewModel.Theme, userViewModel.GetGoogleId(),
            userViewModel.EmailConfirmed ? userViewModel.Email : "anotherEmail");
        var (user, _) = User.Create(registerInput, currentDate);

        user.AsDynamic().Id = userViewModel.Id;
        return user;
    }

    public static string GetGoogleId(this UserViewModel userViewModel) =>
        $"{userViewModel.Name}GoogleId";
}