using Empyrean.Engine_Classes.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class LoadedFont
    {
        private static Library _library;

        public const int SCREEN_DPI = 96;
        //public const int SCREEN_DPI = 72;

        public string FamilyName;

        private Dictionary<int, Glyph> GlyphCharacterMap = new Dictionary<int, Glyph>();

        public int GlyphTextureAtlas;
        public Vector2i TextureAtlasDimensions;

        public int OffsetIntoSSBO;
        public int GlyphCount;

        public int LineHeight { get => TextureAtlasDimensions.Y; }

        static LoadedFont()
        {
            _library = new Library();
        }

        public LoadedFont(string fontName, int fontSize, bool fontIsLocalPath = false)
        {
            string fontPath = GetFullFontPath(fontName, fontIsLocalPath);

            CreateTextureAtlas(fontPath, fontSize);
        }

        ~LoadedFont()
        {
            DeleteTextureAtlas();
        }

        public TextCharacter GetCharacter(int character)
        {
            TextCharacter val = new TextCharacter(GlyphCharacterMap[character]);

            return val;
        }

        public Glyph GetGlyph(int character)
        {
            return GlyphCharacterMap[character];
        }

        public static string GetFontFamily(string fontName, bool fontIsLocalPath = false)
        {
            string path = GetFullFontPath(fontName, fontIsLocalPath);

            try
            {
                SharpFont.Face face = new SharpFont.Face(_library, path);
                return face.FamilyName;
            }
            catch(Exception _)
            {
                return "";
            }
        }

        private static string GetFullFontPath(string fontName, bool fontIsLocalPath = false)
        {
            string baseFontPath;

            if (fontIsLocalPath)
            {
                baseFontPath = @".\";
            }
            else
            {
                switch (WindowConstants.CurrentOS)
                {
                    case OSType.OSX:
                        baseFontPath = @"System\Library\Fonts\";
                        break;
                    case OSType.Linux:
                        baseFontPath = @"System\Library\Fonts\";
                        break;
                    case OSType.Windows:
                    default:
                        baseFontPath = @"C:\Windows\Fonts\";
                        break;
                }
            }

            return baseFontPath + fontName;
        }

        private void CreateTextureAtlas(string fontPath, int fontSize)
        {
            OffsetIntoSSBO = FontManager.CurrGlyphBufferOffset;

            GlyphTextureAtlas = GL.GenTexture();

            //space between the glyphs on the texture atlas to avoid texture bleeding
            int atlasPadding = 2;

            float horizontalOversample = 3;
            float verticalOversample = 1;

            SharpFont.Face face = new SharpFont.Face(_library, fontPath);
            face.SetCharSize(fontSize * horizontalOversample, fontSize * verticalOversample, 0, SCREEN_DPI);

            FamilyName = face.FamilyName;

            int atlasWidth = 0;
            int atlasHeight = 0;

            int maxWidth = 0;


            //TODO, add support for different character sets beyond ASCII
            const int CHARACTER_SET_WIDTH = 128 - 32;
            GlyphCount = CHARACTER_SET_WIDTH;

            List<int> characterCodes = new List<int>();

            uint[] specialCharacters = new uint[]
            {
                '\n',
                '\t',
                '†',
                '‡',
                '•',
                '˜',
                '™',
                '¡',
                '¢',
                '¦',
                '©',
                '®',
                '±',
                '°',
                '¯',
                '¶',
                '»',
                '¿',
                '÷',
                'ø',
            };

            for (int i = 32; i < 128; i++)
            {
                characterCodes.Add(i);

                uint charIndex = face.GetCharIndex((char)i);
                face.LoadGlyph(charIndex, LoadFlags.Render, LoadTarget.Normal);

                maxWidth = maxWidth > face.Glyph.Bitmap.Width ? maxWidth : face.Glyph.Bitmap.Width;
                atlasWidth += face.Glyph.Bitmap.Width;
                atlasHeight = atlasHeight > face.Glyph.Bitmap.Rows ? atlasHeight : face.Glyph.Bitmap.Rows;

                atlasWidth += atlasPadding;
            }

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                characterCodes.Add((int)specialCharacters[i]);
                uint charIndex = face.GetCharIndex(specialCharacters[i]);
                face.LoadGlyph(charIndex, LoadFlags.Render, LoadTarget.Normal);

                maxWidth = maxWidth > face.Glyph.Bitmap.Width ? maxWidth : face.Glyph.Bitmap.Width;
                atlasWidth += face.Glyph.Bitmap.Width;
                atlasHeight = atlasHeight > face.Glyph.Bitmap.Rows ? atlasHeight : face.Glyph.Bitmap.Rows;

                atlasWidth += atlasPadding;
            }

            TextureAtlasDimensions = new Vector2i(atlasWidth, atlasHeight);

            GL.BindTexture(TextureTarget.Texture2D, GlyphTextureAtlas);
            GL.TexImage2D(TextureTarget.Texture2D, 
                0, 
                PixelInternalFormat.Rgba, 
                atlasWidth, 
                atlasHeight, 
                0, 
                PixelFormat.Rgba, 
                PixelType.Float, 
                IntPtr.Zero);


            float[] clonedVertexData = new float[StaticObjects.QUAD_VERTICES.Length];
            float[] clonedTextureData = new float[StaticObjects.TEXTURE_COORDS.Length];

            const int COLORS_COUNT = 4;
            float[] tempGlyphBuffer = new float[maxWidth * atlasHeight * COLORS_COUNT];

            int currWidth = 0;

            int glyphIndex = OffsetIntoSSBO;

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TextRenderer.GlyphSSBO);

            float[] interleavedArray = new float[clonedVertexData.Length + clonedTextureData.Length];

            for (int g = 0; g < characterCodes.Count; g++)
            {
                uint charIndex = face.GetCharIndex((char)characterCodes[g]);
                face.LoadGlyph(charIndex, LoadFlags.Render, LoadTarget.Normal);

                int bitmapWidth = face.Glyph.Bitmap.Width;
                int bitmapHeight = face.Glyph.Bitmap.Rows;

                Glyph glyph = new Glyph(characterCodes[g],
                    glyphIndex,
                    new Vector2(
                        face.Glyph.Bitmap.Width / horizontalOversample, face.Glyph.Bitmap.Rows / verticalOversample),
                    new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop / verticalOversample),
                    (int)(face.Glyph.Advance.X.ToInt32() / horizontalOversample),
                    this);

                if(glyph.CharacterValue == '\n')
                {
                    glyph.Render = false;
                }

                GlyphCharacterMap.Add(characterCodes[g], glyph);

                #region Loading data into texture
                for (int i = 0; i < bitmapHeight; i++)
                {
                    for (int j = 0; j < bitmapWidth; j++)
                    {
                        int pixelIndex = (i * bitmapWidth + j) * COLORS_COUNT;
                        float pixelValue = (float)face.Glyph.Bitmap.BufferData[i * bitmapWidth + j] / 255;

                        //load the current glyph's color information into the temp color buffer
                        tempGlyphBuffer[pixelIndex] = pixelValue;
                        tempGlyphBuffer[pixelIndex + 1] = pixelValue;
                        tempGlyphBuffer[pixelIndex + 2] = pixelValue;
                        tempGlyphBuffer[pixelIndex + 3] = pixelValue;
                    }
                }
                
                //insert tempGlyphBuffer into the glyph texture atlas
                GL.TexSubImage2D(TextureTarget.Texture2D, 
                    0, 
                    currWidth, 
                    0, 
                    bitmapWidth, 
                    bitmapHeight, 
                    PixelFormat.Bgra, 
                    PixelType.Float, 
                    tempGlyphBuffer);
                #endregion

                #region Loading data into SSBO 

                StaticObjects.QUAD_VERTICES.CopyTo(clonedVertexData, 0);

                float xProportion = (float)bitmapWidth / horizontalOversample / WindowConstants.ClientSize.X;
                float yProportion = (float)bitmapHeight / verticalOversample / WindowConstants.ClientSize.Y;

                for (int i = 0; i < clonedVertexData.Length; i += 3)
                {
                    clonedVertexData[i] *= xProportion;
                    clonedVertexData[i + 1] *= yProportion;
                }

                float textureLeftOffset = (float)currWidth / atlasWidth;
                
                float textureWidth = (float)bitmapWidth / atlasWidth;
                float textureHeight = (float)bitmapHeight / atlasHeight;

                #region Set texture coordinates (this case is simple enough to do manually)

                //Top left
                clonedTextureData[0] = textureLeftOffset; //X
                clonedTextureData[1] = 0; //Y

                //Bottom left
                clonedTextureData[2] = textureLeftOffset; //X
                clonedTextureData[3] = textureHeight; //Y
                clonedTextureData[8] = textureLeftOffset; //X
                clonedTextureData[9] = textureHeight; //Y

                //Top right
                clonedTextureData[4] = textureLeftOffset + textureWidth; //X
                clonedTextureData[5] = 0; //Y
                clonedTextureData[6] = textureLeftOffset + textureWidth; //X
                clonedTextureData[7] = 0; //Y

                //Bottom right
                clonedTextureData[10] = textureLeftOffset + textureWidth; //X
                clonedTextureData[11] = textureHeight; //Y
                #endregion

                const int VERTEX_COUNT = 6;
                const int TOTAL_WIDTH = 5;
                const int VERTEX_WIDTH = 3;
                const int TEX_WIDTH = 2;
                for (int i = 0; i < VERTEX_COUNT; i++)
                {
                    interleavedArray[i * TOTAL_WIDTH] = clonedVertexData[i * VERTEX_WIDTH];
                    interleavedArray[i * TOTAL_WIDTH + 1] = clonedVertexData[i * VERTEX_WIDTH + 1];
                    interleavedArray[i * TOTAL_WIDTH + 2] = clonedVertexData[i * VERTEX_WIDTH + 2];
                    interleavedArray[i * TOTAL_WIDTH + 3] = clonedTextureData[i * TEX_WIDTH];
                    interleavedArray[i * TOTAL_WIDTH + 4] = clonedTextureData[i * TEX_WIDTH + 1];
                }

                GL.BufferSubData(BufferTarget.ShaderStorageBuffer,
                    new IntPtr(glyphIndex * TextRenderer.SSBO_ENTRY_SIZE_BYTES),
                    interleavedArray.Length * sizeof(float),
                    interleavedArray);
                #endregion

                currWidth += face.Glyph.Bitmap.Width;
                currWidth += atlasPadding;
                glyphIndex++;
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        }

        private void DeleteTextureAtlas()
        {
            if (GlyphTextureAtlas == 0)
                return;

            if(WindowConstants.InMainThread())
            {
                GL.DeleteTexture(GlyphTextureAtlas);
                GlyphTextureAtlas = 0;
            }
            else
            {
                Window.QueueToRenderCycle(() =>
                {
                    GL.DeleteTexture(GlyphTextureAtlas);
                    GlyphTextureAtlas = 0;
                });
            }
        }
    }
}
