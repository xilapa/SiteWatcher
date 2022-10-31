using Domain.Events;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Domain.Models;

public class User : BaseModel<UserId>
{
    // ctor for EF
    protected User() : base()
    { }

    public User(string googleId, string name, string email, string authEmail, ELanguage language, ETheme theme,
        DateTime currentDate) : base(new UserId(Guid.NewGuid()), currentDate)
    {
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = email == authEmail;
        Language = language;
        Theme = theme;
        Alerts = Enumerable.Empty<Alert>();

        GenerateEmailConfirmationToken(currentDate);
    }

    public string GoogleId { get; } = null!;
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; }
    public ELanguage Language { get; private set; }
    public ETheme Theme { get; private set; }
    public string? SecurityStamp { get; private set; }
    public IEnumerable<Alert> Alerts { get; init; }

    public void Update(UpdateUserInput updatedValues, DateTime updateDate)
    {
        Name = updatedValues.Name;
        if (EmailConfirmed) EmailConfirmed = updatedValues.Email == Email;
        Email = updatedValues.Email;
        Language = updatedValues.Language;
        Theme = updatedValues.Theme;
        LastUpdatedAt = updateDate;

        GenerateEmailConfirmationToken(updateDate);
    }

    public void Deactivate(DateTime deactivateDate)
    {
        Active = false;
        LastUpdatedAt = deactivateDate;
    }

    public void GenerateEmailConfirmationToken(DateTime currentDate)
    {
        if (EmailConfirmed) return;
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

    public static User FromInputModel(RegisterUserInput registerInput, DateTime currentDate)
    {
        return new User(registerInput.GoogleId, registerInput.Name, registerInput.Email, registerInput.AuthEmail,
            registerInput.Language, registerInput.Theme, currentDate);
    }
}