using System.Globalization;

namespace PromoBot.Domain.Entities;

public class FilterRule
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

        bool containsText = CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            messageText,
            KeyWord,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        
        if (!containsText)
            return false;
        
        if (!MaxPrice.HasValue)
            return true;

        var extractedPrice = extractPrice();

        if (!extractedPrice.HasValue)
            return false;
        
        return extractedPrice.Value <= MaxPrice.Value;
    }
}