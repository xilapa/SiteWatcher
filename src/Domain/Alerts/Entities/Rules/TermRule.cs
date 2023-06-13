using System.Text.RegularExpressions;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Domain.Alerts.Entities.Rules;

public class TermRule : Rule
{
    // ctor for EF
    protected TermRule() : base()
    {
        _occurrences = new List<TermOccurrence>();
    }

    public TermRule(string term, DateTime currentDate) : base(currentDate)
    {
        Term = term;
        _occurrences = new List<TermOccurrence>();
    }

    public void Update(UpdateAlertInput updateAlertInput)
    {
        Term = updateAlertInput.Term!.NewValue!;
        FirstWatchDone = false;
    }

    public override async Task<bool> Execute(Stream html)
    {
        var htmlExtractedText = await HtmlUtils.ExtractText(html);
        await html.DisposeAsync();

        // TODO: save the term occurrence count and compare
        var matches = Regex.Matches(htmlExtractedText, Term, RegexOptions.IgnoreCase);

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

    public string Term { get; private set; } = null!;
    private readonly List<TermOccurrence> _occurrences;
    public IReadOnlyCollection<TermOccurrence> Occurrences => _occurrences.ToArray();
}

public record TermOccurrence(string Context)
{
    public string Context { get; set; } = Context;
}