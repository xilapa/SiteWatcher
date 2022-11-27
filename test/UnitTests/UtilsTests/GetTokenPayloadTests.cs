using SiteWatcher.Domain.Common;

namespace UnitTests.UtilsTests;

public sealed class GetTokenPayloadTests
{
    private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
                                 ".eyJpZCI6IjdmkaXJjZXUuc2iOjE2NTM1MjcyODIsImV4cCI6MTY1MzUyNzI5MCwiaWF" +
                                 ".jX4Srj8_Do7s1B0Ra1tSBE4dQsk8dLUYUY_2Y8ONvXs";

    private const string Payload = "eyJpZCI6IjdmkaXJjZXUuc2iOjE2NTM1MjcyODIsImV4cCI6MTY1MzUyNzI5MCwiaWF";

    [Fact]
    public void CorretPayloadIsReturned()
    {
        // Arrange

        // Act
        var payload = Utils.GetTokenPayload(Token);

        // Assert
        Assert.Equal(Payload, payload);
    }
}