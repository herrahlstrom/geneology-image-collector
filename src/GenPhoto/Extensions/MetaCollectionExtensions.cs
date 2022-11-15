using GenPhoto.Models;

namespace GenPhoto.Extensions
{
    internal static class MetaCollectionExtensions
    {
        public static string? GetFilePath(this MetaCollection meta, string currentPath)
        {
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
                return null;
            }

            if (meta.Location is { Length: > 0 } location)
            {
                result.Append(location);
                result.Append(DirSeparator);
            }

            if (meta.Volume is { Length: > 0 } volume)
            {
                result.Append(volume.Replace(":", " "));
            }
            else
            {
                return null;
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

            return result.ToString();
        }
    }
}