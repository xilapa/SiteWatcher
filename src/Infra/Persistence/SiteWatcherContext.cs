using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Infra.Configuration;
using SiteWatcher.Infra.Extensions;

namespace SiteWatcher.Infra;

public class SiteWatcherContext : DbContext, IUnitOfWork
{
    private readonly IAppSettings _appSettings;
    private readonly IMediator _mediator;
    public const string Schema = "siteWatcher_webApi";

    public SiteWatcherContext(IAppSettings appSettings, IMediator mediator)
    {
        _appSettings = appSettings;
        _mediator = mediator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(_appSettings.ConnectionString);
        if (_appSettings.IsDevelopment)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _mediator.DispatchDomainEvents(this);
            PopulateSearchProperties();
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException;

            if ((inner as PostgresException)?.SqlState != PostgresErrorCodes.UniqueViolation)
                throw;

            throw GenerateUniqueViolationException(ex);
        }
    }

    // Postgres collations doesnt support LIKE, call Postgres unaccent extension doesn't make use of indexes,
    // So, not normalized columns for case and accent insensitive search are the best solution
    private void PopulateSearchProperties()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                continue;

            if (entry.Entity is Alert alert)
            {
                entry.Property(AlertMapping.AlertNameSearch).CurrentValue = alert.Name.ToLowerCaseWithoutDiacritics();
                var siteEntry = entry.Navigation(nameof(Site)).EntityEntry;
                if (siteEntry.State != EntityState.Added && siteEntry.State != EntityState.Modified)
                    continue;

                entry.Property(AlertMapping.SiteNameSearch).CurrentValue = alert.Site.Name.ToLowerCaseWithoutDiacritics();
                entry.Property(AlertMapping.SiteUriSearch).CurrentValue = alert.Site.Uri.AbsoluteUri.ToLowerCaseWithoutDiacritics();
            }
        }
    }

    protected static UniqueViolationException GenerateUniqueViolationException(DbUpdateException exception)
    {
        var modelNames = string.Join("; ", exception.Entries.Select(e => e.Metadata.Name.Split('.').Last()));
        return new UniqueViolationException(modelNames);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Alert> Alerts { get; set; }
}