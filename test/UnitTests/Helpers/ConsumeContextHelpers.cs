using MassTransit;
using NSubstitute;

namespace UnitTests.Helpers;

public static class ConsumeContextHelpers
{
    public static ConsumeContext<T> ToConsumeContext<T>(this T obj) where T : class
    {
        var consumeContext = Substitute.For<ConsumeContext<T>>();
        consumeContext.CancellationToken.Returns(CancellationToken.None);
        consumeContext.Message.Returns(obj);

        return consumeContext;
    }
}