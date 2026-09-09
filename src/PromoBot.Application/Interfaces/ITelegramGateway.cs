using System.Threading.Channels;
using PromoBot.Domain.Models;

namespace PromoBot.Application.Interfaces;

public interface ITelegramGateway
{
    Task StartAsync(CancellationToken ct = default);
    Task SendToSavedMessagesAsync(string message, CancellationToken ct = default);
    
    ChannelReader<IncomingMessage> Messages { get; }
}