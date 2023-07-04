using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Events;
using SiteWatcher.Domain.Users.Messages;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Users;

public class User : BaseModel<UserId>
{
    // ctor for EF
    protected User() : base()
    {
        Alerts = new List<Alert>();
        Emails = new List<Email>();
    }

    public User(string googleId, string name, string email, string authEmail, Language language, Theme theme,
        DateTime currentDate) : base(UserId.New(), currentDate)
    {
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = email == authEmail;
        Language = language;
        Theme = theme;
        Alerts = new List<Alert>();
        Emails = new List<Email>();

        GenerateEmailConfirmationToken(currentDate);
    }

    public string GoogleId { get; } = null!;
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; }
    public Language Language { get; private set; }
    public Theme Theme { get; private set; }
    public string? SecurityStamp { get; private set; }
    public ICollection<Alert> Alerts { get; init; }
    public ICollection<Email> Emails { get; set; }

    public void Update(UpdateUserInput updatedValues, DateTime updateDate)
    {
        Name = updatedValues.Name;
        if (EmailConfirmed) EmailConfirmed = updatedValues.Email == Email;
        Email = updatedValues.Email;
        Language = updatedValues.Language;
        Theme = updatedValues.Theme;
        LastUpdatedAt = updateDate;

        GenerateEmailConfirmationToken(updateDate);
        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void Deactivate(DateTime deactivateDate)
    {
        Active = false;
        LastUpdatedAt = deactivateDate;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }

    public void GenerateEmailConfirmationToken(DateTime currentDate)
    {
        if (EmailConfirmed) return;
        SecurityStamp = GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        AddMessage(new EmailConfirmationTokenGeneratedMessage(this, currentDate));
    }

    public bool ConfirmEmail(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            EmailConfirmed = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;
        AddDomainEvent(new UserUpdatedEvent(Id));

        return EmailConfirmed;
    }

    public void GenerateUserActivationToken(DateTime currentDate)
    {
        SecurityStamp = GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        AddMessage(new UserReactivationTokenGeneratedMessage(this, currentDate));
    }

    public bool ReactivateAccount(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            Active = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;
        AddDomainEvent(new UserUpdatedEvent(Id));

        return Active;
    }

    public static User FromInputModel(RegisterUserInput registerInput, DateTime currentDate)
    {
        return new User(registerInput.GoogleId, registerInput.Name, registerInput.Email, registerInput.AuthEmail,
            registerInput.Language, registerInput.Theme, currentDate);
    }
}