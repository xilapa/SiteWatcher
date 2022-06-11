using System.Collections.ObjectModel;

namespace IntegrationTests.Setup;

public class CustomWebApplicationOptions
{
    public CustomWebApplicationOptions()
    {
        InitalDate = null;
        _servicesToReplace = new Dictionary<Type, object>();
    }

    /// <summary>
    /// Date used on database seed
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
}