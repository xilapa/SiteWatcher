using System.Text.RegularExpressions;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Domain.Alerts.Entities.WatchModes;

public class RegexWatch : WatchMode
{
    protected RegexWatch()
    {
        RegexPattern = string.Empty;
        _matches = new Matches();
    }

    public RegexWatch(string regexPattern, bool notifyOnDisappearance, DateTime currentDate) : base(currentDate)
    {
        RegexPattern = regexPattern;
        NotifyOnDisappearance = notifyOnDisappearance;
        _matches = new Matches();
    }

    public bool NotifyOnDisappearance { get; private set; }
    public string RegexPattern { get; private set; }
    private Matches _matches;
    public IReadOnlyDictionary<string, int> Matches => _matches;

    public void Update(UpdateAlertInput updateAlertInput)
    {
        if (updateAlertInput.NotifyOnDisappearance == null && updateAlertInput.RegexPattern == null)
            return;

        if (updateAlertInput.NotifyOnDisappearance?.NewValue  != null)
            NotifyOnDisappearance = updateAlertInput.NotifyOnDisappearance.NewValue;

        if (updateAlertInput.RegexPattern?.NewValue != null)
            RegexPattern = updateAlertInput.RegexPattern.NewValue;
    }

    public override async Task<bool> VerifySite(Stream html)
    {
        var htmlExtractedText = await HtmlUtils.ExtractText(html);
        await html.DisposeAsync();

        var regexMatches = Regex
            .Matches(htmlExtractedText, RegexPattern, RegexOptions.IgnoreCase)
            .Select(m => m.Value.Trim());

        var newMatches = new Matches().Fill(regexMatches);

        // Save the ocurrences for future comparison
        if (!FirstWatchDone)
        {
            // It's needed to change the reference for EF see the change and persist
            _matches = newMatches;
            FirstWatchDone = true;
            return false;
        }

        var notifyUser = false;
        var anyCountDecreased = false;
        foreach (var newMatch in newMatches)
        {
            var hasKey = _matches.ContainsKey(newMatch.Key);
            // if the match is really new, notify user
            if (!hasKey)
            {
                notifyUser = true;
                break;
            }

            // If match count increased, notify user
            var countIncreased = newMatch.Value > _matches[newMatch.Key];
            if (countIncreased)
            {
                notifyUser = true;
                break;
            }

            // Save if the match count decreased,
            // it'll be used if has NotifyOnDisappearance
            if(!anyCountDecreased)
                anyCountDecreased = newMatch.Value < _matches[newMatch.Key];
        }

        // If notifyUser still false and user has NotifyOnDisappearance
        // check if any match count has decreased
        if (!notifyUser && NotifyOnDisappearance)
        {
            notifyUser = anyCountDecreased;
            // If still false, check if the count of matches has decreased
            if(!notifyUser)
                notifyUser = newMatches.Count < _matches.Count;
        }

        // Save the new matches
        _matches = newMatches;

        return notifyUser;
    }
}

/// <summary>
/// Hold the match value with it count.
/// </summary>
public sealed class Matches : Dictionary<string, int>
{
    public Matches Fill(IEnumerable<string> src)
    {
        foreach (var m in src.ToArray())
        {
            if (ContainsKey(m))
            {
                this[m]++;
                continue;
            }

            Add(m, 1);
        }
        return this;
    }
}