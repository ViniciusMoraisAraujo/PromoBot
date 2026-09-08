using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Telegram;

public class TelegramBotNotifier(IConfiguration configuration) : INotifier
{
    private static readonly HttpClient _httpClient = new();
    private readonly string _botToken = configuration["TelegramBot:BotToken"] 
                                        ?? throw new InvalidOperationException("BotToken não configurado.");
    private readonly string _chatId = configuration["TelegramBot:ChatId"] 
                                      ?? throw new InvalidOperationException("ChatId de notificação não configurado.");

    public async Task NotifyAsync(Promotion promotion, CancellationToken ct = default)
    {
        string messageToSend = $"🚨 PROMOÇÃO DETECTADA 🚨\n\n{promotion.Description}";

        var payload = new
        {
            chat_id = _chatId,
            text = messageToSend
        };

        var endpoint = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        await _httpClient.PostAsJsonAsync(endpoint, payload, ct);
    }
    
}