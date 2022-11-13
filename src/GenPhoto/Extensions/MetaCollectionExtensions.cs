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

        public static bool TryGetFilePath(this MetaCollection meta, string currentPath, out string path)
        {
            path = "";
            const char DirSeparator = '\\';
            const char FieldSeparator = '-';

            StringBuilder result = new();

            if (meta.Repository is { Length: > 0 } repository)
            {
                result.Append(repository);
                result.Append(DirSeparator);
            }
            else
            {
                return false;
            }

            if (meta.Location is { Length: > 0 } location)
            {
                result.Append(location);
                result.Append(DirSeparator);
            }

            if (meta.Volume is { Length: > 0 } volume)
            {
                result.Append(volume.Replace(":", ""));
            }
            else
            {
                return false;
            }

            if (meta.Year is { Length: > 0 } year)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("år {0}", year);
            }

            if (meta.Image is { Length: > 0 } image)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("bild {0}", image);
            }

            if (meta.Page is { Length: > 0 } page)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("sida {0}", page);
            }

            if (meta.Reference is { Length: > 0 } reference)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.Append(reference);
            }

            result.Append(System.IO.Path.GetExtension(currentPath));

            path = result.ToString();

            return true;
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