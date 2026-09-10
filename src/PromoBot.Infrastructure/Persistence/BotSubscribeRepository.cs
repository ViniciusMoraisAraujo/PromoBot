using Microsoft.EntityFrameworkCore;
using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Persistence;

public class BotSubscribeRepository(PromoBotDataContext context) : IBotSubscriberRepository
{
    public async Task<BotSubscriber?> GetByChatIdAsync(long chatId, CancellationToken ct = default)
    {
        return await context.BotSubscribers
            .FirstOrDefaultAsync(x => x.ChatId == chatId, ct);
    }

    public async Task AddAsync(BotSubscriber subscriber, CancellationToken ct = default)
    {
        await context.BotSubscribers.AddAsync(subscriber, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BotSubscriber subscriber, CancellationToken ct = default)
    {
        context.BotSubscribers.Update(subscriber);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<long>> GetAllSubscribersAsync(CancellationToken ct = default)
    {
        var subscriberList = await context.BotSubscribers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => x.ChatId)
            .ToListAsync(ct);
        
        return subscriberList;
    }
}