using System.Globalization;
using System.Text.RegularExpressions;

namespace PromoBot.Application.Parsers;

public static partial class PriceExtractor
{
    [GeneratedRegex(@"(?:R\$\s?)(?<valor>(?:\d{1,3}(?:\.\d{3})+|\d+)(?:,\d{2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex PricePattern();

    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static decimal? Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var matches = PricePattern().Matches(text);
        
        if (matches.Count == 0)
            return null;

        foreach (Match match in matches)
        {
            int start = Math.Max(0, match.Index - 25);
            string beforeContext = text[start..match.Index];

            int end = Math.Min(text.Length, match.Index + match.Length + 15);
            string afterContext = text[(match.Index + match.Length)..end];

            bool isCouponBefore = beforeContext.Contains("cupom", StringComparison.OrdinalIgnoreCase) ||
                                  beforeContext.Contains("desconto de", StringComparison.OrdinalIgnoreCase);

            bool isDiscountAfter = afterContext.Contains("off", StringComparison.OrdinalIgnoreCase) ||
                                   afterContext.Contains("de desconto", StringComparison.OrdinalIgnoreCase);

            if (isCouponBefore || isDiscountAfter)
            {
                continue; 
            }

            string rawValue = match.Groups["valor"].Value;
            if (decimal.TryParse(rawValue, NumberStyles.Number, PtBrCulture, out decimal price))
            {
                return price;
            }
        }
        return null;
    }
}