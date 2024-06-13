using SiteWatcher.Infra.Authorization;

namespace SiteWatcher.Infra.EmailSending;

public sealed class EmailThrottler
{
    private readonly Session _session;
    private DateTime? _lastSentDate;
    private readonly TimeSpan _delay;

    public EmailThrottler(EmailSettings emailSettings, Session session)
    {
        _session = session;
        _delay = TimeSpan.FromSeconds(emailSettings.EmailDelaySeconds);
    }

    public Task WaitToSend(CancellationToken cancellationToken)
    {
        if (_lastSentDate is null || _session.Now > _lastSentDate.Value.Add(_delay))
        {
            _lastSentDate = _session.Now;
            return Task.CompletedTask;
        }

        _lastSentDate = _session.Now;
        return Task.Delay(_delay, cancellationToken);
    }
}