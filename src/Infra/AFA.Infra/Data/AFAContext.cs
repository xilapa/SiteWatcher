using AFA.Domain.Entities;
using AFA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AFA.Infra.Data;

public class AFAContext : DbContext, IUnityOfWork
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
