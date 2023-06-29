using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class EmailMapping : BaseModelMapping<Email, EmailId>
{
    public override void Configure(EntityTypeBuilder<Email> builder)
    {
        base.Configure(builder);

        builder.ToTable("Emails");

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

        builder.Property(e => e.ErrorDate)
            .HasColumnType("timestamptz")
            .IsRequired(false);
    }
}