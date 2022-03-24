using System.Text.Json;

namespace SiteWatcher.WebAPI.Constants;

public static class AuthenticationDefaults
{
    public const string RegisterScheme = "register-auth";
    public const string Roles = "roles";
    public const string State = "state";
    
    public static class ClaimTypes 
    {
        public static readonly string Id = "id";
        public static readonly string Name = "name";
        public static readonly string Email = "email";
        public static readonly string EmailConfirmed = "email-confirmed";
        public static readonly string GoogleId = "googleId";
        public static readonly string Locale = "locale";
        public static readonly string Language = "language";
    }

    public static class Google 
    {
        public static readonly string Id = "sub";
    }
}