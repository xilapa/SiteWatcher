using Domain.Events;
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
    public string? SecurityStamp { get; private set;}

    public void Update(UpdateUserInput updatedValues, DateTime updateDate)
    {
        Name = updatedValues.Name;
        if (EmailConfirmed) EmailConfirmed = updatedValues.Email == Email;
        Email = updatedValues.Email;
        Language = updatedValues.Language;
        Theme = updatedValues.Theme;
        LastUpdatedAt = updateDate;
    }

    public void Deactivate(DateTime deactivateDate)
    {
        Active = false;
        LastUpdatedAt = deactivateDate;
    }

    public void GenerateEmailConfirmation(DateTime currentDate)
    {
        SecurityStamp = Utils.Utils.GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        AddDomainEvent(new EmailConfirmationTokenGeneratedEvent(this));
    }

    public bool ConfirmEmail(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            EmailConfirmed = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;

        return EmailConfirmed;
    }

    public void GenerateUserActivationToken(DateTime currentDate)
    {
        SecurityStamp = Utils.Utils.GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        AddDomainEvent(new UserReactivationTokenGeneratedEvent(this));
    }

    public bool ReactivateAccount(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            Active = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;

        return Active;
    }
}