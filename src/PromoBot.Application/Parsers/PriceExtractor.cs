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
            var start = Math.Max(0, match.Index - 20);
            var beforeContext = text.AsSpan(start, match.Index - start);

            var end = Math.Min(text.Length, match.Index + match.Length + 15);
            var afterContext = text.AsSpan(match.Index + match.Length, end - (match.Index + match.Length));

            bool isDiscountValueBefore = beforeContext.Contains("cupom de", StringComparison.OrdinalIgnoreCase) ||
                                        beforeContext.Contains("desconto de", StringComparison.OrdinalIgnoreCase) ||
                                        beforeContext.Contains("vale de", StringComparison.OrdinalIgnoreCase) ||
                                        beforeContext.TrimEnd().EndsWith("-");

            bool isDiscountValueAfter = afterContext.Contains("de desconto", StringComparison.OrdinalIgnoreCase) ||
                                       afterContext.Contains("off", StringComparison.OrdinalIgnoreCase);

            if (isDiscountValueBefore || isDiscountValueAfter)
            {
                continue; 
            }

            if (decimal.TryParse(match.Groups["valor"].ValueSpan, NumberStyles.Number, PtBrCulture, out decimal price))
            {
                return price;
            }
        }

        return null;
    }
}