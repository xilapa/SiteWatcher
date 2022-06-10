using System.Reflection;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.IntegrationTests.Utils;

public static class UserViewModelExtensions
{
    public static User ToModel(this UserViewModel userViewModel, DateTime currentDate)
    {
        var user = new User(userViewModel.GetGoogleId(), userViewModel.Name,
            userViewModel.Email, userViewModel.EmailConfirmed ? userViewModel.Email : "anotherEmail",
            userViewModel.Language, userViewModel.Theme, currentDate);

        var idProp = user.GetType()
            .BaseType!
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .SingleOrDefault(f =>
                f.Attributes.HasFlag(FieldAttributes.InitOnly) &&
                f.Name.Contains(nameof(User.Id)) &&
                f.FieldType == typeof(UserId)
            );
        idProp!.SetValue(user, userViewModel.Id);

        return user;
    }

    public static string GetGoogleId(this UserViewModel userViewModel) =>
        $"{userViewModel.Name}GoogleId";
}