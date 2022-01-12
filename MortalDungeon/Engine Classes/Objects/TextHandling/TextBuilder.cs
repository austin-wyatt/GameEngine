﻿using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;

namespace MortalDungeon.Engine_Classes.TextHandling
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }


        public static (Texture, Vector2 dimensions) DrawString(string text, string fontName, int fontSize, Brush color)
        {
            if(text == "")
                text = " ";

            text.Replace(' ', (char)127);

            Font font = new Font(fontName, fontSize);

            string[] textArr;

            if (text.Contains("\n"))
            {
                textArr = text.Split("\n");
            }
            else
            {
                textArr = new string[] { text };
            }


            StringFormat format = new StringFormat();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            SizeF dim = new SizeF();

            for(int i = 0; i < textArr.Length; i++)
            {
                var temp = g.MeasureString(textArr[i], font, new PointF(0, 0), format);

                if (i == 0)
                {
                    dim = temp;
                }
                else if(temp.Width > dim.Width)
                {
                    dim = temp;
                }
            }

            float dY = dim.Height;

            dim.Height *= textArr.Length;

            Bitmap map = new Bitmap((int)dim.Width, (int)dim.Height);

            //RectangleF rect = new RectangleF(0, 0, dim.Width, dim.Height);

            Graphics graphics = Graphics.FromImage(map);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;


            for (int i = 0; i < textArr.Length; i++)
            {
                if (textArr[i] == "")
                    continue;

                graphics.DrawString(textArr[i], font, color, new PointF(0, i * dY), format);
            }


            if (Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                return (Texture.LoadFromBitmap(map, false, _textureType--, generateMipMaps: false), new Vector2(dim.Width, dim.Height));
            }
            else
            {
                Texture tex = null;
                Vector2 dimensions = new Vector2(dim.Width, dim.Height);
                bool taskCompleted = false;

                Stopwatch timer = new Stopwatch();
                timer.Restart();

                void loadTex()
                {
                    Window.RenderEnd -= loadTex;
                    try
                    {
                        tex = Texture.LoadFromBitmap(map, false, _textureType--, generateMipMaps: false);
                    }
                    finally
                    {
                        taskCompleted = true;
                    }
                }

                Window.RenderEnd += loadTex;

                while(timer.ElapsedMilliseconds < 10000 && !taskCompleted)
                {

                }

                return (tex, dimensions);
            }

        }


    }
}
