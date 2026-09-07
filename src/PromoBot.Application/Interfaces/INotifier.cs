using PromoBot.Domain.Entities;

namespace PromoBot.Application.Interfaces;

public interface INotifier
{
    Task NotifyAsync(Promotion promotion, CancellationToken ct = default);
}