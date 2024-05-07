using MassTransit;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Messages;
using SiteWatcher.Domain.Common.ValueObjects;
using UnitTests.Helpers;

namespace UnitTests.Commands;

public sealed class TestMessage : BaseMessage
{
}

public class TestHandler : BaseMessageHandler<TestMessage>
{
    public TestHandler(ISiteWatcherContext context, ILogger<TestMessage> logger, ISession session) : base(context,
        logger, session)
    {
    }

    protected override Task Handle(ConsumeContext<TestMessage> context)
    {
        return Task.CompletedTask;
    }
}

public class BaseMessageHandlerTests
{
    [Fact]
    public async Task BaseMessageHandlerShouldMarkMessagesAsConsumed()
    {
        // Arrange
        var context = Substitute.For<ISiteWatcherContext>();
        var idempotentConsumers = Array.Empty<IdempotentConsumer>().AsQueryable().BuildMockDbSet();
        context.IdempotentConsumers.Returns(idempotentConsumers);

        var logger = Substitute.For<ILogger<TestMessage>>();
        var session = Substitute.For<ISession>();

        var testHandler = new TestHandler(context, logger, session);
        var consumeContext = new TestMessage().ToConsumeContext();

        // Act
        await testHandler.Consume(consumeContext);

        // Assert
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}