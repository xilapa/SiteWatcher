using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models.Worker;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class IdempotentConsumerMapping : IEntityTypeConfiguration<IdempotentConsumer>
{
    public void Configure(EntityTypeBuilder<IdempotentConsumer> builder)
    {
        builder.ToTable("IdempotentConsumers", schema: "worker");

        builder.HasKey(i => new { i.MessageId, i.Consumer });

        builder.Property(i => i.MessageId)
            .HasColumnType("varchar(128)");

        builder.Property(i => i.Consumer)
            .HasColumnType("varchar(128)");
    }
}