using SiteWatcher.Application.Validators;

namespace SiteWatcher.Application.DTOS.InputModels;

public class UserSubscribeIM : IValidable<UserSubscribeIM>
{
    public string Name { get; set; }
    public string Email { get; set; }
}