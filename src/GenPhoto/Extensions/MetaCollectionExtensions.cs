using GenPhoto.Helpers;
using System.Text.RegularExpressions;

namespace GenPhoto.Extensions
{
    internal static class MetaCollectionExtensions
    {
        private static Regex YearFromVolumeRegEx = new(@"(?:^|[^\d])(?<year>\d\d\d\d)(?:$|[^\d])");

        public static string GetSortKey(this MetaCollection meta)
        {
            string year = meta.GetSortYear();

            return $"{year}-{meta.Repository}-{meta.Volume}";
        }

        private static string GetSortYear(this MetaCollection meta)
        {
            if (meta.Year is { Length: 4 } year)
            {
                return year;
            }
            else if (YearFromVolumeRegEx.Match(meta.Volume ?? "") is { Success: true } yearMatch)
            {
                return yearMatch.Groups["year"].Value;
            }
            else
            {
                return "0000";
            }
        }
    }
}