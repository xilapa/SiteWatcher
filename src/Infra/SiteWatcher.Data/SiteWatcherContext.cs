using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SiteWatcher.Domain.Exceptions;
using Npgsql;

namespace SiteWatcher.Infra.Data;

public class SiteWatcherContext : DbContext, IUnityOfWork
{
    public static readonly string Schema = "siteWatcher_webApi";

    public SiteWatcherContext(DbContextOptions<SiteWatcherContext> dbContextOptions) : base(dbContextOptions)
    {

    }

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

            if((inner as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var modelNames = string.Join("; " ,  ex.Entries.Select(e => e.Metadata.Name.Split('.').Last()));

                throw new UniqueViolationException(modelNames);
            }

            throw ex;
        }
    }

    public DbSet<User> Users { get; set; }
}
