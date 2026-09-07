namespace PromoBot.Domain.Entities;

public record FilterRule(string KeyWord, decimal? MaxPrice)
{
    public bool Matches(string messageText, decimal? extractedPrice)
    {
        if(string.IsNullOrWhiteSpace(messageText))
            return false;
        
        bool containsText = messageText.Contains(KeyWord, StringComparison.OrdinalIgnoreCase);
        
        if (!containsText)
            return false;
        
        if(!MaxPrice.HasValue)
            return true;

        if (!extractedPrice.HasValue)
            return true;
        
        return extractedPrice.Value <= MaxPrice.Value;
    }
}