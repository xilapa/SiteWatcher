using System.Reflection;
using DotNetCore.CAP;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly ICapPublisher _capPublisher;
    private IDbContextTransaction? _currentTransaction;
    public const string Schema = "sw";

    public SiteWatcherContext(IAppSettings appSettings, IMediator mediator, ICapPublisher capPublisher)
    {
        _appSettings = appSettings;
        _mediator = mediator;
        _capPublisher = capPublisher;
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

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        if(_currentTransaction != null)
            return _currentTransaction;

        _currentTransaction = await Database.BeginTransactionAsync(_capPublisher, autoCommit: false, ct);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct)
    {
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            DisposeTransaction();
        }
    }

    private async Task RollbackTransactionAsync(CancellationToken ct)
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            DisposeTransaction();
        }
    }

    private void DisposeTransaction()
    {
        if (_currentTransaction == null) return;
        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
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
    public DbSet<Notification> Notifications { get; set; }
}