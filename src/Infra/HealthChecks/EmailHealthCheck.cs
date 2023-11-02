using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SiteWatcher.Infra.EmailSending;

namespace SiteWatcher.Infra.HealthChecks;

public sealed class EmailHealthCheck : IHealthCheck
{
    private readonly EmailSettings _settings;

    public EmailHealthCheck(EmailSettings settings)
    {
        _settings = settings;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var smtpClient = new SmtpClient();
        var fiveSecondsToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            if (!smtpClient.IsConnected)
                await smtpClient.ConnectAsync(_settings.SmtpHost, _settings.Port, _settings.UseSsl, fiveSecondsToken.Token);

            if (!smtpClient.IsAuthenticated)
                await smtpClient.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword, fiveSecondsToken.Token);

            await smtpClient.DisconnectAsync(true, CancellationToken.None);
        }
        catch(Exception e)
        {
            if (e is OperationCanceledException || e is TaskCanceledException || e is TimeoutException)
                return HealthCheckResult.Degraded("Took more than five seconds to connect to the email host", e);

            return HealthCheckResult.Unhealthy("Cannot connect to the email host", e);
        }

        return HealthCheckResult.Healthy();
    }
}