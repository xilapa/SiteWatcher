using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.IdempotentConsumers;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace IntegrationTests.IdempotentConsumers;

public sealed class CleanIdempotentConsumersTestBase : BaseTestFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await AppFactory.WithDbContext(ctx =>
        {
            AppFactory.CurrentTime = DateTime.UtcNow;
            var fiveDaysAhead = DateTime.UtcNow.AddDays(5); 

            var idempotentConsumers = Enumerable.Range(0, 10)
                .Select(x => new IdempotentConsumer
                {
                    Consumer = "TestConsumer",
                    DateCreated = x < 8 ? AppFactory.CurrentTime : fiveDaysAhead,
                    MessageId = $"{x}"
                });
            ctx.IdempotentConsumers.AddRange(idempotentConsumers);
            return ctx.SaveChangesAsync();
        });
    }
}

public sealed class CleanIdempotentConsumersTests : BaseTest, IClassFixture<CleanIdempotentConsumersTestBase>
{
    public CleanIdempotentConsumersTests(CleanIdempotentConsumersTestBase fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task IdempotentConsumersAreCleanedAfterFiveDays()
    {
        // Arrange
        await using var scope = AppFactory.Services.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<ISession>() as TestSession;
        var handler =  scope.ServiceProvider.GetRequiredService<CleanIdempotentConsumers>();
        var initialCount = await GetIdempotentConsumersCount();

        // Act & Assert

        // First time nothing will be cleaned
        await handler.Clean(CancellationToken.None);
        var currentCount = await GetIdempotentConsumersCount();
        currentCount.Should().Be(initialCount);

        // After five days, 8 should be deleted
        session!.SetNewDate(session.Now.Date.AddDays(6));
        await handler.Clean(CancellationToken.None);
        currentCount = await GetIdempotentConsumersCount();
        currentCount.Should().Be(initialCount - 8);

        // After another five days, the 3 left should be deleted
        session!.SetNewDate(session.Now.Date.AddDays(5));
        await handler.Clean(CancellationToken.None);
        currentCount = await GetIdempotentConsumersCount();
        currentCount.Should().Be(0);
    }

    private Task<int> GetIdempotentConsumersCount()
    {
        return AppFactory.WithDbContext(ctx => ctx.IdempotentConsumers.CountAsync());
    }
}