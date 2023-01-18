using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public static class FontManager
    {
        public const int DEFAULT_FONT_SIZE = 64;

        private static Dictionary<string, LoadedFont> _loadedFontNameMap = new Dictionary<string, LoadedFont>();
        public static List<LoadedFont> FontEntries = new List<LoadedFont>();

        public static int CurrGlyphBufferOffset = 0;

        public static void LoadFont(string fontName, int fontSize, bool fontIsLocalPath = false)
        {
            //Ensure our load font operation is being called from the OpenGL render thread
            if (WindowConstants.InMainThread())
            {
                _LoadFont(fontName, fontSize, fontIsLocalPath);
            }
            else
            {
                Window.QueueToRenderCycle(() =>
                {
                    _LoadFont(fontName, fontSize, fontIsLocalPath);
                });
            }
        }

        private static void _LoadFont(string fontName, int fontSize, bool fontIsLocalPath = false)
        {
            string fontFamily = LoadedFont.GetFontFamily(fontName, fontIsLocalPath) + "_" + fontSize;

            if (_loadedFontNameMap.ContainsKey(fontFamily))
                return;

            LoadedFont font = new LoadedFont(fontName, fontSize, fontIsLocalPath);

            _loadedFontNameMap.Add(fontFamily, font);
            FontEntries.Add(font);

            CurrGlyphBufferOffset += font.OffsetIntoSSBO + font.GlyphCount;
        }

        public static void UnloadFont()
        {
            //sync to main thread

            //Move all entries after the loaded font in the glyph SSBO to the left by the font's glyph count
            //Update all of the glyph indexes and loaded font offsets to match their new positions

            //remove font from loaded font list

            throw new NotImplementedException();
        }

        public static LoadedFont GetFont(string fontFamilyName)
        {
            if (_loadedFontNameMap.TryGetValue(fontFamilyName, out var font))
            {
                return font;
            }
            else
            {
                //first font entry is considered the default font
                return FontEntries[0];
            }
        }

        public static bool FontLoaded(string fontFamilyName)
        {
            return _loadedFontNameMap.ContainsKey(fontFamilyName);
        }

        public static void TestPrint()
        {
            LoadedFont font = FontEntries[^1];

            float[] pixels = new float[font.TextureAtlasDimensions.X * font.TextureAtlasDimensions.Y * 4];

            GL.BindTexture(TextureTarget.Texture2D, font.GlyphTextureAtlas);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, pixels);

            Bitmap map = new Bitmap(font.TextureAtlasDimensions.X, font.TextureAtlasDimensions.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var data = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for(int i = 0; i < map.Height; i++)
            {
                for(int j = 0; j < map.Width; j++)
                {
                    unsafe
                    {
                        int pixelIndex = (i * map.Width + j) * 4;

                        byte* currPixel = (byte*)(data.Scan0 + pixelIndex);

                        *currPixel = (byte)(pixels[pixelIndex] * 255);
                        *(currPixel + 1) = (byte)(pixels[pixelIndex + 1] * 255);
                        *(currPixel + 2) = (byte)(pixels[pixelIndex + 2] * 255);
                        *(currPixel + 3) = (byte)(pixels[pixelIndex + 3] * 255);
                    }
                }
            }

            map.UnlockBits(data);

            map.Save("texture_atlas_test.png");
        }
    }
}
