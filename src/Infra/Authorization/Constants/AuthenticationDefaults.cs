namespace SiteWatcher.Infra.Authorization.Constants;

public static class AuthenticationDefaults
{
    public const string RegisterScheme = "register-auth";
    public const string Roles = "roles";
    public const string State = "state";

    public static class ClaimTypes
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Email = "email";
        public const string EmailConfirmed = "email-confirmed";
        public const string GoogleId = "googleId";
        public const string Locale = "locale";
        public const string Language = "language";
    }

    public static class Google
    {
        public const string Id = "sub";
    }
}