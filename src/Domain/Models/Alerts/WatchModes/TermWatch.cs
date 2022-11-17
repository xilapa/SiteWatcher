using System.Text.RegularExpressions;
using Domain.DTOs.Alerts;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Domain.Models.Alerts.WatchModes;

public class TermWatch : WatchMode
{
    // ctor for EF
    protected TermWatch() : base()
    {
        _occurrences = new List<TermOccurrence>();
    }

    public TermWatch(string term, DateTime currentDate) : base(currentDate)
    {
        Term = term;
        _occurrences = new List<TermOccurrence>();
    }

    public void Update(UpdateAlertInput updateAlertInput)
    {
        Term = updateAlertInput.Term.NewValue!;
    }

    public override async Task<bool> VerifySite(Stream html)
    {
        var htmlExtractedText = await HtmlUtils.ExtractText(html);

        // The max length of Term is 64, the max length of Ocurrence.Context is 512.
        // So, there are left 448 characters to get, 224 for each side.
        var pattern = $"([^.]{{0,224}}{Term}[^.]{{0,224}}\\.?)";
        var matches = Regex.Matches(htmlExtractedText, pattern, RegexOptions.IgnoreCase);

        // Save the ocurrences for future comparison
        if (!FirstWatchDone)
        {
            _occurrences.AddRange(matches.Select(_ => new TermOccurrence(_.Value.Trim())));
            FirstWatchDone = true;
            return false;
        }

        var matchesArray = matches.Select(_ => _.Value.Trim()).ToArray();
        var ocurrencesArray = _occurrences.Select(_ => _.Context).ToArray();

        // If it has differences, save it
        bool SaveDifferenceAndReturn(bool different)
        {
            if(!different) return false;
            _occurrences.Clear();
            _occurrences.AddRange(matchesArray!.Select(v => new TermOccurrence(v)));
            return true;
        }

        // First compare the length of current matches and the saved occurrences
        var differentLength = matchesArray.Length != ocurrencesArray.Length;
        if(differentLength)
            return SaveDifferenceAndReturn(differentLength);

        // If the length are equals, compare if the matches are in the occurrences
        var differentOccurrence = false;
        foreach(var m in matchesArray)
        {
            differentOccurrence = !ocurrencesArray.Contains(m);
            if(differentOccurrence) break;
        }

        return SaveDifferenceAndReturn(differentOccurrence);
    }

    public string Term { get; private set; }
    private readonly List<TermOccurrence> _occurrences;
    public IReadOnlyCollection<TermOccurrence> Occurrences => _occurrences.ToArray();
}

public record TermOccurrence(string Context)
{
    public string Context { get; set; } = Context;
}