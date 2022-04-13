using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Data;

public class SiteWatcherContext : DbContext, IUnityOfWork
{
    public const string Schema = "siteWatcher_webApi";

    public SiteWatcherContext(DbContextOptions<SiteWatcherContext> dbContextOptions) : base(dbContextOptions)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
            optionsBuilder.UseInMemoryDatabase("SiteWatcherTestDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException;

            if ((inner as PostgresException)?.SqlState != PostgresErrorCodes.UniqueViolation)
                throw;

            var modelNames = string.Join("; " ,  ex.Entries.Select(e => e.Metadata.Name.Split('.').Last()));
            throw new UniqueViolationException(modelNames);
        }
    }

    public DbSet<User> Users { get; } = null!;
}
