using SiteWatcher.Domain.Enums;
using System.Linq;
using SiteWatcher.Domain.Extensions;

namespace SiteWatcher.Application.DTOs.InputModels;

public class UserRegisterViewModel
{
    public UserRegisterViewModel(string googleId, string name, string email, string locale)
    {
        this.GoogleId = googleId;
        this.Name = name;
        this.Email = email;
        this.Language = locale.Split("-").First().GetEnumValue<ELanguage>();
    }

    public string GoogleId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
}