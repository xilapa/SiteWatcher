using System.Text.RegularExpressions;
using Domain.DTOs.Alerts;
using Domain.Events.Alerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Domain.Models.Alerts;

public class Alert : BaseModel<AlertId>
{
    // ctor for EF
    protected Alert() : base()
    { }

    public Alert(UserId userId, string name, EFrequency frequency, DateTime currentDate, Site site, WatchMode watchMode)
        : base(new AlertId(), currentDate)
    {
        UserId = userId;
        Name = name;
        Frequency = frequency;
        Site = site;
        WatchMode = watchMode;
        GenerateSearchField();
        AddDomainEvent(new AlertsChangedEvent(UserId));
    }

    public UserId UserId { get; private set; }
    public User User { get; set; }
    public string Name { get; private set; }
    public EFrequency Frequency { get; private set; }
    public DateTime? LastVerification { get; private set; }
    public Site Site { get; private set; }
    public WatchMode WatchMode { get; private set; }

    private List<Notification>? _notifications;
    public IReadOnlyCollection<Notification> Notifications => _notifications ?? new List<Notification>();

    public string SearchField { get; private set; }

    public static Alert GetModelForUpdate(UpdateAlertDto updateAlertDto) =>
        new()
        {
            Id = updateAlertDto.Id,
            UserId = updateAlertDto.UserId,
            CreatedAt = updateAlertDto.CreatedAt,
            Name = updateAlertDto.Name,
            Frequency = updateAlertDto.Frequency,
            Site = new Site(updateAlertDto.SiteUri, updateAlertDto.Name)
        };

    public void Update(UpdateAlertInput updateInput, DateTime updateDate)
    {
        var regenerateSearchField = false;

        if (updateInput.Name is not null)
        {
            Name = updateInput.Name.NewValue!;
            regenerateSearchField = true;
        }

        if (updateInput.Frequency is not null)
            Frequency = updateInput.Frequency.NewValue;

        if (updateInput.SiteName is not null || updateInput.SiteUri is not null)
        {
            UpdateSite(updateInput);
            regenerateSearchField = true;
        }

        if (updateInput.WatchMode is not null || updateInput.Term is not null)
            UpdateWatchMode(updateInput, updateDate);

        if (regenerateSearchField) GenerateSearchField();

        LastUpdatedAt = updateDate;

        AddDomainEvent(new AlertsChangedEvent(UserId));
    }

    private void UpdateSite(UpdateAlertInput updateInput)
    {
        var newSiteName = Site.Name;
        var newSiteUri = Site.Uri.AbsoluteUri;

        if (updateInput.SiteName is not null)
            newSiteName = updateInput.SiteName.NewValue;

        if (updateInput.SiteUri is not null)
            newSiteUri = updateInput.SiteUri.NewValue;

        Site = new Site(newSiteUri!, newSiteName!);
    }

    private void UpdateWatchMode(UpdateAlertInput updateInput, DateTime updateDate)
    {
        var currentWatchMode = Utils.Utils.GetWatchModeEnumByType(WatchMode);

        // If the current watch mode is really new, then recreate it
        if (updateInput.WatchMode is not null && !currentWatchMode!.Value.Equals(updateInput.WatchMode.NewValue))
        {
            // Create the event before the WatchMode's Id is set to zero
            AddDomainEvent(new AlertWatchModeChangedEvent(WatchMode.Id));
            WatchMode = AlertFactory.CreateWatchMode(updateInput, updateDate);
            return;
        }

        // Update term watchMode
        if (currentWatchMode.Equals(EWatchMode.Term) && updateInput.Term is not null)
            (WatchMode as TermWatch)!.Update(updateInput);

        // any changes doesn't have field to update, so just ignore it
    }

    private void GenerateSearchField()
    {
        // Separating name parts ignoring non-alphanumerics characters, diacritics and case
        var nameParts = Regex.Split(Name, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 2)
            .Select(p => p.ToLowerCaseWithoutDiacritics())
            .ToArray();

        // Separating site name parts ignoring white spaces, diacritics and case
        var siteNameParts = Regex.Split(Site.Name, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 2)
            .Select(p => p.ToLowerCaseWithoutDiacritics())
            .ToArray();

        // Separating site uri parts ignoring non-alphanumerics characters, diacritics, case, "http://" and "https://"
        var doubleBarIndex = Site.Uri.AbsoluteUri.IndexOf("//", StringComparison.Ordinal) + 2;
        var siteUri = Site.Uri.AbsoluteUri[doubleBarIndex..].ToLowerCaseWithoutDiacritics();

        var siteUriParts = Regex.Split(siteUri, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 2)
            .Select(p => p.ToLowerCaseWithoutDiacritics())
            .ToArray();

        // Ignoring duplicated terms
        var searchParts = nameParts.Union(siteNameParts).Union(siteUriParts).Distinct().ToArray();
        var stringBuilder = StringBuilderCache.Acquire(200);

        // building the search field
        for (var i = 0; i < searchParts.Length; i++)
        {
            stringBuilder.Append(searchParts[i]);
            if (i != searchParts.Length - 1)
                stringBuilder.Append('|');
        }

        SearchField = StringBuilderCache.GetStringAndRelease(stringBuilder);
    }

    public async Task<AlertToNotify?> VerifySiteHtml(Stream html, DateTime currentTime)
    {
        var notifyUser = await WatchMode.VerifySite(html);
        LastVerification = currentTime;

        if (notifyUser)
        {
            _notifications ??= new List<Notification>();
            _notifications.Add(new Notification(currentTime));
        }

        return notifyUser ? new AlertToNotify(this, User.Language) : null;
    }
}