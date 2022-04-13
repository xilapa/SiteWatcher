using System.ComponentModel;

namespace SiteWatcher.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var enumMember = enumValue.GetType().GetMember(enumValue.ToString());
        var enumAttribute = enumMember[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

        var enumDescription = enumAttribute[0] is null ?
                                string.Empty :
                                (enumAttribute[0] as DescriptionAttribute).Description;

        return enumDescription;
    }
}