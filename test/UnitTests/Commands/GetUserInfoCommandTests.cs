using FluentAssertions;
using Moq;
using SiteWatcher.Application.Users.Commands.GetUserinfo;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;

namespace UnitTests.Commands;

public class GetUserInfoCommandTests
{
    [Fact]
    public async Task GetUserInfoReturnsNullWhenIdIsEmpty()
    {
        // Arrange
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.UserId).Returns(UserId.Empty);

        var cmmd = new GetUserInfoCommand();
        var handler = new GetUserInfoCommandHandler(null!, sessionMock.Object);

        // Act
        var res = await handler.Handle(cmmd, CancellationToken.None);

        // Assert
        res.Should().BeNull();
    }
}