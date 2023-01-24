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
        public static int GlyphAtlasHandle;

        public const int SUPPORTED_GLYPHS = 2000;
        const int VERTEX_COORDS = 6 * 3; //vertex coords per glyph (X, Y, and Z coordinates 6 times)
        const int TEXTURE_COORDS = 6 * 2; //texture coords per glyph (X and Y coordinates 6 times)
        public const int SSBO_ENTRY_SIZE_BYTES = (VERTEX_COORDS + TEXTURE_COORDS) * sizeof(float);
        public const int SSBO_ENTRY_SIZE_ENTRIES = (VERTEX_COORDS + TEXTURE_COORDS);

        public const int ATLAS_WIDTH = 4096;
        public const int ATLAS_HEIGHT = 4096;

        public static void Initialize()
        {
            //create glyph SSBO
            GlyphSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, GlyphSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, SUPPORTED_GLYPHS * SSBO_ENTRY_SIZE_BYTES, 
                IntPtr.Zero, BufferUsageHint.DynamicDraw);

            //create glyph atlas
            GlyphAtlasHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, GlyphAtlasHandle);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                ATLAS_WIDTH,
                ATLAS_HEIGHT,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
        }

        public static void RenderCharacter(TextCharacter character)
        {
            Shaders.TEXT_SHADER.Use();

            //Set texture atlas for character
            //(when rendering multiple characters, each atlas that is used must be tracked and the uniform passed for the character)
            GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, character.Glyph.Font.GlyphTextureAtlas);
            GL.BindTexture(TextureTarget.Texture2D, GlyphAtlasHandle);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, GlyphSSBO);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Renderer._generalArrayBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, Renderer._generalArrayBuffer);

            Renderer.InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref character.Transformations, 0);

            Renderer._instancedRenderArray[16] = character.Color.X;
            Renderer._instancedRenderArray[17] = character.Color.Y;
            Renderer._instancedRenderArray[18] = character.Color.Z;
            Renderer._instancedRenderArray[19] = character.Color.W;

            //silently attempt to display the default font in the case of an incorrect glyph index
            Renderer._instancedRenderArray[20] = character.Glyph.GlyphSSBOIndex;
            Renderer._instancedRenderArray[21] = 0;

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, 22 * sizeof(float), Renderer._instancedRenderArray);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 1);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, 0);
        }

        public static void RenderTextStrings(List<TextString> strings)
        {
            if (strings.Count == 0)
                return;

            Shaders.TEXT_SHADER.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, GlyphAtlasHandle);

            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, GlyphSSBO);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Renderer._generalArrayBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, Renderer._generalArrayBuffer);

            int count = 0;

            const int TRANSFORM_DATA_SIZE = 24;

            for (int i = 0; i < strings.Count; i++)
            {
                TextString textString = strings[i];

                for(int j = 0; j < textString.Characters.Count; j++)
                {
                    TextCharacter character = textString.Characters[j];

                    if (!character.Glyph.Render)
                        continue;

                    int offset = count * TRANSFORM_DATA_SIZE;

                    Renderer.InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref character.Transformations, offset);

                    Renderer._instancedRenderArray[offset + 16] = character.Color.X;
                    Renderer._instancedRenderArray[offset + 17] = character.Color.Y;
                    Renderer._instancedRenderArray[offset + 18] = character.Color.Z;
                    Renderer._instancedRenderArray[offset + 19] = character.Color.W;
                    Renderer._instancedRenderArray[offset + 20] = character.Glyph.GlyphSSBOIndex;
                    Renderer._instancedRenderArray[offset + 21] = 0;

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

            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);
        }
    }
}
