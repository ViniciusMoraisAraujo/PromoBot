using PromoBot.Application.Interfaces;
using Microsoft.Extensions.Options;
using PromoBot.Application.Parsers;
using PromoBot.Domain.Entities;

namespace PromoBot.Application.UseCases;

public class ProcessIncomingMessageUseCase(
    IPromotionRepository promotionRepository,
    INotifier notifier,
    IOptions<List<FilterRule>> filterRulesOptions)
{
    private readonly IPromotionRepository _promotionRepository = promotionRepository;
    private readonly INotifier _notifier = notifier;
    private readonly List<FilterRule> _filterRules = filterRulesOptions.Value ?? [];
    
    public async Task ExecuteAsync(long chatId, int messageId, string messageText, CancellationToken ct = default)
    {
        Console.WriteLine($"Regras ativas: {_filterRules.Count}");
        if(string.IsNullOrEmpty(messageText))
            return;
        
        var alreadyExists = await _promotionRepository.ExistsAsync(chatId, messageId, ct);

        if (alreadyExists)
            return;
        
        var lazyPrice = new Lazy<decimal?>(() => PriceExtractor.Extract(messageText));
        var isMatch = _filterRules.Any(rule => rule.Matches(messageText, () => lazyPrice.Value));       
        
        if (!isMatch)
            return;

        var url = UrlExtractor.Extract(messageText);

        var promotion = new Promotion(
            messageId,
            chatId,
            messageText,
            DateTime.UtcNow,
            url,
            lazyPrice.Value
        );

        try
        {
            await _notifier.NotifyAsync(promotion, ct);
            promotion.MarkAsNotify();
            await _promotionRepository.AddAsync(promotion, ct);
        }
        catch (Exception e)
        {
            throw new Exception($"Error processing incoming message: {e.Message}", e);
        }
    }
}