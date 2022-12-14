﻿using System.Text.RegularExpressions;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Entities.Notifications;
using SiteWatcher.Domain.Alerts.Entities.Rules;
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
    protected Alert()
    { }

    public Alert(UserId userId, string name, Frequencies frequency, DateTime currentDate, Site site, Rule rule)
        : base(new AlertId(), currentDate)
    {
        UserId = userId;
        Name = name;
        Frequency = frequency;
        Site = site;
        Rule = rule;
        GenerateSearchField();
        AddDomainEvent(new AlertsChangedEvent(UserId));
    }

    public UserId UserId { get; private set; }
    public User User { get; set; } = null!;
    public string Name { get; private set; } = null!;
    public Frequencies Frequency { get; private set; }
    public DateTime? LastVerification { get; private set; }
    public Site Site { get; private set; } = null!;
    public Rule Rule { get; private set; } = null!;

    private List<Notification>? _notifications;
    public IReadOnlyCollection<Notification> Notifications => _notifications ?? new List<Notification>();
    public ICollection<Email> Emails { get; set; }

    public string SearchField { get; private set; } = null!;

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
        if (updateInput.Rule is not null || updateInput.Term is not null
            || updateInput.RegexPattern is not null || updateInput.NotifyOnDisappearance is not null)
            UpdateRule(updateInput, updateDate);

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

    private void UpdateRule(UpdateAlertInput updateInput, DateTime updateDate)
    {
        var currentRule = GetRuleEnumByType(Rule);

        // If the current rule is really new, then recreate it
        if (updateInput.Rule is not null && !currentRule!.Value.Equals(updateInput.Rule.NewValue))
        {
            // Create the event before the rule's Id is set to zero
            AddDomainEvent(new AlertRuleChangedEvent(Rule.Id));
            Rule = AlertFactory.CreateRule(updateInput, updateDate);
            return;
        }

        // Update term rule
        // TODO: move this null check to the TermWatch update method
        if (currentRule.Equals(Rules.Term) && updateInput.Term is not null)
            (Rule as TermRule)!.Update(updateInput);

        // Update regex rule
        if (currentRule.Equals(Rules.Regex))
            (Rule as RegexRule)!.Update(updateInput);

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

    public async Task<AlertToNotify?> ExecuteRule(Stream? html, DateTime currentTime)
    {
        // when stream is null the site can't be fetched
        if(html == Stream.Null || html == null)
            return GenerateAlertToNotify(NotificationType.Error, currentTime);

        var notifyUser = await Rule.Execute(html);
        LastVerification = currentTime;

        AlertToNotify? alertToNotify = null;

        if (notifyUser)
            alertToNotify = GenerateAlertToNotify(NotificationType.Sucess, currentTime);

        return alertToNotify;
    }

    public AlertToNotify GenerateAlertToNotify(NotificationType type, DateTime currentTime)
    {
        var notification = new Notification(currentTime);
        _notifications ??= new List<Notification>();
        _notifications.Add(notification);
        return new AlertToNotify(this, notification.Id, type, User.Language);
    }

    /// <summary>
    /// Correlate the email with the notification.
    /// </summary>
    /// <param name="email">The email to be set on the notifications</param>
    /// <param name="notificationIds">The list of notifications Ids</param>
    public void SetEmail(Email email, IEnumerable<NotificationId> notificationIds)
    {
        var notification = _notifications?.SingleOrDefault(n => notificationIds.Contains(n.Id));
        if (notification == null) return;
        notification.SetEmail(email);
    }
}