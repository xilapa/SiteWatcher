using FluentAssertions;
using MockQueryable.NSubstitute;
using Moq;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;

namespace UnitTests.Commands;



public sealed class UpdateAlertCommandTests
{
    private readonly Mock<IIdHasher> _hasherMock;

    public UpdateAlertCommandTests()
    {
        _hasherMock = new Mock<IIdHasher>();
    }

    public static TheoryData<int> InvalidIds => new()
    {
        0,
        default!
    };

    [Theory]
    [MemberData(nameof(InvalidIds))]
    public async Task AlertIsNotUpdatedWithInvalidId(int alertId)
    {
        // Arrange
        _hasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(alertId);

        var handler = new UpdateAlertCommandHandler(_hasherMock.Object,null!, null!);
        var command = new UpdateAlertCommmand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None) as ErrorResult;

        // Assert
        result!.Errors
            .Should()
            .BeEquivalentTo(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));
    }

    [Fact]
    public async Task NonexistentAlertDoesntCallSaveChanges()
    {
        // Arrange
        _hasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(1);

        var alertDbSetMock = Array.Empty<Alert>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Alerts).Returns(alertDbSetMock);

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.UserId).Returns(UserId.Empty);

        var handler = new UpdateAlertCommandHandler(_hasherMock.Object, contextMock.Object, sessionMock.Object);
        var command = new UpdateAlertCommmand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None) as ErrorResult;

        // Assert
        result!.Errors
            .Should()
            .BeEquivalentTo(ApplicationErrors.ALERT_DO_NOT_EXIST);

        contextMock
            .Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}