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

    public static (User, EmailConfirmationTokenGeneratedMessage?) Create(RegisterUserInput registerInput, DateTime currentDate)
    {
        var user = new User(registerInput.GoogleId, registerInput.Name, registerInput.Email, registerInput.AuthEmail,
            registerInput.Language, registerInput.Theme, currentDate);
        var emailConfirmationMessage = user.GenerateEmailConfirmationToken(currentDate);
        return (user, emailConfirmationMessage);
    }

    private User(string googleId, string name, string email, string authEmail, Language language, Theme theme,
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

    public (UserUpdatedEvent, EmailConfirmationTokenGeneratedMessage?) Update(UpdateUserInput updatedValues, DateTime updateDate)
    {
        Name = updatedValues.Name;
        if (EmailConfirmed) EmailConfirmed = updatedValues.Email == Email;
        Email = updatedValues.Email;
        Language = updatedValues.Language;
        Theme = updatedValues.Theme;
        LastUpdatedAt = updateDate;

        return (new UserUpdatedEvent(Id), GenerateEmailConfirmationToken(updateDate));
    }

    public UserUpdatedEvent Deactivate(DateTime deactivateDate)
    {
        Active = false;
        LastUpdatedAt = deactivateDate;
        return new UserUpdatedEvent(Id);
    }

    public EmailConfirmationTokenGeneratedMessage? GenerateEmailConfirmationToken(DateTime currentDate)
    {
        if (EmailConfirmed) return null;
        SecurityStamp = GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        return new EmailConfirmationTokenGeneratedMessage(this, currentDate);
    }

    public (bool emailConfirmed, UserUpdatedEvent) ConfirmEmail(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            EmailConfirmed = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;

        return (EmailConfirmed, new UserUpdatedEvent(Id));
    }

    public UserReactivationTokenGeneratedMessage? GenerateReactivationToken(DateTime currentDate)
    {
        if (Active) return null;
        SecurityStamp = GenerateSafeRandomBase64String();
        LastUpdatedAt = currentDate;
        return new UserReactivationTokenGeneratedMessage(this, currentDate);
    }

    public (bool active, UserUpdatedEvent)  ReactivateAccount(string token, DateTime currentDate)
    {
        if (token == SecurityStamp)
            Active = true;

        SecurityStamp = null;
        LastUpdatedAt = currentDate;

        return (Active, new UserUpdatedEvent(Id));
    }
}