using System.Globalization;
using System.Text;

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

        var normalizedMessage = RemoveDiacritics(messageText);
        var key = RemoveDiacritics(KeyWord);
        bool containsText = normalizedMessage
            .Contains(key, StringComparison.OrdinalIgnoreCase);
        
        if (!containsText)
            return false;
        
        if (!MaxPrice.HasValue)
            return true;

        var extractedPrice = extractPrice();
        if (!extractedPrice.HasValue)
            return true;
        
        return extractedPrice.Value <= MaxPrice.Value;
    }
    
    private static string RemoveDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}