using System.Security.Cryptography;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Domain.Models.Alerts.WatchModes;

public class AnyChangesWatch : WatchMode
{
    // ctor for EF
    protected AnyChangesWatch() : base()
    {
        HtmlHash = string.Empty;
    }

    public AnyChangesWatch(DateTime currentDate) : base(currentDate)
    {
        HtmlHash = string.Empty;
    }

    public string HtmlHash { get; private set; }

    public override async Task<bool> VerifySite(Stream html)
    {
        var shaHasher = SHA256.Create();
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