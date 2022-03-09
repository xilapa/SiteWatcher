using System.Data.Common;

namespace SiteWatcher.Domain.Exceptions;

public class UniqueViolationException : DbException
{
    public UniqueViolationException(string modelName) : base($"The {modelName} already exists")
    { }
}