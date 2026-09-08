using Microsoft.Extensions.Logging;
using PromoBot.Application.Interfaces;
using Microsoft.Extensions.Options;
using PromoBot.Application.Parsers;
using PromoBot.Domain.Entities;

namespace PromoBot.Application.UseCases;

public class ProcessIncomingMessageUseCase(
    IPromotionRepository promotionRepository,
    INotifier notifier,
    IOptions<List<FilterRule>> filterRulesOptions,
    ILogger<ProcessIncomingMessageUseCase> logger)
{
    private readonly IPromotionRepository _promotionRepository = promotionRepository;
    private readonly INotifier _notifier = notifier;
    private readonly List<FilterRule> _filterRules = filterRulesOptions.Value ?? [];
    private readonly ILogger<ProcessIncomingMessageUseCase> _logger = logger;
    
    public async Task ExecuteAsync(long chatId, int messageId, string messageText, CancellationToken ct = default)
    {
        _logger.LogInformation("Processing message {MessageId} from Chat {ChatId} (Active rules: {RuleCount})", messageId, chatId, _filterRules.Count);

        if(string.IsNullOrEmpty(messageText))
        {
            _logger.LogInformation("Received empty message from Chat {ChatId} (Id: {MessageId})", chatId, messageId);
            return;
        }
        
        var alreadyExists = await _promotionRepository.ExistsAsync(chatId, messageId, ct);

        if (alreadyExists)
        {
            _logger.LogInformation("Promotion already exists for Chat {ChatId} (Id: {MessageId})", chatId, messageId);
            return;
        }
        
        var lazyPrice = new Lazy<decimal?>(() => PriceExtractor.Extract(messageText));
        var matchedRule = _filterRules.FirstOrDefault(rule => rule.Matches(messageText, () => lazyPrice.Value));       
        
        if (matchedRule is null)
        {
            _logger.LogInformation("Message from Chat {ChatId} (Id: {MessageId}) does not match any filter rules", chatId, messageId);
            return;
        }

        _logger.LogInformation("Message {MessageId} from Chat {ChatId} matched rule '{KeyWord}'", messageId, chatId, matchedRule.KeyWord);

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
            _logger.LogInformation("Promotion from Chat {ChatId} (Id: {MessageId}) sent to Saved Messages and saved to database", chatId, messageId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing incoming message {MessageId} from Chat {ChatId}", messageId, chatId);
            throw new Exception($"Error processing incoming message: {e.Message}", e);
        }
    }
}