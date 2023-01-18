using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes.TextHandling
{
    public static class TextBuilder
    {
        private static Bitmap ImageBitmap;
        private static Graphics g;

        //public static void Initialize()
        //{
        //    ImageBitmap = new Bitmap(100, 100);

        //    g = Graphics.FromImage(ImageBitmap);
        //    g.SmoothingMode = SmoothingMode.AntiAlias;
        //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //}

        private static int _textureType = -1000000;

        static TextBuilder()
        {
            ImageBitmap = new Bitmap(100, 100);

            g = Graphics.FromImage(ImageBitmap);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextContrast = 4;

            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            ////g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //g.CompositingQuality = CompositingQuality.HighQuality;
        }

        private static object _gLock = new object();
        public static Vector2 DrawString(string text, string fontName, int fontSize, Brush color, Action<Texture> setTexture, Color clearColor, float lineHeightMult = 1)
        {
            if(text == "")
                text = " ";

            text.Replace(' ', (char)127);

            Font font = new Font(fontName, fontSize, FontStyle.Regular);

            

            string[] textArr;

            if (text.Contains("\n"))
            {
                textArr = text.Split("\n");
            }
            else
            {
                textArr = new string[] { text };
            }


            //StringFormat format = new StringFormat();
            StringFormat format = StringFormat.GenericTypographic;
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            

            SizeF dim = new SizeF();

            for(int i = 0; i < textArr.Length; i++)
            {
                lock (_gLock)
                {
                    var temp = g.MeasureString(textArr[i], font, new PointF(0, 0), format);

                    if (i == 0)
                    {
                        dim = temp;
                    }
                    else if (temp.Width > dim.Width)
                    {
                        dim = temp;
                    }
                }
            }

            float dY = dim.Height * lineHeightMult;

            dim.Height *= 1 + ((textArr.Length - 1) * lineHeightMult);

            //dim.Width = (int)dim.Width;
            //dim.Height = (int)dim.Height;

            Bitmap map = new Bitmap((int)dim.Width + 1, (int)dim.Height + 2);

            
            //RectangleF rect = new RectangleF(0, 0, dim.Width, dim.Height);

            Graphics graphics = Graphics.FromImage(map);
            graphics.Clear(clearColor);
            //graphics.Clear(Color.Transparent);
            //graphics.Clear(Color.Black);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //graphics.SmoothingMode = SmoothingMode.HighQuality;

            ////graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            ////graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            ////graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            ////graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            graphics.CompositingQuality = CompositingQuality.HighQuality;

            graphics.TextContrast = 0;

            for (int i = 0; i < textArr.Length; i++)
            {
                if (textArr[i] == "")
                    continue;

                graphics.DrawString(textArr[i], font, color, new PointF(0, i * dY), format);
            }

            BitmapData data = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadWrite, map.PixelFormat);

            byte clearA = clearColor.A;
            byte clearR = clearColor.R;
            byte clearG = clearColor.G;
            byte clearB = clearColor.B;

            int widthHeight = map.Width * map.Height;
            unsafe
            {
                byte* val = (byte*)data.Scan0;

                byte r;
                byte g;
                byte b;
                byte a;

                for (int i = 0; i < widthHeight; i++)
                {
                    r = *(val + 2);
                    g = *(val + 1);
                    b = *(val + 0);
                    a = *(val + 3);

                    if (a == clearA && r == clearR && g == clearG && b == clearB)
                    {
                        *(val + 3) = 0;
                        //*(val + 0) = 255;
                    }

                    //*(val + 2) = 255;

                    val += 4;
                }

            }

            map.UnlockBits(data);

            Texture tex = null;
            //Vector2 dimensions = new Vector2(dim.Width, dim.Height);
            Vector2 dimensions = new Vector2(dim.Width, dim.Height);

            //Stopwatch timer = new Stopwatch();
            //timer.Restart();

            void loadTex()
            {
                //int type = _textureType--;

                try
                {
                    tex = Texture.LoadFromBitmap(map, nearest: true, --_textureType, generateMipMaps: false);
                    setTexture(tex);
                }
                catch { }
            }

            Window.QueueToRenderCycle(loadTex);

            //map.Save("Z://test.png", ImageFormat.Png);

            return dimensions;
        }


    }
}
