using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System;

namespace SiteWatcher.Domain.Extensions;

public static class StringExtensions
{
    public static byte[] GetHashedBytes(this string text)
    {
        var textBytes = Encoding.ASCII.GetBytes(text);
        var hashedBytes = SHA256.HashData(textBytes);
        return hashedBytes;
    }

    /// <summary>
    /// Return the <typeparamref name="EnumT"/> equivalent value of the current string
    /// based on <typeparamref name="EnumT"/>'s Description Attributes. 
    /// </summary>
    /// <typeparam name="EnumT"></typeparam>
    /// <param name="stringValue"></param>
    /// <returns></returns>
    public static EnumT GetEnumValue<EnumT>(this string stringValue) where EnumT : Enum
    {
        var enumInstance = Activator.CreateInstance<EnumT>();
        var enumOptions = typeof(EnumT)
                            .GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Select(m => new {
                                m.Name,
                                Value = m.GetValue(enumInstance),
                                DescAtt = m.GetCustomAttributes(typeof(DescriptionAttribute),false).FirstOrDefault()
                            });

        var option = enumOptions.FirstOrDefault(opts => (opts.DescAtt as DescriptionAttribute).Description == stringValue.ToLower());

        if (option is null)
            return default;

        return (EnumT) option.Value;
    }
}