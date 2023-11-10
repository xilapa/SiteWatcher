using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class IdempotentConsumerMapping : IEntityTypeConfiguration<IdempotentConsumer>
{
    public void Configure(EntityTypeBuilder<IdempotentConsumer> builder)
    {
        builder.ToTable("IdempotentConsumers", schema: SiteWatcherContext.Schema);

        builder.HasKey(i => new { i.MessageId, i.Consumer });

        builder.Property(i => i.MessageId)
            .HasColumnType("varchar(128)");

        builder.Property(i => i.Consumer)
            .HasColumnType("varchar(128)");

        builder.Property(i => i.DateCreated)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
    }
}