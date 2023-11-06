namespace SiteWatcher.Application.Common.Constants;

public static class ApplicationErrors
{
    // TODO: Translate error messages at the fronteend
    public static readonly string INTERNAL_ERROR = "An error has occurred.";
    public static readonly string NAME_MUST_HAVE_ONLY_LETTERS = "Username must only have letters.";
    public static readonly string USER_DO_NOT_EXIST = "User does not exist.";
    public static readonly string GOOGLE_AUTH_ERROR = "An error has ocurred during Google Authorization";
    public static readonly string ALERT_DO_NOT_EXIST = "Alert does not exist.";
    public static readonly string UPDATE_DATA_IS_NULL = "The update data is null";

    public static string ValueIsNullOrEmpty(string valueName) =>
        $"{valueName} not be null or empty.";

    public static string ValueBellowMinimumLength(string valueName, int minimumLength = 3) =>
        $"{valueName} length should be at least {minimumLength} characters.";

    public static string ValueAboveMaximumLength(string valueName, int maximumLenght = 64) =>
        $"{valueName} length should be at most {maximumLenght} characters.";

    public static string ValueIsInvalid(string valueName) =>
        $"{valueName} is invalid.";
}