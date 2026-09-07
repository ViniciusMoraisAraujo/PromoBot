using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Telegram;

public class SavedMessagesNotifier(ITelegramGateway telegramGateway) : INotifier
{
    private readonly ITelegramGateway _telegramGateway = telegramGateway;
    
    public async Task NotifyAsync(Promotion promotion, CancellationToken ct = default)
    {
        string message = $"New promotion received:\n\n" +
                         $"Chat ID: {promotion.ChatId}\n" +
                         $"Message ID: {promotion.MessageId}\n" +
                         $"Value: {(promotion.Value.HasValue ? $"R$ {promotion.Value.Value:N2}" : "not identified")}";
        
        await _telegramGateway.SendToSavedMessagesAsync(message, ct);
    }
}