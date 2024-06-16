using System.Diagnostics;
using FluentAssertions;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Services;

public sealed class EmailThrottlerTests
{
    private readonly EmailSettings _emailSettings;
    private readonly EmailThrottler _emailThrottler;
    private readonly TestSession _session;

    public EmailThrottlerTests()
    {
        _emailSettings = new EmailSettings
        {
            EmailDelaySeconds = 5
        };
        _session = new TestSession(null!, DateTime.UtcNow);
        _emailThrottler = new EmailThrottler(_emailSettings, _session);
    }

    [Fact]
    public async Task FirstCallShouldNotHaveDelay()
    {
        // Arrange
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var emailDelay = TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds);

        // Act
        await _emailThrottler.WaitToSend(CancellationToken.None);
        stopWatch.Stop();

        // Assert
        stopWatch.Elapsed.Should().BeLessThan(emailDelay);
    }

    [Fact]
    public async Task ShouldNotHaveDelayIfDelayHasAlreadyPassed()
    {
        // Arrange
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var emailDelay = TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds);

        // Simulate an email sending
        await _emailThrottler.WaitToSend(CancellationToken.None);

        // Simulate that time has passed
        var timeAfterDelay = _session.Now.Add(emailDelay).AddSeconds(1);
        _session.SetNewDate(timeAfterDelay);

        // Act
        await _emailThrottler.WaitToSend(CancellationToken.None);
        stopWatch.Stop();

        // Assert
        stopWatch.Elapsed.Should().BeLessThan(emailDelay);
    }

    [Fact]
    public async Task EmailsAreSentRespectingDelay()
    {
        // Arrange
        var stopWatch = new Stopwatch();
        var emailDelay = TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds);

        // Simulate an email sending
        await _emailThrottler.WaitToSend(CancellationToken.None);

        // Act
        stopWatch.Start();
        await _emailThrottler.WaitToSend(CancellationToken.None);
        stopWatch.Stop();

        // Assert
        stopWatch.Elapsed.Should().BeGreaterOrEqualTo(emailDelay);
    }
}