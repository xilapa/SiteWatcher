using SiteWatcher.Domain.Common.Messages;

namespace UnitTests.ModelTests;

public sealed class BaseMessageTests
{
    [Fact]
    public void MessageImplementationShouldHaveEmptyPublicConstructor()
    {
        var messages = typeof(BaseMessage).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseMessage)))
            .ToArray();

        foreach (var message in messages)
        {
            var hasTheCtor = message.GetConstructors().Any(c => c.GetParameters().Length == 0);
            Assert.True(hasTheCtor, $"{message.Name} should have an empty public constructor");
        }
    }
}