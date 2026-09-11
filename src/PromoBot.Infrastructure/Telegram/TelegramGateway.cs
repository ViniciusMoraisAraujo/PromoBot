using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromoBot.Application.Interfaces;
using PromoBot.Domain.Models;
using TL;
using Channel = System.Threading.Channels.Channel;

namespace PromoBot.Infrastructure.Telegram;

public class TelegramGateway : ITelegramGateway
{
    private readonly TelegramSettings _settings;
    private readonly HashSet<long> _targetChatIds;
    private readonly WTelegram.Client _client;
    private readonly ILogger<TelegramGateway> _logger;

    private readonly Channel<IncomingMessage> _channel = Channel.CreateUnbounded<IncomingMessage>(
        new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = false
        });
    
    public ChannelReader<IncomingMessage>  Messages => _channel.Reader;
    
    public TelegramGateway(IOptions<TelegramSettings> settings, ILogger<TelegramGateway> logger) 
    { 
        _logger = logger;
        _settings = settings.Value;
        _targetChatIds = settings.Value.TargetChatIds
            .Select(NormalizeChatId)
            .ToHashSet();

        _client = new WTelegram.Client(ConfigResolver);
    }

    private static long NormalizeChatId(long id)
    {
        string idStr = id.ToString();
        if (idStr.StartsWith("-100"))
            return long.Parse(idStr[4..]);

        return Math.Abs(id);
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
        _client.OnUpdates += HandleUpdate;
    }

    public async Task SendToSavedMessagesAsync(string message, CancellationToken ct = default)
    {
        await _client.SendMessageAsync(InputPeer.Self, message);
    }

    private Task HandleUpdate(IObject update)
    {
        try
        {
            if (update is not UpdatesBase updates)
                return Task.CompletedTask;

            foreach (var u in updates.UpdateList)
            {
                var (peerId, msgId, text) = u switch
                {
                    UpdateNewChannelMessage { message: Message m } => (m.Peer?.ID, m.id, m.message),
                    UpdateNewMessage { message: Message m } => (m.Peer?.ID, m.id, m.message),
                    _ => (null, 0, null)
                };

                if (peerId is null || string.IsNullOrWhiteSpace(text))
                    continue;

                if (_targetChatIds.Contains(peerId.Value))
                {
                    _channel.Writer.TryWrite(new IncomingMessage(peerId.Value, msgId, text));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Falha ao processar update: {Exception}", ex);
        }

        return Task.CompletedTask;
    }
}