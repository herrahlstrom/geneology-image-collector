using GenPhoto.Shared;
using System.Text.RegularExpressions;

namespace GenPhoto.Parser;

public static class MetaParser
{
    public static IEnumerable<KeyValuePair<string, string>> GetMetaFromPath(string path)
    {
        foreach (var metaItem in GetSpecialCaseFromPath(path))
        {
            yield return metaItem;
        }

        Match rxMatch;

        if (path.StartsWith("Källor\\AD\\"))
        {
            path = path
                .Replace("-O,-P", "-OP")
                .Replace("-N,-O", "-NO");
        }

        // A II a 19
        if ((rxMatch = Regex.Match(path, @"^Källor\\(?<repo>.+\([A-Z]{1,2}\))\\(?<volume>[A-Z](?: I+)?(?: a)? [\d]+(?: c)? \(\d{4}-\d{4}\))")).Success)
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Repository), rxMatch.Groups["repo"].Value);
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Volume), rxMatch.Groups["volume"].Value);
        }
        else if ((rxMatch = Regex.Match(path, @"^Källor\\AD\\(?<repo>(?:.{2,99}\-)+)(?<region>[A-Z]{1,2})\-(?<volume>[A-Z](?:I*a?)\-[\d]+)\-(?<year>\d{4}(?:\-\d{4})?)")).Success)
        {
            string repo = $"{rxMatch.Groups["repo"].Value.Replace("-", " ")} ({rxMatch.Groups["region"].Value})";
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Repository), repo);

            string volume = $"{rxMatch.Groups["volume"].Value.Replace("-", " ")} ({rxMatch.Groups["year"].Value})";
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Volume), volume);
        }

        if ((rxMatch = Regex.Match(path, @"år[- ](?<year>1[6789]\d\d)", RegexOptions.IgnoreCase)).Success)
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Year), rxMatch.Groups["year"].Value);
        }

        if ((rxMatch = Regex.Match(path, @"bild[- ](?<page>\d+)", RegexOptions.IgnoreCase)).Success)
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Image), rxMatch.Groups["image"].Value);
        }

        if ((rxMatch = Regex.Match(path, @"sida[- ](?<page>\d+)", RegexOptions.IgnoreCase)).Success)
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Page), rxMatch.Groups["page"].Value);
        }

        if ((rxMatch = Regex.Match(path, @"[- ](?<ref>[\dAC]\d{7}_\d{5})(?:[^\d]|$)")).Success)
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Reference), rxMatch.Groups["ref"].Value);
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> GetSpecialCaseFromPath(string path)
    {
        if (path.Contains("Statistiska-Centralbyrån", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("SCB", StringComparison.OrdinalIgnoreCase))
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Repository), "Statistiska Centralbyrån (SCB)");
        }

        if (path.Contains("H1AA") && path.Contains("1940-års-folkräkning"))
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Volume), "Folkräkning");
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Year), "1940");
        }

        if (path.Contains("Per Görsta Ahlström - 1946 - Går i land i New York"))
        {
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Location), "New York");
            yield return new KeyValuePair<string, string>(nameof(ImageMetaKey.Year), "1946");
        }
    }
}