using Domain.DTOs.Alert;

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

    public string Term { get; private set; }
    private readonly IEnumerable<TermOccurrence> _occurrences;
    public IReadOnlyCollection<TermOccurrence> Occurrences => _occurrences.ToArray();
}

public record TermOccurrence(string Context)
{
    public string Context { get; set; } = Context;
}