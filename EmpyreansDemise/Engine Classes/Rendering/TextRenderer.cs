using Empyrean.Engine_Classes.Text;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public static class TextRenderer
    {
        public static int GlyphSSBO;

        public const int SUPPORTED_GLYPHS = 2000;
        const int VERTEX_COORDS = 6 * 3; //vertex coords per glyph (X, Y, and Z coordinates 6 times)
        const int TEXTURE_COORDS = 6 * 2; //texture coords per glyph (X and Y coordinates 6 times)
        public const int SSBO_ENTRY_SIZE_BYTES = (VERTEX_COORDS + TEXTURE_COORDS) * sizeof(float);

        public static void Initialize()
        {
            GlyphSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, GlyphSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, SUPPORTED_GLYPHS * SSBO_ENTRY_SIZE_BYTES, 
                IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        public static void RenderCharacter(TextCharacter character)
        {
            Shaders.TEXT_SHADER.Use();

            //Set texture atlas for character
            //(when rendering multiple characters, each atlas that is used must be tracked and the uniform passed for the character)
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, character.Glyph.Font.GlyphTextureAtlas);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, GlyphSSBO);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Renderer._generalArrayBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, Renderer._generalArrayBuffer);

            Renderer.InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref character.Transformations, 0);

            Renderer._instancedRenderArray[16] = character.Color.X;
            Renderer._instancedRenderArray[17] = character.Color.Y;
            Renderer._instancedRenderArray[18] = character.Color.Z;
            Renderer._instancedRenderArray[19] = character.Color.W;

            //silently attempt to display the default font in the case of an incorrect glyph index
            Renderer._instancedRenderArray[20] = character.Glyph.GlyphIndex < FontManager.CurrGlyphBufferOffset ?
                character.Glyph.GlyphIndex : character.Glyph.GlyphIndex % FontManager.FontEntries[0].GlyphCount;
            Renderer._instancedRenderArray[21] = 0;

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, 22 * sizeof(float), Renderer._instancedRenderArray);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 1);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, 0);
        }

        private static Dictionary<int, TextureUnit> _usedTextures = new Dictionary<int, TextureUnit>();
        public static void RenderString(RenderBatch renderBatch)
        {
            if (renderBatch.Items.Count == 0)
                return;

            Shaders.TEXT_SHADER.Use();

            GL.Enable(EnableCap.FramebufferSrgb);
            //GL.Disable(EnableCap.Blend);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, GlyphSSBO);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Renderer._generalArrayBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, Renderer._generalArrayBuffer);


            TextureUnit currentTextureUnit = TextureUnit.Texture0;

            int count = 0;

            const int TRANSFORM_DATA_SIZE = 24;

            for (int i = 0; i < renderBatch.Items.Count; i++)
            {
                TextString textString = (TextString)renderBatch.Items[i];

                for(int j = 0; j < textString.Characters.Count; j++)
                {
                    TextCharacter character = textString.Characters[j];

                    if (!character.Glyph.Render)
                        continue;

                    if (!_usedTextures.ContainsKey(character.Glyph.Font.GlyphTextureAtlas))
                    {
                        _usedTextures.Add(character.Glyph.Font.GlyphTextureAtlas, currentTextureUnit);
                        GL.ActiveTexture(currentTextureUnit);
                        GL.BindTexture(TextureTarget.Texture2D, character.Glyph.Font.GlyphTextureAtlas);

                        currentTextureUnit++;
                    }

                    int offset = count * TRANSFORM_DATA_SIZE;

                    Renderer.InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref character.Transformations, offset);

                    Renderer._instancedRenderArray[offset + 16] = character.Color.X;
                    Renderer._instancedRenderArray[offset + 17] = character.Color.Y;
                    Renderer._instancedRenderArray[offset + 18] = character.Color.Z;
                    Renderer._instancedRenderArray[offset + 19] = character.Color.W;

                    //silently attempt to display the default font in the case of an incorrect glyph index
                    Renderer._instancedRenderArray[offset + 20] = character.Glyph.GlyphIndex < FontManager.CurrGlyphBufferOffset ?
                        character.Glyph.GlyphIndex : character.Glyph.GlyphIndex % FontManager.FontEntries[0].GlyphCount;
                    Renderer._instancedRenderArray[offset + 21] = _usedTextures[character.Glyph.Font.GlyphTextureAtlas] - TextureUnit.Texture0;

                    count++;
                }
            }

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer,
                        IntPtr.Zero,
                        count * TRANSFORM_DATA_SIZE * sizeof(float),
                        Renderer._instancedRenderArray);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, count);

            Renderer.ObjectsDrawn += count;
            Renderer.DrawCount++;

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, 0);

            GL.Disable(EnableCap.FramebufferSrgb);
            //GL.Enable(EnableCap.Blend);

            _usedTextures.Clear();
        }
    }
}
