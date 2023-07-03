using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.Exceptions;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Notifications;
using SiteWatcher.Domain.Users;
using SiteWatcher.Infra.Extensions;

namespace SiteWatcher.Infra;

public class SiteWatcherContext : DbContext, ISiteWatcherContext
{
    private readonly IAppSettings _appSettings;
    private readonly IMediator _mediator;
    public const string Schema = "sw";

    public SiteWatcherContext(IAppSettings appSettings, IMediator mediator)
    {
        _appSettings = appSettings;
        _mediator = mediator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder
            // .UseModel(SiteWatcherContextModel.Instance)
            .UseNpgsql(
                _appSettings.ConnectionString,
                opts => opts.MigrationsHistoryTable("EfMigrations", "sw"));
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
            var saveRes = await base.SaveChangesAsync(cancellationToken);

            // If CAP transaction is used, then commit it
            if (Database.CurrentTransaction != null)
                await Database.CurrentTransaction.CommitAsync(CancellationToken.None);
            return saveRes;
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException;

            if ((inner as PostgresException)?.SqlState != PostgresErrorCodes.UniqueViolation)
                throw;

            throw GenerateUniqueViolationException(ex);
        }
    }

    protected static UniqueViolationException GenerateUniqueViolationException(DbUpdateException exception)
    {
        var modelNames = string.Join("; ", exception.Entries.Select(e => e.Metadata.Name.Split('.').Last()));
        return new UniqueViolationException(modelNames);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<IdempotentConsumer> IdempotentConsumers { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}