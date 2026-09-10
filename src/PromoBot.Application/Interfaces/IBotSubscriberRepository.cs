using PromoBot.Domain.Entities;

namespace PromoBot.Application.Interfaces;

public interface IBotSubscriberRepository
{
    Task<BotSubscriber?> GetByChatIdAsync(long chatId, CancellationToken ct = default);
    Task AddAsync(BotSubscriber subscriber, CancellationToken ct = default);
    Task UpdateAsync (BotSubscriber subscriber, CancellationToken ct = default);
    Task<List<long>> GetAllSubscribersAsync(CancellationToken ct = default);
}