using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeneologyImageCollector;

internal class ImageLoader
{
    public ImageSource GetImageSource(string fullPath)
    {
        return new BitmapImage(new Uri(fullPath));
    }

    public ImageSource GetImageSource(Guid id, string fullPath, Size maxSize)
    {
        string extension = Path.GetExtension(fullPath);
        var tempPath = Path.Combine(Path.GetTempPath(), $"{id}_{maxSize.Width}x{maxSize.Height}{extension}");

        if (!File.Exists(tempPath))
        {
            throw new NotImplementedException();
        }

        return new BitmapImage(new Uri(tempPath));
    }
}