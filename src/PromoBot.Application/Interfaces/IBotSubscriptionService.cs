using PromoBot.Domain.Entities;

namespace PromoBot.Application.Interfaces;

public interface IBotSubscriptionService
{
    Task<List<long>> GetAllSubscribeAsync(CancellationToken ct);
    Task SubscribeAsync(long subscriberId, string? userName,CancellationToken ct);
    Task ReactivateAsync(long chatId, CancellationToken ct = default);
    Task DeactivateAsync(long chatId, CancellationToken ct = default);
}