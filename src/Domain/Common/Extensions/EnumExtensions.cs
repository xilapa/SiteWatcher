using System.ComponentModel;

namespace SiteWatcher.Domain.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var enumMember = enumValue.GetType().GetMember(enumValue.ToString());
        var enumAttribute = enumMember[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

        return enumAttribute[0] is null ?
                                string.Empty :
                                (enumAttribute[0] as DescriptionAttribute)!.Description;
    }
}