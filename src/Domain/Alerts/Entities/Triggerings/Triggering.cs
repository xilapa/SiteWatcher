namespace SiteWatcher.Domain.Alerts.Entities.Triggerings;

public class Triggering
{
    public Triggering(DateTime date)
    {
        Date = date;
    }
    public DateTime Date { get; set; }
}