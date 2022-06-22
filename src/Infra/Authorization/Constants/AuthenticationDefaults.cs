namespace SiteWatcher.Infra.Authorization.Constants;

public static class AuthenticationDefaults
{
    public const string Roles = "roles";
    public const string State = "state";
    public const string GoogleAuthClient = nameof(GoogleAuthClient);

    public static class ClaimTypes
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Email = "email";
        public const string EmailConfirmed = "email-confirmed";
        public const string GoogleId = "googleId";
        public const string Locale = "locale";
        public const string Language = "language";
        public const string Theme = "theme";
    }

    public static class Google
    {
        public const string Id = "sub";
        public const string Picture = "picture";
    }

    public static class Issuers
    {
        public const string Login = nameof(Login);
        public const string Register = nameof(Register);
    }

    public static class Schemes
    {
        public const string Login = "login-auth";
        public const string Register = "register-auth";
    }
}