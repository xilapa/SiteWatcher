using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Models;

public class User : BaseModel<Guid>
{
    // ctor for EF
    protected User() 
    {
        Id = Guid.NewGuid();
    }

    public User(string googleId, string name, string email, string authEmail, ELanguage language) : this()
    {
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = email == authEmail;
        Language = language;
    }

    public string GoogleId { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public ELanguage Language { get; private set; }
}