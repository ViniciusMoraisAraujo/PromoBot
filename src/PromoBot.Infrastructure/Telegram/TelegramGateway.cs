using Microsoft.Extensions.Options;
using PromoBot.Application.Interfaces;
using TL;

namespace PromoBot.Infrastructure.Telegram;

public class TelegramGateway : ITelegramGateway
{
    private readonly TelegramSettings _settings;
    private readonly HashSet<long> _targetChatIds;
    private readonly WTelegram.Client _client;

    public event Func<long, int, string, Task>? OnMessageReceived;

    public TelegramGateway(IOptions<TelegramSettings> settings)
    {
        _settings = settings.Value;
        _targetChatIds = [.. settings.Value.TargetChatIds];

        _client = new WTelegram.Client(ConfigResolver);
    }

    private string? ConfigResolver(string key) => key switch
    {
        "api_id" => _settings.ApiId.ToString(),
        "api_hash" => _settings.ApiHash,
        "phone_number" => _settings.PhoneNumber,
        "verification_code" => Console.ReadLine(),
        "password" => Console.ReadLine(),
        _ => null
    };

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _client.LoginUserIfNeeded();
        _client.OnUpdates += HandleUpdateAsync;
    }

    public async Task SendToSavedMessagesAsync(string message, CancellationToken ct = default)
    {
        await _client.SendMessageAsync(InputPeer.Self, message);
    }

    private async Task HandleUpdateAsync(IObject update)
    {
        try
        {
            var (peerId, msgId, text) = update switch
            {
                UpdateNewChannelMessage { message: Message m } => (m.Peer?.ID, m.id, m.message),
                UpdateNewMessage { message: Message m } => (m.Peer?.ID, m.id, m.message),
                _ => (null, 0, null)
            };

            if (peerId is null || string.IsNullOrWhiteSpace(text))
                return;

            if (_targetChatIds.Contains(peerId.Value) && OnMessageReceived is not null)
                await OnMessageReceived.Invoke(peerId.Value, msgId, text);
        }
        catch (Exception ex)
        {
            // TODO: logar de verdade (ILogger) — não deixar update engolir exceção silenciosa
            Console.WriteLine($"Erro processando update: {ex}");
        }
    }
}