using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class RuleMapping : BaseModelMapping<Rule, RuleId>
{
    public override void Configure(EntityTypeBuilder<Rule> builder)
    {
        base.Configure(builder);

        builder.ToTable("Rules");

        builder.Property<AlertId>(nameof(AlertId));

        builder.HasIndex(nameof(AlertId));

        builder.Property(a => a.Id)
            .HasConversion<RuleId.EfCoreValueConverter>()
            .HasValueGeneratorFactory<RuleIdValueGeneratorFactory>()
            .HasColumnType("integer")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(w => w.FirstWatchDone)
            .HasColumnType("boolean")
            .IsRequired();

        builder.HasDiscriminator(r => r.RuleType)
            .HasValue<AnyChangesRule>(RuleType.AnyChanges)
            .HasValue<TermRule>(RuleType.Term)
            .HasValue<RegexRule>(RuleType.Regex);

        builder.Property(r => r.RuleType)
            .HasColumnType("char")
            .HasConversion<RuleConverter>()
            .IsRequired();
    }
}

public class AnyChangesRuleMapping : IEntityTypeConfiguration<AnyChangesRule>
{
    public void Configure(EntityTypeBuilder<AnyChangesRule> builder)
    {
        builder.Property(a => a.HtmlHash)
            .HasColumnType("varchar(64)");
    }
}

public class TermRuleMapping : IEntityTypeConfiguration<TermRule>
{
    public void Configure(EntityTypeBuilder<TermRule> builder)
    {
        builder.Property(t => t.Term)
            .HasColumnType("varchar(64)");

        builder.OwnsMany(t => t.Occurrences,
            b =>
            {
                b.ToTable("TermOccurrences");

                b.Property(o => o.Context)
                    .HasColumnType("varchar(512)");
            });

        builder.Metadata
            .FindNavigation(nameof(TermRule.Occurrences))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RegexRuleMapping : IEntityTypeConfiguration<RegexRule>
{
    public void Configure(EntityTypeBuilder<RegexRule> builder)
    {
        builder.Property(r => r.NotifyOnDisappearance)
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(r => r.RegexPattern)
            .HasColumnType("varchar(512)")
            .IsRequired();

        builder.Property<Matches>("_matches")
            .HasConversion<RegexRuleMatchesValueConverter>()
            .HasColumnType("text")
            .HasColumnName(nameof(RegexRule.Matches));
    }
}