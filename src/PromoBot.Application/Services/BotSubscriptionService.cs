using PromoBot.Application.Interfaces;
using PromoBot.Domain.Entities;

namespace PromoBot.Application.Services;

public class BotSubscriptionService(IBotSubscriberRepository repository) : IBotSubscriptionService
{
    public async Task<List<long>> GetAllSubscribeAsync(CancellationToken ct)
    {
        var allSubscribersAsync = await repository.GetAllSubscribersAsync(ct);
        return allSubscribersAsync;
    }

    public async Task SubscribeAsync(long subscriberId, string? userName, CancellationToken ct)
    {
        var sub = await repository.GetByChatIdAsync(subscriberId, ct);

        if (sub is null)
        {
            var subscription = new BotSubscriber(subscriberId, userName);
            await repository.AddAsync(subscription, ct);
            return;
        }

        if (!sub.IsActive)
        {
            sub.Reactivate();
            await repository.UpdateAsync(sub, ct);
        }
    }

    public async Task ReactivateAsync(long subscriberChatId, CancellationToken ct = default)
    {
        var sub = await repository.GetByChatIdAsync(subscriberChatId, ct);
        
        if (sub == null || sub.IsActive)
            return;
        
        sub.Reactivate();
        await repository.UpdateAsync(sub, ct);
    }

    public async Task DeactivateAsync(long subscriberChatId, CancellationToken ct = default)
    {
        var sub = await repository.GetByChatIdAsync(subscriberChatId, ct);
        
        if (sub == null || !sub.IsActive)
            return;
        
        sub.Deactivate();
        await repository.UpdateAsync(sub, ct);
    }
}