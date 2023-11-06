using MassTransit;
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

    protected override async Task Handle(ConsumeContext<EmailConfirmationTokenGeneratedMessage> context)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/confirm-email?t={context.Message.ConfirmationToken}";
        var email = EmailFactory.EmailConfirmation(context.Message, link, Session.Now);
        Context.Emails.Add(email);

        await _authService.SetEmailConfirmationTokenExpiration(context.Message.ConfirmationToken, context.Message.UserId);
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}