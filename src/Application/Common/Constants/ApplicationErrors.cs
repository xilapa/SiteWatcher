namespace SiteWatcher.Application.Common.Constants;

public static class ApplicationErrors
{
    // TODO: Translate error messages at the fronteend
    public static readonly string INTERNAL_ERROR = "An error has occurred.";
    public static readonly string NAME_NOT_BE_NULL_OR_EMPTY = "Username not be null or empty.";
    public static readonly string NAME_MINIMUM_LENGTH = "Username length should be at least 3.";
    public static readonly string NAME_MUST_HAVE_ONLY_LETTERS = "Username must only have letters.";
    public static readonly string EMAIL_NOT_BE_NULL_OR_EMPTY = "Email not be null or empty.";
    public static readonly string EMAIL_IS_INVALID = "Email is not valid.";
    public static readonly string LANGUAGE_IS_INVALID = "Language is not valid";
    public static readonly string THEME_IS_INVALID = "Theme is not valid";
    public static readonly string USER_DO_NOT_EXIST = "User does not exist.";
    public static readonly string GOOGLE_AUTH_ERROR = "An error has ocurred during Google Authorization";
    public static readonly string INVALID_TOKEN = "Invalid token.";
}