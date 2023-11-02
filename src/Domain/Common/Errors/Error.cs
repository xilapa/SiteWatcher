namespace SiteWatcher.Domain.Common.Errors;

public sealed class Error
{
    public Error(ErrorType type, params string[] messages)
    {
        Type = type;
        Messages = messages;
    }

    public ErrorType Type { get; }
    public string[] Messages { get; }

    public static Error Validation(params string[] messages) => new Error(ErrorType.Validation, messages);
}