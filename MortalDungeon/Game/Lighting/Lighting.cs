using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Lighting;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Lighting
{
    public enum LightObstructionType 
    {
        Full = 99,
        None = 0,
        Tree,
        Grate
    }
    public class LightObstruction 
    {
        //public BitArray ObstructedLexels = new BitArray(Lighting.LEXEL_PER_TILE_WIDTH * Lighting.LEXEL_PER_TILE_HEIGHT);
        public LightObstructionType ObstructionType = LightObstructionType.None;
        private Vector2i _position = new Vector2i();
        public Vector2i Position => _position;

        public bool Valid = false;

        public LightObstruction() { }

        public LightObstruction(BaseTile mapPosition) 
        {
            _position = FeatureEquation.PointToMapCoords(mapPosition.TilePoint);
        }

        public LightObstruction(Vector2i mapPosition)
        {
            _position = mapPosition;
        }

        public void SetPosition(BaseTile mapPosition) 
        {
            _position = FeatureEquation.PointToMapCoords(mapPosition.TilePoint);

            Valid = true;
        }
    }

    public class LightGenerator 
    {
        public Vector3 LightColor = new Vector3(1f, 1f, 1f);
        public float Brightness = 0.5f; //initial alpha color
        public float Radius = 1; //how far this light should extend in tiles
        public Vector2i Position = new Vector2i(0, 0); //tilemap position
        public Vector2i LightOffset = new Vector2i(); //how many lexels removed from the tilemap position this generator should be

        public bool On = true;

        public float AlphaFalloff => Brightness / (Radius * Lighting.LEXEL_PER_TILE_WIDTH);
    }

    public class Lighting
    {
        //600x600 (or possibly more precise after proof of concept) Framebuffer texture for saving where light obstructions are using color values

        //600x600 Framebuffer texture for storing actual lighting values. This will, in a circular radius around each light generator, reference
        //the obstruction texture and determine if light can reach any given point. This 500x500 texture will then be rendered onto a quad the
        //size of the loaded tilemaps. This quad should be rendered after all units, structures, and tilemaps but before the UI. The depth buffer
        //should also be cleared before rendering so that all objects regardless of height are affected.

        private static readonly Texture ObstructionSpritesheet = Texture.LoadFromFile("Resources/LightObstructionSheet.png");

        public FrameBufferObject ObstructionMap;
        public FrameBufferObject LightTexture;

        public Scene Scene;

        public bool Initialized = false;

        public Lighting(Scene scene) 
        {
            Scene = scene;
        }

        const int TILE_OFFSET = 30; //also set it in the shaders


        public const int LEXEL_PER_TILE_WIDTH = 32;
        public const int LEXEL_PER_TILE_HEIGHT = 32;

        private static readonly float[] SquareBounds = new float[]
        {
            -1f, -1f, 0.0f,
            -1f, 1f, 0.0f,
            1f, 1f, 0.0f,
            1f, -1f, 0.0f,
        };

        public void InitializeFramebuffers(int loadedTileDimensions = 150) 
        {
            loadedTileDimensions += TILE_OFFSET;

            ObstructionMap = new FrameBufferObject(new Vector2i(loadedTileDimensions * LEXEL_PER_TILE_WIDTH, loadedTileDimensions * LEXEL_PER_TILE_HEIGHT));
            LightTexture = new FrameBufferObject(new Vector2i(loadedTileDimensions * LEXEL_PER_TILE_WIDTH, loadedTileDimensions * LEXEL_PER_TILE_HEIGHT));

            GL.BindTexture(TextureTarget.Texture2D, LightTexture.RenderTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            UpdateObstructionMap(new List<LightObstruction>(), ref Renderer._instancedRenderArray);
            UpdateLightTexture(new List<LightGenerator>(), ref Renderer._instancedRenderArray);

            Initialized = true;
        }

        private const int _obstructionDataOffset = 3;

        /// <summary>
        /// Draw the light obstructions onto the obstruction map
        /// </summary>
        public void UpdateObstructionMap(List<LightObstruction> objects, ref float[] _instancedRenderDataArray) 
        {
            GL.Viewport(0, 0, ObstructionMap.FBODimensions.X, ObstructionMap.FBODimensions.Y);

            GL.Disable(EnableCap.DepthTest);

            Shaders.LIGHT_OBSTRUCTION_SHADER.Use();

            ObstructionSpritesheet.Use(TextureUnit.Texture0);

            ObstructionMap.BindFrameBuffer();

            ObstructionMap.ClearColorBuffer(true);

            EnableObstructionShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedVertexBuffer);


            GL.BufferData(BufferTarget.ArrayBuffer, SquareBounds.Length * sizeof(float), SquareBounds, BufferUsageHint.StaticDraw); //take the raw vertices

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); //vertex data

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedArrayBuffer);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, _obstructionDataOffset, 0 * sizeof(float));  //the local X and Y positions of the LightObstruction
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, _obstructionDataOffset, 2 * sizeof(float));  //spritesheet position



            int currIndex = 0;

            int count = 0;

            Vector2i zeroPoint = Scene._tileMapController.GetTopLeftTilePosition();

            foreach (LightObstruction obstruction in objects)
            {
                Vector2i localizedPoint = obstruction.Position - zeroPoint;


                if (obstruction.Valid)
                {
                    _instancedRenderDataArray[currIndex++] = localizedPoint.X;
                    _instancedRenderDataArray[currIndex++] = localizedPoint.Y;

                    _instancedRenderDataArray[currIndex++] = (float)obstruction.ObstructionType;

                    count++;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * _obstructionDataOffset * sizeof(float), _instancedRenderDataArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count * 4);

            ErrorCode err = GL.GetError();

            if (err != ErrorCode.NoError) 
            {
                throw new Exception();
            }

            FramebufferErrorCode errorTest = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (errorTest != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Error in FrameBuffer: " + errorTest);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            Renderer.DrawCount++;

            DisableObstructionShaderAttributes();

            ObstructionMap.UnbindFrameBuffer();

            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
        }


        private const int _lightDataOffset = 12;


        /// <summary>
        /// Redraw the light texture using the current light obstruction map
        /// </summary>
        public void UpdateLightTexture(List<LightGenerator> generators, ref float[] _instancedRenderDataArray) 
        {
            ClearBuffer(LightTexture, CombatScene.EnvironmentColor.ToVector());

            GL.Viewport(0, 0, LightTexture.FBODimensions.X, LightTexture.FBODimensions.Y);

            //GL.Disable(EnableCap.DepthTest);

            Shaders.LIGHT_SHADER.Use();

            //GL.Disable(EnableCap.Blend);
            //GL.BlendEquation(BlendEquationMode.Min);
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.Min);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ObstructionMap.RenderTexture);


            LightTexture.BindFrameBuffer();

            //if (generators.Count == 0)
            //    return;

            EnableLightShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedVertexBuffer);


            //GL.BufferData(BufferTarget.ArrayBuffer, Circle.Vertices.Length * sizeof(float), Circle.Vertices, BufferUsageHint.StaticDraw); //take the raw vertices
            GL.BufferData(BufferTarget.ArrayBuffer, SquareBounds.Length * sizeof(float), SquareBounds, BufferUsageHint.StaticDraw); //take the raw vertices

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0); //vertex data

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedArrayBuffer);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, _lightDataOffset, 0 * sizeof(float));  //the generator's texel position
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, _lightDataOffset, 2 * sizeof(float));  //the generator's light color and brightness
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, _lightDataOffset, 6 * sizeof(float));  //the generator's alpha falloff
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, _lightDataOffset, 7 * sizeof(float));  //the generator's radius
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, _lightDataOffset, 8 * sizeof(float));  //environment ambient color

            

            int currIndex = 0;

            int count = 0;

            Vector2i zeroPoint = Scene._tileMapController.GetTopLeftTilePosition();

            foreach (LightGenerator generator in generators)
            {
                Vector2i localizedPoint = generator.Position - zeroPoint;
                //Vector2i localizedPoint = zeroPoint - generator.Position;

                localizedPoint *= LEXEL_PER_TILE_WIDTH;

                localizedPoint.X += LEXEL_PER_TILE_WIDTH * TILE_OFFSET / 2; //to deal with the obstructor offset
                localizedPoint.Y += LEXEL_PER_TILE_WIDTH * TILE_OFFSET / 2;

                localizedPoint += generator.LightOffset;

                if (generator.On)
                {
                    _instancedRenderDataArray[currIndex++] = localizedPoint.X;
                    _instancedRenderDataArray[currIndex++] = localizedPoint.Y;

                    _instancedRenderDataArray[currIndex++] = generator.LightColor.X;
                    _instancedRenderDataArray[currIndex++] = generator.LightColor.Y;
                    _instancedRenderDataArray[currIndex++] = generator.LightColor.Z;
                    _instancedRenderDataArray[currIndex++] = generator.Brightness;

                    _instancedRenderDataArray[currIndex++] = generator.AlphaFalloff;

                    _instancedRenderDataArray[currIndex++] = generator.Radius * 2f;

                    _instancedRenderDataArray[currIndex++] = CombatScene.EnvironmentColor.R;
                    _instancedRenderDataArray[currIndex++] = CombatScene.EnvironmentColor.G;
                    _instancedRenderDataArray[currIndex++] = CombatScene.EnvironmentColor.B;
                    _instancedRenderDataArray[currIndex++] = CombatScene.EnvironmentColor.A;

                    count++;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * _lightDataOffset * sizeof(float), _instancedRenderDataArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count * 4);

            ErrorCode err = GL.GetError();

            if (err != ErrorCode.NoError)
            {
                throw new Exception();
            }

            FramebufferErrorCode errorTest = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (errorTest != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Error in FrameBuffer: " + errorTest);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            Renderer.DrawCount++;

            DisableLightShaderAttributes();

            LightTexture.UnbindFrameBuffer();

            GL.GenerateTextureMipmap(LightTexture.RenderTexture);

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);


            GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
        }


        public GameObject CreateTexturedQuad(FrameBufferObject fbo, int texName)
        {
            Texture texture = new Texture(fbo.RenderTexture, texName);

            RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER)
            {
                Material = new Material() { Diffuse = texture }
            };


            obj.Material.Diffuse.TextureId = texName;
            obj.Textures.TextureIds[0] = texName;

            Renderer.LoadTextureFromTextureObj(obj.Material.Diffuse, texName);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = -1,
                Repeats = -1
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());

            GameObject temp = new GameObject();
            temp.AddBaseObject(baseObj);

            temp.BaseObjects[0].BaseFrame.CameraPerspective = true;

            return temp;
        }


        static readonly float[] g_quad_vertex_buffer_data = new float[]{
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            1.0f,  1.0f, 0.0f,
        };

        public void ClearBuffer(FrameBufferObject buffer, Vector4 color) 
        {
            Shaders.COLOR_SHADER.Use();

            GL.Disable(EnableCap.Blend);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribDivisor(0, 0);
            GL.VertexAttribDivisor(1, 1);

            GL.Viewport(0, 0, buffer.FBODimensions.X, buffer.FBODimensions.Y);

            buffer.BindFrameBuffer();
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_vertex_buffer_data.Length * sizeof(float), g_quad_vertex_buffer_data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._instancedArrayBuffer);

            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 4, 0);


            Renderer._instancedRenderArray[0] = color.X;
            Renderer._instancedRenderArray[1] = color.Y;
            Renderer._instancedRenderArray[2] = color.Z;
            Renderer._instancedRenderArray[3] = color.W;

            GL.BufferData(BufferTarget.ArrayBuffer, 4 * sizeof(float), Renderer._instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 1);

            Renderer.DrawCount++;

            buffer.UnbindFrameBuffer();

            GL.VertexAttribDivisor(1, 0);

            GL.Enable(EnableCap.Blend);

            GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
        }

        private static void EnableObstructionShaderAttributes()
        {
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribDivisor(1, 1);
            GL.VertexAttribDivisor(2, 1);
        }

        private static void DisableObstructionShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.VertexAttribDivisor(1, 0);
            GL.VertexAttribDivisor(2, 0);
        }

        private static void EnableLightShaderAttributes()
        {
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.VertexAttribDivisor(1, 1);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
        }

        private static void DisableLightShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.VertexAttribDivisor(1, 0);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
        }
    }
}
