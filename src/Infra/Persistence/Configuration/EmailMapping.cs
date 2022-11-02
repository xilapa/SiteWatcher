using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models.Emails;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class EmailMapping : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion<EmailId.EfCoreValueConverter>()
            .HasValueGeneratorFactory<EmailIdValueGeneratorFactory>()
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(e => e.Recipients)
            .HasConversion<EmailRecipientsValueConverter>()
            .IsRequired();

        builder.Property(e => e.DateSent)
            .HasColumnType("timestamptz")
            .IsRequired();

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