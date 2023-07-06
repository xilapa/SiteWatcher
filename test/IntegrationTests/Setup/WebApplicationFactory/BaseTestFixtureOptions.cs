using System.Collections.ObjectModel;
using SiteWatcher.Infra.Persistence;

namespace SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

public class BaseTestFixtureOptions
{
    public BaseTestFixtureOptions()
    {
        InitialDate = null;
        _servicesToReplace = new Dictionary<Type, object>();
        DatabaseType = DatabaseType.SqliteInMemory;
    }

    /// <summary>
    /// Date used on database seed. Must be in UTC.
    /// </summary>
    public DateTime? InitialDate { get; set; }

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

public class BaseTestFixtureOptionsBuilder
{
    private readonly BaseTestFixtureOptions _options;

    public BaseTestFixtureOptionsBuilder()
    {
        _options = new BaseTestFixtureOptions();
    }

    public BaseTestFixtureOptionsBuilder SetInitialDate(DateTime date)
    {
        _options.InitialDate = date;
        return this;
    }

    public BaseTestFixtureOptionsBuilder UseDatabase(DatabaseType databaseType)
    {
        _options.DatabaseType = databaseType;
        return this;
    }

    public BaseTestFixtureOptionsBuilder ReplaceService(Type serviceType, object service)
    {
        _options.ReplaceService(serviceType, service);
        return this;
    }

    public BaseTestFixtureOptions Build() => _options;
}