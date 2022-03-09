using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using SiteWatcher.Domain.Exceptions;
using Npgsql;
using System.Linq;
using System.Collections.Generic;

namespace SiteWatcher.Infra.Data;

public class SiteWatcherContext : DbContext, IUnityOfWork
{
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
                var modelName = ex.Entries[0].Metadata.Name.Split('.').Last();
                throw new UniqueViolationException(modelName);
            }

            throw ex;
        }
    }

    public DbSet<User> Users { get; set; }
}
