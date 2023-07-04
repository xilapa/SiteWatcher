using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.Messages;

namespace SiteWatcher.Application.Emails.Messages;

public class
    CreateEmailOnUserReactivationTokenGeneratedMessageHandler : BaseMessageHandler<UserReactivationTokenGeneratedMessage>
{
    private readonly IAppSettings _appSettings;
    private readonly IAuthService _authService;

    public CreateEmailOnUserReactivationTokenGeneratedMessageHandler(ISiteWatcherContext context,
        ILogger<UserReactivationTokenGeneratedMessage> logger, ISession session, IAppSettings appSettings,
        IAuthService authService) : base(context, logger, session)
    {
        _appSettings = appSettings;
        _authService = authService;
    }

    protected override async Task Consume(UserReactivationTokenGeneratedMessage message, CancellationToken ct)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/reactivate-account?t={message.ConfirmationToken}";
        var email = EmailFactory.AccountActivation(message, link, Session.Now);
        Context.Emails.Add(email);

        await _authService.SetAccountActivationTokenExpiration(message.ConfirmationToken, message.UserId);
        await Context.SaveChangesAsync(ct);
    }
}