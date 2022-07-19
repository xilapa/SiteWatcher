using AutoMapper;
using Domain.DTOs.Alert;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace UnitTests.Commands;

public class UpdateAlertCommandTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new ();

    public static IEnumerable<object[]> InvalidIdData()
    {
        yield return new object[] {new AlertId(0)};
        yield return new object[] {AlertId.Empty};
    }

    [Theory]
    [MemberData(nameof(InvalidIdData))]
    public async Task AlertIsNotUpdatedWithInvalidId(AlertId alertId)
    {
        // Arrange
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<UpdateAlertInput>(It.IsAny<object>()))
            .Returns(new UpdateAlertInput {AlertId = alertId});

        var handler = new UpdateAlertCommmandHandler(mapperMock.Object, _alertRepositoryMock.Object, null!, null!);
        var command = new UpdateAlertCommmand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors
            .Should()
            .BeEquivalentTo(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.AlertId)));

        _alertRepositoryMock
            .Verify(r => r.GetAlertForUpdate(It.IsAny<AlertId>(), It.IsAny<UserId>()),
                Times.Never);
    }

    [Fact]
    public async Task NonexistentAlertDoesntCallSaveChanges()
    {
        // Arrange
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<UpdateAlertInput>(It.IsAny<object>()))
            .Returns(new UpdateAlertInput {AlertId = new AlertId(1)});

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.UserId).Returns(UserId.Empty);

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new UpdateAlertCommmandHandler(mapperMock.Object, _alertRepositoryMock.Object, sessionMock.Object,
            uowMock.Object);
        var command = new UpdateAlertCommmand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors
            .Should()
            .BeEquivalentTo(ApplicationErrors.ALERT_DO_NOT_EXIST);

        uowMock
            .Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}