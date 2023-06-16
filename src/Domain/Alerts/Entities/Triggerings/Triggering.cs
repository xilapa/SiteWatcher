namespace SiteWatcher.Domain.Alerts.Entities.Triggerings;

public class Triggering
{
    public Triggering(DateTime date, TriggeringStatus status)
    {
        Date = date;
        Status = status;
    }

    public DateTime Date { get; set; }
    public TriggeringStatus Status { get; set; }
}