using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Infra;

public class SiteWatcherContext : DbContext, IUnityOfWork
{
    private readonly IAppSettings _appSettings;
    public const string Schema = "siteWatcher_webApi";

    public SiteWatcherContext(IAppSettings appSettings)
    {
        _appSettings = appSettings;
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

    public DbSet<User> Users { get; set; }
}
