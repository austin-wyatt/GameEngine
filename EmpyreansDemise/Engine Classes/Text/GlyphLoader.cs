using Empyrean.Engine_Classes.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes.Text
{
    public class RowEntry
    {
        public int SSBOIndex;
        public int Width;
        public int RowXOffset;

        public CellRow ParentRow;

        public RowEntry(int width, int ssboIndex, int xOffset, CellRow parent)
        {
            Width = width;
            SSBOIndex = ssboIndex;
            ParentRow = parent;
            RowXOffset = xOffset;
        }

        public Vector2i GetAtlasOffset()
        {
            Vector2i offset = new Vector2i();

            offset.X = ParentRow.ParentCell.CellIndex.X * ParentRow.ParentCell.Width + RowXOffset;
            offset.Y = ParentRow.ParentCell.CellIndex.Y * ParentRow.ParentCell.Height + ParentRow.CellYOffset;

            return offset;
        }
    }

    public class CellRow 
    {
        //height should be powers of 2 only
        public readonly int RowHeight;
        public readonly int CellYOffset;

        public List<RowEntry> GlyphEntries = new List<RowEntry>();

        public bool Full = false;

        public Cell ParentCell;

        public int CurrentWidth = 0;

        public CellRow(int height, int yOffset, Cell parentCell)
        {
            RowHeight = height;
            ParentCell = parentCell;
            CellYOffset = yOffset;
        }

        /// <summary>
        /// Attempts to create an entry of the specified width in the row <para/>
        /// If the row cannot accommodate the glyph, the function returns false
        /// and should be considered full
        /// </summary>
        public bool AddEntry(int width, int ssboIndex, out RowEntry entry)
        {
            if(ParentCell.Width - CurrentWidth < width)
            {
                Full = true;
                entry = null;
                return false;
            }

            entry = new RowEntry(width, ssboIndex, CurrentWidth, this);
            GlyphEntries.Add(entry);
            CurrentWidth += width;

            return true;
        }
    }

    public class Cell 
    {
        public int Width = 1024;
        public int Height = 1024;

        public Vector2i CellIndex;

        public List<CellRow> Rows = new List<CellRow>();

        public int CurrentHeight = 0;

        const int MIN_ROW_HEIGHT = 4;

        public bool Full = false;

        public Cell(Vector2i index, int width, int height)
        {
            CellIndex = index;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Checks whether another row could be added to the cell <para/>
        /// If false, the cell should be considered inactive in regards
        /// to adding new glyph rows
        /// </summary>
        /// <returns></returns>
        public bool CheckActive()
        {
            return Height - CurrentHeight <= MIN_ROW_HEIGHT;
        }

        public bool AddRow(int height, out CellRow row)
        {
            if(Height - CurrentHeight >= height)
            {
                row = new CellRow(height, CurrentHeight, this);
                Rows.Add(row);

                CurrentHeight += height;
                return true;
            }

            row = null;
            return false;
        }
    }

    public static class GlyphLoader
    {
        private static Dictionary<int, CellRow> AvailableRows = new Dictionary<int, CellRow>();
        private static List<Cell> Cells = new List<Cell>();
        private static List<Cell> ActiveCells = new List<Cell>();

        private static int GlyphSSBOOffset;
        private static HashSet<int> FreedSSBOIndexes = new HashSet<int>();

        public static Dictionary<FontInfo, Dictionary<int, Glyph>> LoadedGlyphs = new Dictionary<FontInfo, Dictionary<int, Glyph>>();

        public const int SCREEN_DPI = 96;

        static GlyphLoader()
        {
            const int CELL_WIDTH = 1024;
            const int CELL_HEIGHT = 1024;

            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    Cells.Add(new Cell(new Vector2i(j, i), CELL_WIDTH, CELL_HEIGHT));
                    ActiveCells.Add(Cells[^1]);
                }
            }
        }

        private static object _glyphIndexLock = new object();
        /// <summary>
        /// Gets an available glyph index <para/>
        /// </summary>
        /// <returns></returns>
        public static int GetAvailableGlyphIndex()
        {
            lock (_glyphIndexLock)
            {
                if (FreedSSBOIndexes.Count > 0)
                    return FreedSSBOIndexes.First();

                int offset = GlyphSSBOOffset;
                GlyphSSBOOffset++;

                if (offset >= TextRenderer.SUPPORTED_GLYPHS)
                {
                    //a glyph cell should be freed and any current references to glyphs in that cell should be resolved
                    throw new NotImplementedException();
                }

                return offset;
            }
        }

        private static object _getGlyphLock = new object();
        public static Glyph GetGlyph(int character, FontInfo font)
        {
            lock (_getGlyphLock)
            {
                if (LoadedGlyphs.TryGetValue(font, out var dict))
                {
                    if (dict.TryGetValue(character, out var glyph))
                    {
                        return glyph;
                    }
                }

                int ssboIndex = GetAvailableGlyphIndex();

                Glyph newGlyph = new Glyph(character, ssboIndex, default, default, 0, 0, font);

                if (WindowConstants.InMainThread(Thread.CurrentThread))
                {
                    LoadGlyph(newGlyph, font);
                }
                else
                {
                    Window.QueueToRenderCycle(() => LoadGlyph(newGlyph, font));
                }

                return newGlyph;
            }
        }

        public static TextCharacter GetCharacter(int character, FontInfo font)
        {
            TextCharacter val = new TextCharacter(GetGlyph(character, font));

            return val;
        }



        private static float[] _interleavedArray = new float[StaticObjects.QUAD_VERTICES.Length + StaticObjects.TEXTURE_COORDS.Length];
        private static float[] _clonedVertexData = new float[StaticObjects.QUAD_VERTICES.Length];
        private static float[] _clonedTextureData = new float[StaticObjects.TEXTURE_COORDS.Length];
        private static void LoadGlyph(Glyph glyph, FontInfo font)
        {
            int atlasPadding = 2;

            float horizontalOversample = 1;
            float verticalOversample = 1;
            LoadTarget fontLoadTarget = LoadTarget.Normal;

            int lcdExtension = fontLoadTarget == LoadTarget.Lcd ? 3 : 1;

            const int COLORS_COUNT = 4;

            SharpFont.Face face = font.GetFace();
            //face.SetCharSize(font.FontSize * horizontalOversample, font.FontSize * verticalOversample, 0, SCREEN_DPI);

            uint charIndex = face.GetCharIndex((uint)glyph.CharacterValue);
            face.LoadGlyph(charIndex, LoadFlags.AdvanceFlagFastOnly, fontLoadTarget);

            bool kerning = face.HasKerning;

            float bitmapWidth = face.Glyph.Bitmap.Width;
            float bitmapHeight = face.Glyph.Bitmap.Rows;

            int entryWidth = (int)bitmapWidth + atlasPadding * 2;
            int entryHeight = (int)bitmapHeight + atlasPadding * 2;

            RowEntry entry = GetRowEntry(entryWidth, entryHeight, glyph.GlyphSSBOIndex);

            if(entry == null)
            {
                throw new Exception($"Failed to create row entry for glyph {glyph.CharacterValue} of font {font.FullPath}");
            }

            Vector2i atlasOffset = entry.GetAtlasOffset();

            face.LoadGlyph(charIndex, LoadFlags.Render, fontLoadTarget);

            bitmapWidth = face.Glyph.Bitmap.Width;
            bitmapHeight = face.Glyph.Bitmap.Rows;

            glyph.Size = new Vector2(
                bitmapWidth / (horizontalOversample * lcdExtension), 
                bitmapHeight / verticalOversample);
            glyph.Bearing = new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop);
            glyph.Advance = (int)(face.Glyph.Advance.X.ToInt32() / (horizontalOversample));
            glyph.FreeTypeGlyphIndex = charIndex;
            glyph.LineHeight = (float)face.Size.Metrics.Height.ToInt32() / WindowConstants.ClientSize.Y * WindowConstants.ScreenUnits.Y;
            glyph.Descender = (float)face.Size.Metrics.Descender.ToInt32() / WindowConstants.ClientSize.Y * WindowConstants.ScreenUnits.Y;

            if (glyph.CharacterValue == '\n')
            {
                glyph.Render = false;
            }

            Dictionary<int, Glyph> dict;
            if(LoadedGlyphs.TryGetValue(font, out dict))
            {
                dict.TryAdd(glyph.CharacterValue, glyph);
            }
            else
            {
                dict = new Dictionary<int, Glyph>();
                dict.Add(glyph.CharacterValue, glyph);
                LoadedGlyphs.Add(font, dict);
            }

            #region Loading data into texture

            float[] tempTextureBuffer = new float[(int)(bitmapWidth * bitmapHeight * COLORS_COUNT)];

            byte[] glyphBufferData = null;
            int glyphPitch = 0;

            if (bitmapHeight > 0)
            {
                glyphBufferData = face.Glyph.Bitmap.BufferData;
                glyphPitch = face.Glyph.Bitmap.Pitch;
            }

            if (fontLoadTarget == LoadTarget.Lcd)
            {
                for (int i = 0; i < bitmapHeight; i++)
                {
                    for (int j = 0; j < bitmapWidth; j += 3)
                    {
                        int pixelIndex = i * glyphPitch + j;

                        //load the current glyph's color information into the temp color buffer
                        int texAtlasIndex = (int)(bitmapWidth * i + j / lcdExtension) * COLORS_COUNT;

                        tempTextureBuffer[texAtlasIndex] = glyphBufferData[pixelIndex] / 255f;
                        tempTextureBuffer[texAtlasIndex + 1] = glyphBufferData[pixelIndex + 1] / 255f;
                        tempTextureBuffer[texAtlasIndex + 2] = glyphBufferData[pixelIndex + 2] / 255f;
                        tempTextureBuffer[texAtlasIndex + 3] = glyphBufferData[pixelIndex] / 255f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < bitmapHeight; i++)
                {
                    for (int j = 0; j < bitmapWidth; j++)
                    {
                        int pixelIndex = i * glyphPitch + j;

                        //load the current glyph's color information into the temp color buffer
                        int texAtlasIndex = (int)(bitmapWidth * i + j / lcdExtension) * COLORS_COUNT;

                        tempTextureBuffer[texAtlasIndex] = glyphBufferData[pixelIndex] / 255f;
                        tempTextureBuffer[texAtlasIndex + 1] = glyphBufferData[pixelIndex] / 255f;
                        tempTextureBuffer[texAtlasIndex + 2] = glyphBufferData[pixelIndex] / 255f;
                        tempTextureBuffer[texAtlasIndex + 3] = glyphBufferData[pixelIndex] / 255f;
                    }
                }
            }


            #endregion

            #region Loading data into SSBO 
            StaticObjects.QUAD_VERTICES.CopyTo(_clonedVertexData, 0);

            float xProportion = (float)bitmapWidth / (horizontalOversample * lcdExtension) / WindowConstants.ClientSize.X;
            float yProportion = (float)bitmapHeight / verticalOversample / WindowConstants.ClientSize.Y;

            for (int i = 0; i < _clonedVertexData.Length; i += 3)
            {
                _clonedVertexData[i] *= xProportion;
                _clonedVertexData[i + 1] *= yProportion;
            }

            float textureLeftOffset = (float)(atlasOffset.X + atlasPadding) / TextRenderer.ATLAS_WIDTH;
            float textureTopOffset = (float)(atlasOffset.Y + atlasPadding) / TextRenderer.ATLAS_HEIGHT;

            float textureWidth = (float)bitmapWidth / lcdExtension / TextRenderer.ATLAS_WIDTH;
            float textureHeight = (float)bitmapHeight / TextRenderer.ATLAS_HEIGHT;

            #region Set texture coordinates (this case is simple enough to do manually)

            //Top left
            _clonedTextureData[0] = textureLeftOffset; //X
            _clonedTextureData[1] = textureTopOffset; //Y

            //Bottom left
            _clonedTextureData[2] = textureLeftOffset; //X
            _clonedTextureData[3] = textureTopOffset + textureHeight; //Y
            _clonedTextureData[8] = textureLeftOffset; //X
            _clonedTextureData[9] = textureTopOffset + textureHeight; //Y

            //Top right
            _clonedTextureData[4] = textureLeftOffset + textureWidth; //X
            _clonedTextureData[5] = textureTopOffset; //Y
            _clonedTextureData[6] = textureLeftOffset + textureWidth; //X
            _clonedTextureData[7] = textureTopOffset; //Y

            //Bottom right
            _clonedTextureData[10] = textureLeftOffset + textureWidth; //X
            _clonedTextureData[11] = textureTopOffset + textureHeight; //Y
            #endregion

            const int VERTEX_COUNT = 6;
            const int TOTAL_WIDTH = 5;
            const int VERTEX_WIDTH = 3;
            const int TEX_WIDTH = 2;
            const int SSBO_SIZE = TextRenderer.SSBO_ENTRY_SIZE_ENTRIES;

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                _interleavedArray[i * TOTAL_WIDTH] = _clonedVertexData[i * VERTEX_WIDTH];
                _interleavedArray[i * TOTAL_WIDTH + 1] = _clonedVertexData[i * VERTEX_WIDTH + 1];
                _interleavedArray[i * TOTAL_WIDTH + 2] = _clonedVertexData[i * VERTEX_WIDTH + 2];
                _interleavedArray[i * TOTAL_WIDTH + 3] = _clonedTextureData[i * TEX_WIDTH];
                _interleavedArray[i * TOTAL_WIDTH + 4] = _clonedTextureData[i * TEX_WIDTH + 1];
            }

            #endregion


            GL.BindTexture(TextureTarget.Texture2D, TextRenderer.GlyphAtlasHandle);
            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                atlasOffset.X + atlasPadding,
                atlasOffset.Y + atlasPadding,
                (int)bitmapWidth,
                (int)bitmapHeight,
                PixelFormat.Rgba,
                PixelType.Float,
                tempTextureBuffer);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, TextRenderer.GlyphSSBO);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer,
                    new IntPtr(glyph.GlyphSSBOIndex * TextRenderer.SSBO_ENTRY_SIZE_BYTES),
                    _interleavedArray.Length * sizeof(float),
                    _interleavedArray);



            //get the height of the glyph

            //check AvailableRows for the nearest power of 2 higher than the height

            //if the row does not exist, walk through each active cell and attempt to add the row
            //if a row is created in this manner, add it to the AvailableRows dict

            //Once you have a row, attempt to add an entry. If this fails, mark the row as full and
            //repeat the step above to create a new row 

            //Once you have an entry you can generate the glyph and position it in the atlas texture
            //as well as place it in the SSBO
        }

        private static RowEntry GetRowEntry(int entryWidth, int entryHeight, int ssboIndex)
        {
            int nearestHeight = BitOps.NearestPowerOf2((uint)(entryHeight));

            CellRow row = null;

            List<Cell> cellsToRemove = new List<Cell>(5);
            while (row == null)
            {
                if (!AvailableRows.TryGetValue(nearestHeight, out row))
                {
                    for (int i = 0; i < ActiveCells.Count; i++)
                    {
                        if (ActiveCells[i].AddRow(nearestHeight, out row))
                        {
                            AvailableRows.Add(nearestHeight, row);
                            break;
                        }
                        else
                        {
                            cellsToRemove.Add(ActiveCells[i]);
                        }
                    }
                }

                for (int i = cellsToRemove.Count - 1; i >= 0; i--)
                {
                    if (!cellsToRemove[i].CheckActive())
                    {
                        ActiveCells.Remove(cellsToRemove[i]);
                        cellsToRemove[i].Full = true;
                    }
                }

                if (row == null)
                {
                    //delete a cell and create a row there
                    throw new NotImplementedException();
                }

                if (row.AddEntry(entryWidth, ssboIndex, out RowEntry entry))
                {
                    return entry;
                }
                else
                {
                    AvailableRows.Remove(nearestHeight);
                    row = null;
                }
            }

            return null;
        }

        public static void TestPrint()
        {
            float[] pixels = new float[TextRenderer.ATLAS_HEIGHT * TextRenderer.ATLAS_WIDTH * 4];

            GL.BindTexture(TextureTarget.Texture2D, TextRenderer.GlyphAtlasHandle);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, pixels);

            Bitmap map = new Bitmap(TextRenderer.ATLAS_WIDTH, TextRenderer.ATLAS_HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var data = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int i = 0; i < map.Height; i++)
            {
                for (int j = 0; j < map.Width; j++)
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
            map.Dispose();
        }
    }
}
