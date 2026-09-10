using Microsoft.EntityFrameworkCore;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Persistence;

public class PromoBotDataContext(DbContextOptions<PromoBotDataContext> options) : DbContext(options)
{
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<BotSubscriber> BotSubscribers => Set<BotSubscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromoBotDataContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}