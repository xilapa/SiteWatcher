using System.Text.RegularExpressions;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Entities.Notifications;
using SiteWatcher.Domain.Alerts.Entities.WatchModes;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.Extensions;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts;

public class Alert : BaseModel<AlertId>
{
    // ctor for EF
    protected Alert() : base()
    { }

    public Alert(UserId userId, string name, Frequencies frequency, DateTime currentDate, Site site, WatchMode watchMode)
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
    public Frequencies Frequency { get; private set; }
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

        // TODO: remove these verifications after tests
        if (updateInput.WatchMode is not null || updateInput.Term is not null
            || updateInput.RegexPattern is not null || updateInput.NotifyOnDisappearance is not null)
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
        var currentWatchMode = GetWatchModeEnumByType(WatchMode);

        // If the current watch mode is really new, then recreate it
        if (updateInput.WatchMode is not null && !currentWatchMode!.Value.Equals(updateInput.WatchMode.NewValue))
        {
            // Create the event before the WatchMode's Id is set to zero
            AddDomainEvent(new AlertWatchModeChangedEvent(WatchMode.Id));
            WatchMode = AlertFactory.CreateWatchMode(updateInput, updateDate);
            return;
        }

        // Update term watchMode
        // TODO: move this null check to the TermWatch update method
        if (currentWatchMode.Equals(WatchModes.Term) && updateInput.Term is not null)
            (WatchMode as TermWatch)!.Update(updateInput);

        // Update regex watchMode
        if (currentWatchMode.Equals(WatchModes.Regex))
            (WatchMode as RegexWatch)!.Update(updateInput);

        // any changes doesn't have field to update, so just ignore it
    }

    private void GenerateSearchField()
    {
        // Separating name parts ignoring non-alphanumerics characters, diacritics and case
        var nameParts = Regex.Split(Name, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 1)
            .Select(p => p.ToLowerCaseWithoutDiacritics())
            .ToArray();

        // Separating site name parts ignoring white spaces, diacritics and case
        var siteNameParts = Regex.Split(Site.Name, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 1)
            .Select(p => p.ToLowerCaseWithoutDiacritics())
            .ToArray();

        // Separating site uri parts ignoring non-alphanumerics characters, diacritics, case, "http://" and "https://"
        var doubleBarIndex = Site.Uri.AbsoluteUri.IndexOf("//", StringComparison.Ordinal) + 2;
        var siteUri = Site.Uri.AbsoluteUri[doubleBarIndex..].ToLowerCaseWithoutDiacritics();

        var siteUriParts = Regex.Split(siteUri, "\\W")
            .Where(p => !string.IsNullOrWhiteSpace(p) && !string.IsNullOrEmpty(p) && p.Length > 1)
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

        AlertToNotify? alertToNotify = null;

        if (notifyUser)
            alertToNotify = GenerateAlertToNotify(currentTime);

        return alertToNotify;
    }

    public AlertToNotify GenerateAlertToNotify(DateTime currentTime)
    {
        var notification = new Notification(currentTime);
        _notifications ??= new List<Notification>();
        _notifications.Add(notification);
        return new AlertToNotify(this, notification.Id, User.Language);
    }

    /// <summary>
    /// Correlate the email with the notification.
    /// </summary>
    /// <param name="email">The email to be set on the notifications</param>
    /// <param name="notificationIds">The list of notifications Ids</param>
    /// <returns>True if the email was set on any notification, False if there is no notification with the Ids passed</returns>
    public bool SetEmail(Email email, IEnumerable<NotificationId> notificationIds)
    {
        var notification = _notifications?.SingleOrDefault(n => notificationIds.Contains(n.Id));
        if (notification == null) return false;
        notification.SetEmail(email);
        return true;
    }
}