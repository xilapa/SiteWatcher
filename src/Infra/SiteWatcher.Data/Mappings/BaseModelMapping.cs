using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Data.Mappings;

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
    }
}