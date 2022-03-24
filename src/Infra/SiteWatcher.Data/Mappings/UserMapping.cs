using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Infra.Mappings;

public class UserMapping : BaseModelMapping<User,Guid>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(u => u.Id)
                .HasColumnType("uuid");

        builder.Property(u => u.GoogleId)
                .HasColumnType("varchar(30)")
                .IsRequired();

        builder.HasIndex(u => u.GoogleId, "IX_Unique_User_GoogleId")
                .IsUnique();

        builder.Property(u => u.Name)
                .HasColumnType("varchar(64)")
                .IsRequired();
        
        builder.Property(u => u.Email)
                .HasColumnType("varchar(320)")
                .IsRequired();

        builder.Property(u => u.EmailConfirmed)
                .HasColumnType("boolean")
                .IsRequired();

        builder.Property(u => u.Language)
                .HasColumnType("smallint")
                .IsRequired();
    }
}