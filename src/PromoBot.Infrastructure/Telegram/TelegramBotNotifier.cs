using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Telegram;

public class TelegramBotNotifier(
    IConfiguration configuration, 
    HttpClient httpClient,
    IBotSubscriptionService subscriptionService,
    ILogger<TelegramBotNotifier> logger) : INotifier
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IBotSubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<TelegramBotNotifier> _logger = logger;
    private readonly string _botToken = configuration["TelegramBot:BotToken"] 
                                        ?? throw new InvalidOperationException("BotToken não configurado.");

    public async Task NotifyAsync(Promotion promotion, CancellationToken ct = default)
    {
        var subscribers = await _subscriptionService.GetAllSubscribeAsync(ct);
        if (subscribers.Count == 0) return;

        string messageToSend = $"🚨 PROMOÇÃO DETECTADA 🚨\n\n{promotion.Description}";
        var endpoint = $"https://api.telegram.org/bot{_botToken}/sendMessage";

        foreach (var chatId in subscribers)
        {
            try
            {
                var payload = new
                {
                    chat_id = chatId,
                    text = messageToSend
                };

                var response = await _httpClient.PostAsJsonAsync(endpoint, payload, ct);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    await _subscriptionService.DeactivateAsync(chatId, ct);
                    _logger.LogWarning("Usuário {ChatId} bloqueou o bot. Inscrição desativada.", chatId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar promoção para o ChatId {ChatId}", chatId);
            }
        }
    }
}