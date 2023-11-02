using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails.Messages;

namespace SiteWatcher.Application.Emails.Messages;

public class SendEmailOnEmailCreatedMessageHandler : BaseMessageHandler<EmailCreatedMessage>
{
    private readonly IEmailServiceSingleton _emailService;

    public SendEmailOnEmailCreatedMessageHandler(ISiteWatcherContext context, ILogger<EmailCreatedMessage> logger,
        ISession session, IEmailServiceSingleton emailService) : base(context, logger, session)
    {
        _emailService = emailService;
    }

    protected override async Task Handle(ConsumeContext<EmailCreatedMessage> context)
    {
        var error = await _emailService
            .SendEmailAsync(context.Message.Subject,
                context.Message.Body!,
                context.Message.Recipients,
                context.CancellationToken);

        if (error != null) throw new Exception(error);

        await Context.SaveChangesAsync(CancellationToken.None);
    }
}