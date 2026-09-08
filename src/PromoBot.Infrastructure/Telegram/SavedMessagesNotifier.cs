using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Telegram;

public class SavedMessagesNotifier(ITelegramGateway telegramGateway) : INotifier
{
    private readonly ITelegramGateway _telegramGateway = telegramGateway;
    
    public async Task NotifyAsync(Promotion promotion, CancellationToken ct = default)
    {
        string message = $"🚨 PROMOÇÃO DETECTADA 🚨\n\n{promotion.Description}";
        
        await _telegramGateway.SendToSavedMessagesAsync(message, ct);
    }
}