using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.Messages;

namespace SiteWatcher.Application.Emails.Messages;

public class CreateEmailOnEmailConfirmationTokenGeneratedMessageHandler : BaseMessageHandler<EmailConfirmationTokenGeneratedMessage>
{
    private readonly IAppSettings _appSettings;
    private readonly IAuthService _authService;

    public CreateEmailOnEmailConfirmationTokenGeneratedMessageHandler(ISiteWatcherContext context,
        ILogger<EmailConfirmationTokenGeneratedMessage> logger, ISession session, IAppSettings appSettings,
        IAuthService authService) : base(context, logger, session)
    {
        _appSettings = appSettings;
        _authService = authService;
    }

    protected override async Task Consume(EmailConfirmationTokenGeneratedMessage message, CancellationToken ct)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/confirm-email?t={message.ConfirmationToken}";
        var email = EmailFactory.EmailConfirmation(message, link, Session.Now);
        Context.Emails.Add(email);

        await _authService.SetEmailConfirmationTokenExpiration(message.ConfirmationToken, message.UserId);
        await Context.SaveChangesAsync(ct);
    }
}