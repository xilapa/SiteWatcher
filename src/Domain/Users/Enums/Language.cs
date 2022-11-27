using System.ComponentModel;

namespace SiteWatcher.Domain.Users.Enums;

public enum Language
{
    [Description("pt")]
    BrazilianPortuguese = 1,

    [Description("en")]
    English = 2,

    [Description("es")]
    Spanish = 3
}