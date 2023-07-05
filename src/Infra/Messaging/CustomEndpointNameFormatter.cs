using System.Text.RegularExpressions;
using MassTransit;

namespace SiteWatcher.Infra.Messaging;

public partial class CustomEndpointNameFormatter : KebabCaseEndpointNameFormatter
{
    public CustomEndpointNameFormatter() : base(includeNamespace: false) { }

    [GeneratedRegex("(-message-dispatcher)|(-message)", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    private static readonly Regex _regex = MyRegex();

    public override string SanitizeName(string name) =>
        "site-watcher-" + _regex.Replace(base.SanitizeName(name), "");
}