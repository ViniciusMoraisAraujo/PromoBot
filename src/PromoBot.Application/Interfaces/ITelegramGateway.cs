namespace PromoBot.Application.Interfaces;

public interface ITelegramGateway
{
    Task StartAsync(CancellationToken ct = default);
    Task SendToSavedMessagesAsync(string message, CancellationToken ct = default);
    event Func<long, int, string, Task>? OnMessageReceived;
}