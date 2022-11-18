using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PersistenceCompiledModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Emails;
using SiteWatcher.Domain.Models.Worker;
using SiteWatcher.Infra.Extensions;

namespace SiteWatcher.Infra;

public class SiteWatcherContext : DbContext, IUnitOfWork
{
    private readonly IAppSettings _appSettings;
    private readonly IMediator? _mediator;
    public const string Schema = "siteWatcher_webApi";

    public SiteWatcherContext(IAppSettings appSettings, IMediator? mediator)
    {
        _appSettings = appSettings;
        _mediator = mediator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder
            .UseModel(SiteWatcherContextModel.Instance)
            .UseNpgsql(_appSettings.ConnectionString);
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
            if(_mediator != null)
                await _mediator.DispatchDomainEvents(this);
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

    protected static UniqueViolationException GenerateUniqueViolationException(DbUpdateException exception)
    {
        var modelNames = string.Join("; ", exception.Entries.Select(e => e.Metadata.Name.Split('.').Last()));
        return new UniqueViolationException(modelNames);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<IdempotentConsumer> IdempotentConsumers { get; set; }
    public DbSet<Email> Emails { get; set; }
}