using AFA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AFA.Infra.Data;
public class AFAContext : DbContext
{
    public AFAContext(DbContextOptions<AFAContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
            optionsBuilder.UseInMemoryDatabase("AFATestDb");
    }

    public DbSet<User> Users { get; set; }
}
