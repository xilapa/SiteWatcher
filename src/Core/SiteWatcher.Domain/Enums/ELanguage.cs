using System.ComponentModel;

namespace SiteWatcher.Domain.Enums;

public enum ELanguage
{
    [Description("pt")]
    BrasilianPortuguese = 1,

    [Description("en")]
    English = 2,

    [Description("es")]
    Spanish = 3
}