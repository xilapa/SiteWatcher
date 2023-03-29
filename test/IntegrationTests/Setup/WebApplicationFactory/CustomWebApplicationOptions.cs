using System.Collections.ObjectModel;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public class CustomWebApplicationOptions
{
    public CustomWebApplicationOptions()
    {
        InitalDate = null;
        _servicesToReplace = new Dictionary<Type, object>();
        DatabaseType = DatabaseType.SqliteInMemory;
    }

    /// <summary>
    /// Date used on database seed. Must be in UTC.
    /// </summary>
    public DateTime? InitalDate { get; set; }

    private readonly Dictionary<Type, object> _servicesToReplace;

    /// <summary>
    /// The type of service to replace with the replacement implementation
    /// </summary>
    public void ReplaceService(Type serviceType, object service) =>
        _servicesToReplace.TryAdd(serviceType, service);

    public ReadOnlyDictionary<Type, object> ReplacementServices =>
        new (_servicesToReplace);

    /// <summary>
    /// Database provider type. Defaults to Sqlite in memory.
    /// </summary>
    public DatabaseType DatabaseType { get; set; }
}

public enum DatabaseType
{
    SqliteInMemory,
    /// <summary>
    /// Use Sqlite on disk when the test calls a dapper repository.
    /// Dapper repositories closes the underlying connection after each call,
    /// thus deleting "Sqlite in memory".
    /// </summary>
    SqliteOnDisk,
    PostgresOnDocker
}