using System.Security.Cryptography;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Domain.Alerts.Entities.Rules;

public sealed class AnyChangesRule : Rule
{
    // ctor for EF
    private AnyChangesRule()
    {
        HtmlHash = string.Empty;
        RuleType = RuleType.AnyChanges;
    }

    public AnyChangesRule(DateTime currentDate) : base(currentDate)
    {
        HtmlHash = string.Empty;
        RuleType = RuleType.AnyChanges;
    }

    public string HtmlHash { get; private set; }

    public override async Task<bool> Execute(Stream html)
    {
        using var shaHasher = SHA256.Create();
        var hashedBytes = await shaHasher.ComputeHashAsync(html);

        // Create an hexadecimal string
        var stringBuilder = StringBuilderCache.Acquire(64);
        foreach (var hashedByte in hashedBytes)
            stringBuilder.Append(hashedByte.ToString("x2"));

        var newHash = StringBuilderCache.GetStringAndRelease(stringBuilder);

        if (!FirstWatchDone)
        {
            FirstWatchDone = true;
            HtmlHash = newHash;
            return false;
        }

        var hasChanged = !newHash.Equals(HtmlHash);
        HtmlHash = newHash;
        return hasChanged;
    }
}