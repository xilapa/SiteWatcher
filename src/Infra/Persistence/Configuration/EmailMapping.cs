using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class EmailMapping : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion<EmailId.EfCoreValueConverter>()
            .HasColumnType("uuid");

        // UserName + : + UserEmail = 64 + 1 + 320 = 385
        builder.Property(e => e.Recipient)
            .HasColumnType("varchar(385)")
            .IsRequired();

        builder.Property(e => e.DateSent)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(e => e.Subject)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(e => e.Body)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasColumnType("text")
            .IsRequired(false);
    }
}