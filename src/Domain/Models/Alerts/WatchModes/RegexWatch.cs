using System.Text.RegularExpressions;
using Domain.DTOs.Alerts;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Domain.Models.Alerts.WatchModes;

public class RegexWatch : WatchMode
{
    protected RegexWatch()
    {
        RegexPattern = string.Empty;
        _matches = new List<string>();
    }

    public RegexWatch(string regexPattern, bool notifyOnDisappearance, DateTime currentDate) :base(currentDate)
    {
        RegexPattern = regexPattern;
        NotifyOnDisappearance = notifyOnDisappearance;
        _matches = new List<string>();
    }

    public bool NotifyOnDisappearance { get; private set; }
    public string RegexPattern { get; private set; }
    private readonly List<string> _matches;
    public IReadOnlyCollection<string> Matches => _matches.ToArray();

    public void Update(UpdateAlertInput updateAlertInput)
    {
        if(updateAlertInput.NotifyOnDisappearance == null && updateAlertInput.RegexPattern == null)
            return;

        if(updateAlertInput.NotifyOnDisappearance != null)
            NotifyOnDisappearance = updateAlertInput.NotifyOnDisappearance.NewValue;

        if(updateAlertInput.RegexPattern?.NewValue != null)
            RegexPattern = updateAlertInput.RegexPattern.NewValue;
    }

    public override async Task<bool> VerifySite(Stream html)
    {
        var htmlExtractedText = await HtmlUtils.ExtractText(html);

        var newMatches = Regex.Matches(htmlExtractedText, RegexPattern, RegexOptions.IgnoreCase);

        // Save the ocurrences for future comparison
        if (!FirstWatchDone)
        {
            _matches.AddRange(newMatches.Select(m => m.Value.Trim()));
            FirstWatchDone = true;
            return false;
        }

        var newMatchesArray = newMatches.Select(_ => _.Value.Trim()).ToArray();

        // If it has differences, save it
        bool SaveMatchesAndReturn(bool different)
        {
            if (!different) return false;
            _matches.Clear();
            _matches.AddRange(newMatchesArray);
            return true;
        }

        // Verify if it has new occurrences
        var hasNewOccurrences = false;
        foreach (var newMatch in newMatchesArray)
        {
            hasNewOccurrences = !_matches.Contains(newMatch);
            if (hasNewOccurrences) break;
        }

        // If it is not to notify on matche disappearance, save the new matches
        if (!NotifyOnDisappearance)
            return SaveMatchesAndReturn(hasNewOccurrences);

        var differentLength = newMatchesArray.Length != _matches.Count;
        return SaveMatchesAndReturn(differentLength);
    }
}