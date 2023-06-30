using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Users;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class UpdateUserCommandTests
{
    [Fact]
    public async Task CantUpdateNonExistingUser()
    {
        // Arrange
        var dbSetMock = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);
        var session = new Mock<ISession>().Object;

        var commandHandler = new UpdateUserCommandHandler(contextMock.Object, session, new FakeCache());

        // Act
        var result = await commandHandler.Handle(new UpdateUserCommand(), CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.USER_DO_NOT_EXIST);
    }
}