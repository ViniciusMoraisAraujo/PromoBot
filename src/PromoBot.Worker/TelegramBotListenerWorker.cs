using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using PromoBot.Application.Interfaces;

namespace PromoBot.Worker;

public class TelegramBotListenerWorker(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramBotListenerWorker> logger) : BackgroundService
{
    private readonly TelegramBotClient _botClient = new(
        configuration["TelegramBot:BotToken"] 
        ?? throw new InvalidOperationException("BotToken não configurado."));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando escuta de mensagens do Telegram Bot (/start)...");

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text is null)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text.Trim();

        using var scope = scopeFactory.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<IBotSubscriptionService>();

        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            await subscriptionService.SubscribeAsync(chatId, message.Chat.Username, ct);

            await bot.SendMessage(
                chatId: chatId,
                text: "👋 Olá! Você se inscreveu com sucesso. A partir de agora você receberá todas as promoções aqui!",
                cancellationToken: ct);

            logger.LogInformation("Novo inscrito registrado: ChatId {ChatId} (@{Username})", chatId, message.Chat.Username);
        }
        else if (text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
        {
            await subscriptionService.DeactivateAsync(chatId, ct);

            await bot.SendMessage(
                chatId: chatId,
                text: "Você cancelou a inscrição e não receberá mais notificações.",
                cancellationToken: ct);
            
            logger.LogInformation("Inscrição cancelada: ChatId {ChatId}", chatId);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Erro no Telegram Bot Listener");
        return Task.CompletedTask;
    }
}