namespace PromoBot.Domain.Entities;

public record FilterRule(string KeyWord, decimal? MaxPrice)
{
    public bool Matches(string messageText, Func<decimal?> extractPrice)
    {
        if(string.IsNullOrWhiteSpace(messageText))
            return false;
        
        bool containsText = messageText.Contains(KeyWord, StringComparison.OrdinalIgnoreCase);
        
        if (!containsText)
            return false;
        
        if(!MaxPrice.HasValue)
            return true;


        var extractedPrice = extractPrice();
        if (!extractedPrice.HasValue)
            return true;
        
        return extractedPrice.Value <= MaxPrice.Value;
    }
}