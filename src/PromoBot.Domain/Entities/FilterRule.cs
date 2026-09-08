namespace PromoBot.Domain.Entities;

public record FilterRule
{
    public string KeyWord { get; init; } = string.Empty;
    public decimal? MaxPrice { get; init; }

    public FilterRule()
    {
    }

    public FilterRule(string keyWord, decimal? maxPrice = null)
    {
        KeyWord = keyWord;
        MaxPrice = maxPrice;
    }

    public bool Matches(string messageText, Func<decimal?> extractPrice)
    {
        if (string.IsNullOrWhiteSpace(messageText) || string.IsNullOrWhiteSpace(KeyWord))
            return false;
        
        bool containsText = messageText.Contains(KeyWord, StringComparison.OrdinalIgnoreCase);
        
        if (!containsText)
            return false;
        
        if (!MaxPrice.HasValue)
            return true;

        var extractedPrice = extractPrice();
        if (!extractedPrice.HasValue)
            return true;
        
        return extractedPrice.Value <= MaxPrice.Value;
    }
}