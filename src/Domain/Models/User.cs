using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Models;

public class User : BaseModel<Guid>
{
    // ctor for EF
    protected User() : base(Guid.NewGuid())
    { }

    public User(string googleId, string name, string email, string authEmail, ELanguage language) : this()
    {
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = email == authEmail;
        Language = language;
    }

    public string GoogleId { get; } = null!;
    public string Name { get; } = null!;
    public string Email { get; } = null!;
    public bool EmailConfirmed { get; }
    public ELanguage Language { get; }
}