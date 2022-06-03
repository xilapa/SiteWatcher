using SiteWatcher.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SiteWatcher.Infra.Configuration;

public abstract class BaseModelMapping<TModel,IdType> : IEntityTypeConfiguration<TModel> where TModel : BaseModel<IdType>
{
    public virtual void Configure(EntityTypeBuilder<TModel> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Active)
                .HasColumnType("boolean")
                .IsRequired();

        builder.Property(m => m.CreatedAt)
                .HasColumnType("timestamp")
                .IsRequired();

        builder.Property(m => m.LastUpdatedAt)
                .HasColumnType("timestamp")
                .IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}