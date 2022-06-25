using Domain.DTOs.Alert;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public class Alert : BaseModel<AlertId>
{
    // ctor for EF
    protected Alert() : base(new AlertId())
    { }

    public Alert(UserId userId, string name, EFrequency frequency, DateTime currentDate, Site site)
        : this()
    {
        UserId = userId;
        Name = name;
        Frequency = frequency;
        Site = site;
        CreatedAt = currentDate;
        LastUpdatedAt = currentDate;
    }

    public UserId UserId { get; }
    public string Name { get; private set; }
    public EFrequency Frequency { get; private set; }
    public DateTime? LastVerification { get; private set; }
    public Site Site { get; private set; }

    public static Alert FromInputModel(CreateAlertInput inputModel, UserId userId, DateTime currentDate)
    {
        var site = new Site(inputModel.SiteUri, inputModel.SiteName);
        var alert = new Alert(userId, inputModel.Name, inputModel.Frequency, currentDate, site);
        return alert;
    }
}