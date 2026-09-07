using Microsoft.EntityFrameworkCore;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Persistence;

public class PromoBotDataContext(DbContextOptions<PromoBotDataContext> options) : DbContext(options)
{
    public DbSet<Promotion> Promotions => Set<Promotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromoBotDataContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}