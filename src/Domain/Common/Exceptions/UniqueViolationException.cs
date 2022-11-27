using System.Data.Common;

namespace SiteWatcher.Domain.Common.Exceptions;

public class UniqueViolationException : DbException
{
    public UniqueViolationException(string modelName) : base($"The {modelName} already exists")
    { }

    protected UniqueViolationException()
    { }

    protected UniqueViolationException(string? message, Exception? innerException) : base(message, innerException)
    { }

    protected UniqueViolationException(string? message, int errorCode) : base(message, errorCode)
    { }
}