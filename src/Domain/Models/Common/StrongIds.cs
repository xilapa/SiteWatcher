using StronglyTypedIds;

[assembly:StronglyTypedIdDefaults(
    converters: StronglyTypedIdConverter.Default |
                StronglyTypedIdConverter.TypeConverter |
                StronglyTypedIdConverter.SystemTextJson |
                StronglyTypedIdConverter.EfCoreValueConverter |
                StronglyTypedIdConverter.DapperTypeHandler,
    implementations: StronglyTypedIdImplementations.Default |
                     StronglyTypedIdImplementations.IComparable |
                     StronglyTypedIdImplementations.IEquatable)]
namespace SiteWatcher.Domain.Models.Common;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
public partial struct UserId {}

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
public partial struct AlertId {}

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
public partial struct WatchModeId {}

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
public partial struct NotificationId {}

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Int)]
public partial struct EmailId {}