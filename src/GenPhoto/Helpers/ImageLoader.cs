using System.Drawing;
using System.IO;

namespace GenPhoto.Helpers;

internal static class ImageHelper
{
    public static Uri GetImageDisplayPath(Guid id, string fullPath, Size maxSize)
    {
        string extension = Path.GetExtension(fullPath);
        var tempPath = Path.Combine(Path.GetTempPath(), $"{id}_{maxSize.Width}x{maxSize.Height}{extension}");

        if (!File.Exists(tempPath))
        {
            ImageProcessor.ResizeImage(fullPath, tempPath, maxSize);
        }

        return new Uri(tempPath);
    }
}