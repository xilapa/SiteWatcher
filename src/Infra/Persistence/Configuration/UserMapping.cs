using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Configuration;

public class UserMapping : BaseModelMapping<User,UserId>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(u => u.Id)
                .HasConversion<UserId.EfCoreValueConverter>()
                .HasColumnType("uuid");

        builder.Property(u => u.GoogleId)
                .HasColumnType("varchar(30)")
                .IsRequired();

        //todo: indice único quebra em caso de user desativado
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

        builder.Property(u => u.Theme)
                .HasColumnType("smallint")
                .IsRequired();
    }
}