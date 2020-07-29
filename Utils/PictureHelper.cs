using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Utils
{
    /// <summary>
    /// Not used because pitcure is crapping on player side.
    /// Avatar craps to 300x300
    /// Headers doesn't crap
    /// </summary>
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
    }
}
