using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails.Events;

namespace SiteWatcher.Application.Emails.Messages;

public class SendEmailOnEmailCreatedMessageHandler : BaseMessageHandler<EmailCreatedMessage>
{
    private readonly IEmailServiceSingleton _emailService;
    private readonly IEmailSettings _settings;

    public SendEmailOnEmailCreatedMessageHandler(ISiteWatcherContext context, ILogger<EmailCreatedMessage> logger,
        ISession session, IEmailServiceSingleton emailService, IEmailSettings settings) : base(context, logger, session)
    {
        _emailService = emailService;
        _settings = settings;
    }

    protected override async Task Consume(EmailCreatedMessage message, CancellationToken ct)
    {
        var error = await _emailService.SendEmailAsync(message.Subject, message.Body!, message.Recipients, ct);
        await Task.Delay(TimeSpan.FromSeconds(_settings.EmailDelaySeconds), ct);
        if (error != null) throw new Exception(error);
        await Context.SaveChangesAsync(ct);
    }
}