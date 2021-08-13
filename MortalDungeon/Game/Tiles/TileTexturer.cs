using MortalDungeon.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using System.Linq;

namespace MortalDungeon.Game.Tiles
{
    public static class TileTexturer
    {
        private const int tile_width = 62; //individual tile width
        private const int tile_width_partial = 46; //stacked width
        private const int tile_height = 54; //individual tile height
        private const int tile_height_partial = 27; //stacked height

        private static Texture TileSpritesheet = Texture.LoadFromFile("Resources/TileSpritesheet.png");

        private static Random random = new Random();

        public static void InitializeTexture(TileMap map)
        {
            if (map.FrameBuffer != null) 
            {
                map.FrameBuffer.Dispose();
            }

            int textureScale = 2; //how much we are scaling up the base tile textures to make it look good
            map.FrameBuffer = new FrameBufferObject(new Vector2i((int)((tile_width + (map.Width) * tile_width_partial) / WindowConstants.AspectRatio) * textureScale,
                (tile_height * map.Height + tile_height) * textureScale));

            map.DynamicTexture = new Texture(map.FrameBuffer.RenderTexture, TextureName.DynamicTexture);

            map.Tiles.ForEach(tile => map.TilesToUpdate.Add(tile));
            RenderTilesToFramebuffer(map);
        }

        public static void UpdateTexture(TileMap map) 
        {
            RenderTilesToFramebuffer(map);
        }

        private static void RenderTilesToFramebuffer(TileMap map) 
        {
            map.FrameBuffer.BindFrameBuffer();

            GL.Viewport(0, 0, map.FrameBuffer.FBODimensions.X, map.FrameBuffer.FBODimensions.Y);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.Disable(EnableCap.DepthTest);


            HashSet<BaseTile> outlineTiles = new HashSet<BaseTile>();
            HashSet<BaseTile> nonOutlineTiles = new HashSet<BaseTile>();

            foreach (BaseTile tile in map.TilesToUpdate)
            {
                if (tile.Outline)
                    outlineTiles.Add(tile);
                else
                    nonOutlineTiles.Add(tile);
            }

            RenderTiles(nonOutlineTiles, ref Renderer._instancedRenderArray, map);
            RenderTiles(outlineTiles, ref Renderer._instancedRenderArray, map);

            //RenderTiles(map.TilesToUpdate, ref Renderer._instancedRenderArray, map);

            map.FrameBuffer.UnbindFrameBuffer();
            GL.Enable(EnableCap.DepthTest);


            GL.BindTexture(TextureTarget.Texture2D, map.FrameBuffer.RenderTexture);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
        }


        private const int _dataOffset = 20;
        private static void RenderTiles(HashSet<BaseTile> objects, ref float[] _instancedRenderDataArray, TileMap map)
        {
            if (objects.Count == 0)
                return;

            Shaders.TILE_MAP_SHADER.Use();

            RenderableObject Display = objects.First().BaseObjects[0]._currentAnimation.CurrentFrame;

            TileSpritesheet.Use(TextureUnit.Texture0);
            EnableInstancedShaderAttributes();


            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedVertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw); //take the raw vertices


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, _dataOffset, 0 * sizeof(float));  //color that will be applied to the primary texture
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, _dataOffset, 4 * sizeof(float));  //spritesheet position [0], second texture position [1], 
                                                                                                                 //mix percent [2], outline [3] (0 or 1)
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, _dataOffset, 8 * sizeof(float));  //color that will be applied to the outline
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, _dataOffset, 12 * sizeof(float)); //X position of tile [0], Y position of tile[1], map width [2] and height [3]
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, _dataOffset, 16 * sizeof(float)); //width of the FBO render texture [0], height of the FBO render texture [1], empty [2, 3]
            int currIndex = 0;

            int count = 0;

            float mixPercent;
            int overlayPosition = 0;

            foreach (BaseTile tile in objects) 
            {
                if (tile.Render)
                {
                    if (tile.InFog)
                    {
                        int num = random.Next() % 4;
                        TileType fogType;

                        switch (num)
                        {
                            default:
                                fogType = TileType.Fog_1;
                                break;
                            case 1:
                                fogType = TileType.Fog_2;
                                break;
                            case 2:
                                fogType = TileType.Fog_3;
                                break;
                            case 3:
                                fogType = TileType.Fog_4;
                                break;
                        }

                        overlayPosition = (int)fogType;

                        mixPercent = tile.InFog && map.Controller.Scene.CurrentUnit != null && tile.Explored[map.Controller.Scene.CurrentUnit.Team] ? 0.5f : 1;
                    }
                    else 
                    {
                        mixPercent = 0;
                    }

                    for (int j = 0; j < tile.BaseObjects.Count; j++)
                    {
                        if (tile.BaseObjects[j].Render)
                        {
                            _instancedRenderDataArray[currIndex++] = tile.Color.X;
                            _instancedRenderDataArray[currIndex++] = tile.Color.Y;
                            _instancedRenderDataArray[currIndex++] = tile.Color.Z;
                            _instancedRenderDataArray[currIndex++] = tile.Color.W;

                            _instancedRenderDataArray[currIndex++] = (int)tile.Properties.Type;
                            _instancedRenderDataArray[currIndex++] = overlayPosition;
                            _instancedRenderDataArray[currIndex++] = mixPercent;
                            _instancedRenderDataArray[currIndex++] = tile.Outline ? 1 : 0;

                            _instancedRenderDataArray[currIndex++] = tile.OutlineColor.X;
                            _instancedRenderDataArray[currIndex++] = tile.OutlineColor.Y;
                            _instancedRenderDataArray[currIndex++] = tile.OutlineColor.Z;
                            _instancedRenderDataArray[currIndex++] = tile.OutlineColor.W;

                            _instancedRenderDataArray[currIndex++] = tile.TilePoint.X;
                            _instancedRenderDataArray[currIndex++] = tile.TilePoint.Y;
                            _instancedRenderDataArray[currIndex++] = 0;
                            _instancedRenderDataArray[currIndex++] = 0;

                            _instancedRenderDataArray[currIndex++] = map.FrameBuffer.FBODimensions.X;
                            _instancedRenderDataArray[currIndex++] = map.FrameBuffer.FBODimensions.Y;
                            _instancedRenderDataArray[currIndex++] = WindowConstants.ClientSize.X;
                            _instancedRenderDataArray[currIndex++] = WindowConstants.ClientSize.Y;

                            count++;
                        }
                    }
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * _dataOffset * sizeof(float), _instancedRenderDataArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count * 4);


            FramebufferErrorCode errorTest = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (errorTest != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Error in RenderFrameBuffer: " + errorTest);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            Renderer.DrawCount++;

            DisableInstancedShaderAttributes();
        }


        private static void EnableInstancedShaderAttributes()
        {
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
        }

        private static void DisableInstancedShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
        }
    }
}
