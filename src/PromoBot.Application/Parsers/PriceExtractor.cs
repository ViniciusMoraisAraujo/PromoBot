using System.Globalization;
using System.Text.RegularExpressions;

namespace PromoBot.Application.Parsers;

public static partial class PriceExtractor
{
    [GeneratedRegex(@"(?:R\$\s?)(?<valor>(?:\d{1,3}(?:\.\d{3})*|\d+)(?:,\d{2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex PricePattern();

    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static decimal? Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = PricePattern().Match(text);

        if (!match.Success)
            return null;

        string rawValue = match.Groups["valor"].Value;

        if (decimal.TryParse(rawValue, NumberStyles.Number, PtBrCulture, out decimal price))
        {
            return price;
        }

        return null;
    }
}