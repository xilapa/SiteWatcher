using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users.Messages;

namespace SiteWatcher.Application.Emails.Messages;

public class
    CreateEmailOnUserReactivationTokenGeneratedMessageHandler : BaseMessageHandler<UserReactivationTokenGeneratedMessage>
{
    private readonly IAppSettings _appSettings;
    private readonly IAuthService _authService;
    private readonly IPublisher _publisher;

    public CreateEmailOnUserReactivationTokenGeneratedMessageHandler(ISiteWatcherContext context,
        ILogger<UserReactivationTokenGeneratedMessage> logger, ISession session, IAppSettings appSettings,
        IAuthService authService, IPublisher publisher) : base(context, logger, session)
    {
        _appSettings = appSettings;
        _authService = authService;
        _publisher = publisher;
    }

    protected override async Task Handle(ConsumeContext<UserReactivationTokenGeneratedMessage> context)
    {
        var link = $"{_appSettings.FrontEndUrl}/#/security/reactivate-account?t={context.Message.ConfirmationToken}";
        var (email, emailCreatedMessage) = EmailFactory.AccountActivation(context.Message, link, Session.Now);
        Context.Emails.Add(email);

        await _authService.SetAccountActivationTokenExpiration(context.Message.ConfirmationToken, context.Message.UserId);
        await _publisher.PublishAsync(emailCreatedMessage, context.CancellationToken);

        await Context.SaveChangesAsync(CancellationToken.None);
    }
}