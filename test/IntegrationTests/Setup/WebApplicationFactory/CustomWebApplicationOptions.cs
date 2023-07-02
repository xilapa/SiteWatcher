using System.Collections.ObjectModel;
using SiteWatcher.Infra.Persistence;

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

public class CustomWebApplicationOptionsBuilder
{
    private readonly CustomWebApplicationOptions _options;

    public CustomWebApplicationOptionsBuilder()
    {
        _options = new CustomWebApplicationOptions();
    }

    public CustomWebApplicationOptionsBuilder SetInitialDate(DateTime date)
    {
        _options.InitalDate = date;
        return this;
    }

    public CustomWebApplicationOptionsBuilder UseDatabase(DatabaseType databaseType)
    {
        _options.DatabaseType = databaseType;
        return this;
    }

    public CustomWebApplicationOptionsBuilder ReplaceService(Type serviceType, object service)
    {
        _options.ReplaceService(serviceType, service);
        return this;
    }

    public CustomWebApplicationOptions Build() => _options;
}