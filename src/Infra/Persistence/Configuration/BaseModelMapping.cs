using SiteWatcher.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SiteWatcher.Infra.Persistence.Configuration;

public abstract class BaseModelMapping<TModel,IdType> : IEntityTypeConfiguration<TModel> where TModel : BaseModel<IdType>
{
    public virtual void Configure(EntityTypeBuilder<TModel> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Active)
                .HasColumnType("boolean")
                .IsRequired();

        builder.Property(m => m.CreatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

        builder.Property(m => m.LastUpdatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}