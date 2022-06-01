using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Models;

public class User : BaseModel<UserId>
{
    // ctor for EF
    protected User() : base(new UserId(Guid.NewGuid()))
    { }

    public User(string googleId, string name, string email, string authEmail, ELanguage language, ETheme theme) : this()
    {
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = email == authEmail;
        Language = language;
        Theme = theme;
    }

    public string GoogleId { get; } = null!;
    public string Name { get; private set; } = null!;
    public string Email { get; private set;} = null!;
    public bool EmailConfirmed { get; private set;}
    public ELanguage Language { get; private set;}
    public ETheme Theme { get; private set;}

    public void Update(UpdateUserInput updatedValues, DateTime updateDate)
    {
        Name = updatedValues.Name;
        if (EmailConfirmed) EmailConfirmed = updatedValues.Email == Email;
        Email = updatedValues.Email;
        Language = updatedValues.Language;
        Theme = updatedValues.Theme;
        LastUpdatedAt = updateDate;
    }
}