namespace SiteWatcher.Infra.Authorization.Constants;

public static class AuthenticationDefaults
{
    public const string Roles = "roles";
    public const string AuthtokePayloadKey = nameof(AuthtokePayloadKey);
    public const string UserIdKey = nameof(UserIdKey);
    public const string ClaimsKey = nameof(ClaimsKey);
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
        public const string ProfilePicUrl = "profile-pic-url";
    }

    public static class Google
    {
        public const string Id = "sub";
        public const string Picture = "picture";
        public const string Locale = "locale";
    }

    public static class Issuers
    {
        public const string Login = nameof(Login);
        public const string Register = nameof(Register);
    }

    public static class Schemes
    {
        public const string Google = "google";
        public const string Cookie = "cookie";
        public const string Login = "login-auth";
        public const string Register = "register-auth";
    }
}