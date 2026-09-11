using Microsoft.EntityFrameworkCore;
using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Persistence;

public class PromotionRepository(PromoBotDataContext context) : IPromotionRepository
{
    private readonly PromoBotDataContext _context = context;

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
    {
        await _context.Promotions.AddAsync(promotion, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(long chatId, int messageId, CancellationToken ct = default)
    {
        return await _context.Promotions
            .AnyAsync(p => p.ChatId == chatId && p.MessageId == messageId, ct);
    }

    public async Task<int> DeleteOlderPromotionThanAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        var deleted = await _context.Promotions
            .Where(p => p.PromotionDate < cutoffDate)
            .ExecuteDeleteAsync(ct);

        return deleted;
    }
}