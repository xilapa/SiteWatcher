using StronglyTypedIds;

[assembly:StronglyTypedIdDefaults(
    converters: StronglyTypedIdConverter.Default |
                StronglyTypedIdConverter.TypeConverter |
                StronglyTypedIdConverter.SystemTextJson |
                StronglyTypedIdConverter.EfCoreValueConverter,
    implementations: StronglyTypedIdImplementations.Default |
                     StronglyTypedIdImplementations.IComparable |
                     StronglyTypedIdImplementations.IEquatable)]
namespace SiteWatcher.Domain.Models.Common;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.Guid)]
public partial struct UserId {}