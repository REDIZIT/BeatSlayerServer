using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BeatSlayerServer.Utils
{
    public static class PictureHelper
    {
        /// <summary>
        /// Make image more like square
        /// </summary>
        /// <param name="filepath"></param>
        public static void CutImage(string filepath, string targetfilepath)
        {
            using (Bitmap src = Image.FromFile(filepath) as Bitmap)
            {
                int centerX = (int)(src.Width / 2f);
                int centerY = (int)(src.Height / 2f);
                int min = Math.Min(src.Width, src.Height);

                Rectangle cropRect = new Rectangle(0, 0, min, min);
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }

                target.Save(targetfilepath);
            }
        }
        public static Bitmap CutImage(string filepath)
        {
            using (Bitmap src = Image.FromFile(filepath) as Bitmap)
            {
                int centerX = (int)(src.Width / 2f);
                int centerY = (int)(src.Height / 2f);
                int min = Math.Min(src.Width, src.Height);

                Rectangle cropRect = new Rectangle(centerX - min / 2, centerY - min / 2, min, min);
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }

                return target;
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.Width, image.Height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }


    public enum ImageSize
    {
        _512x512,
        _128x128
    }
}
