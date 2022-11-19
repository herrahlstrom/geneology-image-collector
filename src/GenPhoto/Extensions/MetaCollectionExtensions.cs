using GenPhoto.Models;
using GenPhoto.Shared;

namespace GenPhoto.Extensions
{
    internal static class MetaCollectionExtensions
    {
        public static string? GetFilePath(this IEnumerable<MetaItemViewModel> meta, string currentPath)
        {
            const char DirSeparator = '\\';
            const char FieldSeparator = '-';

            StringBuilder result = new();

            if (meta.GetValue(ImageMetaKey.Repository) is { Length: > 0 } repository)
            {
                result.Append(repository);
                result.Append(DirSeparator);
            }
            else
            {
                return null;
            }

            if (meta.GetValue(ImageMetaKey.Location) is { Length: > 0 } location)
            {
                result.Append(location);
                result.Append(DirSeparator);
            }

            if (meta.GetValue(ImageMetaKey.Volume) is { Length: > 0 } volume)
            {
                result.Append(volume.Replace(":", " "));
            }
            else
            {
                return null;
            }

            if (meta.GetValue(ImageMetaKey.Year) is { Length: > 0 } year)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("år {0}", year);
            }

            if (meta.GetValue(ImageMetaKey.Image) is { Length: > 0 } image)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("bild {0}", image);
            }

            if (meta.GetValue(ImageMetaKey.Page) is { Length: > 0 } page)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.AppendFormat("sida {0}", page);
            }

            if (meta.GetValue(ImageMetaKey.Reference) is { Length: > 0 } reference)
            {
                result.AppendFormat(" {0} ", FieldSeparator);
                result.Append(reference);
            }

            result.Append(System.IO.Path.GetExtension(currentPath));

            return result.ToString();
        }

        public static string? GetValue(this IEnumerable<MetaItemViewModel> metaItems, string key)
        {
            return metaItems
                .Where(x => x.Key == key)
                .Select(x => x.Value).FirstOrDefault();
        }

        public static string? GetValue(this IEnumerable<MetaItemViewModel> metaItems, ImageMetaKey key)
        {
            return GetValue(metaItems, key.ToString());
        }

        public static bool IsMatch(this IEnumerable<MetaItemViewModel> metaItems, string w)
        {
            return metaItems.Any(x => x.Value.Contains(w, StringComparison.OrdinalIgnoreCase));
        }
    }
}