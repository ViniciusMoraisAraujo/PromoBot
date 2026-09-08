using PromoBot.Domain.Entities;

namespace PromoBot.Application.Interfaces;

public interface IPromotionRepository
{
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task<bool> ExistsAsync(long chatId, int messageId, CancellationToken ct = default);
    Task UpdateAsync(Promotion promotion, CancellationToken ct);
}