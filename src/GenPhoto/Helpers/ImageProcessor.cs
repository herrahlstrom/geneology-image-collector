using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace GenPhoto.Helpers;

[SupportedOSPlatform("windows")]
public static class ImageProcessor
{
    public static void ResizeImage(string source, string targetPath, Size targetSize)
    {
        Image img = Image.FromFile(source);

        Size size = GetSize(img.Size, targetSize);

        using var destImage = new Bitmap(size.Width, size.Height);
        destImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

        using (Graphics graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var destRect = new Rectangle(0, 0, size.Width, size.Height);
            var srcRect = new Rectangle(0, 0, img.Width, img.Height);
            graphics.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
        }

        ImageFormat format = Path.GetExtension(targetPath) switch
        {
            ".png" => ImageFormat.Png,
            ".bmp" => ImageFormat.Bmp,
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            _ => throw new NotSupportedException()
        };

        destImage.Save(targetPath, format);
    }

    private static Size GetSize(Size currentSize, Size targetSize)
    {
        if (targetSize.Width >= currentSize.Width && targetSize.Height >= currentSize.Height)
        {
            return currentSize;
        }

        float fWidth = (float)currentSize.Width / targetSize.Width;
        float fHeight = (float)currentSize.Height / targetSize.Height;

        if (fWidth > fHeight)
        {
            return new Size(targetSize.Width, (int)(currentSize.Height / fWidth));
        }

        return new Size((int)(currentSize.Width / fHeight), targetSize.Height);
    }
}