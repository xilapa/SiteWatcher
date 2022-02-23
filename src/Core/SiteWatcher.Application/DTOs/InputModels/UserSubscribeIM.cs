using SiteWatcher.Application.Validators;

namespace SiteWatcher.Application.DTOs.InputModels;

public class UserSubscribeIM : IValidable<UserSubscribeIM>
{
    public string Name { get; set; }
    public string Email { get; set; }
}