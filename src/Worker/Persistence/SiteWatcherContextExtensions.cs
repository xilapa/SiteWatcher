using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Models;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Persistence;

public static class SiteWatcherContextExtensions
{
    public static async Task<bool> HasBeenProcessed(this SiteWatcherContext context, string messageId, string consumerName) =>
         await context.IdempotentConsumers
            .AsNoTracking()
            .AnyAsync(i => i.Consumer == consumerName && i.MessageId == messageId);

        public static async Task MarkMessageAsConsumed(this SiteWatcherContext context, string messageId, string consumerName)
        {
            var idemPotencyConsumer = new IdempotentConsumer()
            {
                MessageId = messageId,
                Consumer = consumerName,
            };
            context.Add(idemPotencyConsumer);
            await context.SaveChangesAsync();
        }
}