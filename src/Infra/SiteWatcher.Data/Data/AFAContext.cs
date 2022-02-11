using SiteWatcher.Domain.Entities;
using SiteWatcher.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<User> Users { get; set; }
}
