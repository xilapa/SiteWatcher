using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails.Messages;

namespace SiteWatcher.Application.Emails.Messages;

public sealed class SendEmailOnEmailCreatedMessageHandler : BaseMessageHandler<EmailCreatedMessage>
{
    private readonly IEmailService _emailService;

    public SendEmailOnEmailCreatedMessageHandler(ISiteWatcherContext context, ILogger<EmailCreatedMessage> logger,
        ISession session, IEmailService emailService) : base(context, logger, session)
    {
        _emailService = emailService;
    }

    protected override Task Handle(ConsumeContext<EmailCreatedMessage> context)
    {
        return _emailService
            .SendEmailAsync(context.Message.Subject,
                context.Message.Body!,
                context.Message.Recipients,
                context.CancellationToken);
    }
}