using System.Text.RegularExpressions;

namespace PromoBot.Application.Parsers;

public static partial class UrlExtractor
{
    
    [GeneratedRegex(@"https?:\/\/[^\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();

    private static readonly char[] CharactersToTrim = ['.', ',', ';', '!', '?', ')', ']', '>'];

    public static string? Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        Match match = UrlPattern().Match(text);

        if (!match.Success)
            return null;

        string cleanedUrl = match.Value.TrimEnd(CharactersToTrim);

        if (Uri.TryCreate(cleanedUrl, UriKind.Absolute, out Uri? uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult.ToString();
        }

        return null;
    }
}