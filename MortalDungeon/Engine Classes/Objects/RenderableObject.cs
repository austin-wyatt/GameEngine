using System;
using System.Collections.Generic;
using System.Text;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Engine_Classes
{
    public enum ObjectRenderType 
    {
        Unknown,
        Color,
        Texture,
        Particle
    }

    public static class Colors 
    {
        public static Vector4 Black = new Vector4(0, 0, 0, 1);
        public static Vector4 White = new Vector4(1, 1, 1, 1);
        public static Vector4 Red = new Vector4(1, 0, 0, 1);
        public static Vector4 Green = new Vector4(0, 1, 0, 1);
        public static Vector4 Blue = new Vector4(0, 0, 1, 1);

        public static Vector4 TranslucentRed = new Vector4(1, 0, 0, 0.5f);
        public static Vector4 TranslucentBlue = new Vector4(0, 0, 1, 0.4f);

        public static Vector4 UILightGray = new Vector4(0.85f, 0.85f, 0.85f, 1);
        public static Vector4 UIDefaultGray = new Vector4(0.5f, 0.5f, 0.5f, 1);
        public static Vector4 UIHoveredGray = new Vector4(0.4f, 0.4f, 0.4f, 1);
        public static Vector4 UISelectedGray = new Vector4(0.3f, 0.3f, 0.3f, 1);

        public static Vector4 Transparent = new Vector4(0, 0, 0, 0);
    }

    //public struct ShaderInfo 
    //{
    //    public ShaderInfo(string vertex, string fragment) 
    //    {
    //        Vertex = vertex;
    //        Fragment = fragment;
    //    }
    //    public string Vertex;
    //    public string Fragment;
    //}

    public struct RotationData
    {
        public float X;
        public float Y;
        public float Z;
    }

    public class RenderableObject
    {
        public float[] Vertices;
        public ObjectRenderType RenderType = ObjectRenderType.Color;
        public uint[] VerticesDrawOrder;
        public int Points;
        public Vector3 Center;

        //when a renderable object is loaded into a scene it's texture needs to be added to the texture list
        public TextureInfo Textures;

        //Every renderable object begins at the origin and is placed from there.
        public Vector4 Position = new Vector4(0, 0, 0, 1.0f);

        public int Stride;

        //Textures and shaders will be loaded separately then assigned to the object
        public Shader ShaderReference;
        public Texture TextureReference;


        //transformations
        public Matrix4 Translation = Matrix4.Identity;
        public Matrix4 Rotation = Matrix4.Identity;
        public Matrix4 Scale = Matrix4.Identity;

        public Matrix4 Transformations = Matrix4.Identity;

        public RotationData RotationInfo = new RotationData() { X = 0, Y = 0, Z = 0 };

        public Vector4 Color = new Vector4();
        public float ColorProportion = 0;

        public bool CameraPerspective = false;

        public ObjectIDs ObjectID = ObjectIDs.Unknown;
        public float SpritesheetPosition = 0;
        public Vector2 SideLengths = new Vector2(1, 1);

        public RenderableObject(float[] vertices, uint[] verticesDrawOrder, int points, TextureInfo textures, Vector4 color, ObjectRenderType renderType, Shader shaderReference, Vector3 center = new Vector3()) 
        {
            Center = center;
            Points = points;
            Vertices = CenterVertices(vertices);
            Textures = textures;
            RenderType = renderType;
            VerticesDrawOrder = verticesDrawOrder;
            ShaderReference = shaderReference;

            Color = color;

            Stride = GetVerticesSize(vertices) / Points;
        }

        public RenderableObject(ObjectDefinition def, TextureInfo textures, Vector4 color, ObjectRenderType renderType, Shader shaderReference)
        {
            Center = def.Center;
            Points = def.Points;
            Textures = textures;
            RenderType = renderType;
            VerticesDrawOrder = def.Indices;
            ShaderReference = shaderReference;
            ObjectID = def.ID;
            SpritesheetPosition = def.SpritesheetPosition;
            SideLengths = def.SideLengths;

            if (def.ShouldCenter())
            {
                Vertices = CenterVertices(def.Vertices);
            }
            else
            {
                Vertices = def.Vertices;
            }

            Color = color;

            Stride = GetVerticesSize(def.Vertices) / Points;
        }

        public RenderableObject(ObjectDefinition def, Vector4 color, ObjectRenderType renderType, Shader shaderReference)
        {
            Center = def.Center;
            Points = def.Points;
            Textures = def.Textures;
            RenderType = renderType;
            VerticesDrawOrder = def.Indices;
            ShaderReference = shaderReference;
            ObjectID = def.ID;
            SpritesheetPosition = def.SpritesheetPosition;
            SideLengths = def.SideLengths;
            if (def.ShouldCenter())
            {
                Vertices = CenterVertices(def.Vertices);
            }
            else
            {
                Vertices = def.Vertices;
            }

            Color = color;

            Stride = GetVerticesSize(def.Vertices) / Points;
        }

        public RenderableObject(RenderableObject oldObj)
        {
            Center = oldObj.Center;
            Points = oldObj.Points;
            Textures = oldObj.Textures;
            RenderType = oldObj.RenderType;
            VerticesDrawOrder = oldObj.VerticesDrawOrder;
            ShaderReference = oldObj.ShaderReference;
            ObjectID = oldObj.ObjectID;
            SpritesheetPosition = oldObj.SpritesheetPosition;
            SideLengths = oldObj.SideLengths;
            Vertices = oldObj.Vertices;

            Color = new Vector4(oldObj.Color);
            ColorProportion = oldObj.ColorProportion;

            Stride = oldObj.Stride;

            Translation = new Matrix4(new Vector4(oldObj.Translation.Row0), new Vector4(oldObj.Translation.Row1), new Vector4(oldObj.Translation.Row2), new Vector4(oldObj.Translation.Row3));
            Rotation = new Matrix4(new Vector4(oldObj.Rotation.Row0), new Vector4(oldObj.Rotation.Row1), new Vector4(oldObj.Rotation.Row2), new Vector4(oldObj.Rotation.Row3));
            Scale = new Matrix4(new Vector4(oldObj.Scale.Row0), new Vector4(oldObj.Scale.Row1), new Vector4(oldObj.Scale.Row2), new Vector4(oldObj.Scale.Row3));

            Position = new Vector4(oldObj.Position);
        }

        public int GetRenderDataOffset(ObjectRenderType renderType = ObjectRenderType.Unknown) 
        {
            if(renderType == ObjectRenderType.Unknown)
            {
                renderType = RenderType;
            }

            switch (renderType) 
            {
                case ObjectRenderType.Color:
                    return 5 * sizeof(float);
                case ObjectRenderType.Texture:
                    return 3 * sizeof(float);
                case ObjectRenderType.Particle:
                    return 5 * sizeof(float);
                default:
                    return 0;
            }
        }

        public int GetVerticesSize()
        {
            return Vertices.Length * sizeof(float);
        }

        public int GetVerticesSize(float[] vertices)
        {
            return vertices.Length * sizeof(float);
        }

        public int GetVerticesDrawOrderSize()
        {
            return VerticesDrawOrder.Length * sizeof(uint);
        }

        public float[] GetPureVertexData()
        {
            int stride = Vertices.Length / Points;
            float[] vertexData = new float[Vertices.Length - 2 * Points];
            int vertexDataStride = (Vertices.Length - 2 * Points) / Points;

            for (int i = 0; i < Points; i++)
            {
                vertexData[i * vertexDataStride] = Vertices[i * stride];
                vertexData[i * vertexDataStride + 1] = Vertices[i * stride + 1];
                vertexData[i * vertexDataStride + 2] = Vertices[i * stride + 2];
            }

            return vertexData;
        }

        public void SetColor(Vector4 color) 
        {
            Color = color;
        }

        //TRANSLATE FUNCTIONS
        public void TranslateX(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += f;
            Position.X = currentTranslation.X;

            SetTranslation(currentTranslation);
        }
        public void TranslateY(float f) 
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Y += f;
            Position.Y = currentTranslation.Y;

            SetTranslation(currentTranslation);
        }
        public void TranslateZ(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Z += f;
            Position.Z = currentTranslation.Z;

            SetTranslation(currentTranslation);
        }
        public void Translate(Vector3 translation)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += translation.X;
            currentTranslation.Y += translation.Y;
            currentTranslation.Z += translation.Z;

            Position.X = currentTranslation.X;
            Position.Y = currentTranslation.Y;
            Position.Z = currentTranslation.Z;

            SetTranslation(currentTranslation);
        }

        //SCALE FUNCTIONS
        public void ScaleAll (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X *= f;
            currentScale.Y *= f;
            currentScale.Z *= f;

            SetScale(currentScale);
        }
        public void SetScaleAll(float f)
        {
            Vector3 currentScale = new Vector3(f, f, f);

            SetScale(currentScale);
        }
        public void ScaleAddition(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X += f;
            currentScale.Y += f;
            currentScale.Z += f;

            SetScale(currentScale);
        }
        public void ScaleX (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[0] *= f;

            SetScale(currentScale);
        }
        public void ScaleY(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[1] *= f;

            SetScale(currentScale);
        }
        public void ScaleZ(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[2] *= f;

            SetScale(currentScale);
        }

        //ROTATE FUNCTIONS
        public void RotateX(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            RotationInfo.X += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        public void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        public void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Z += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }

        //TRANSFORMATION SETTERS
        //public void SetRotation(Vector3 translations)
        //{
        //    Translation = Matrix4.CreateTranslation(translations);
        //}
        public void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);

            CalculateTransformationMatrix();
        }
        public void SetTranslation(Vector3 translations) 
        {
            Translation = Matrix4.CreateTranslation(translations);
            Position = new Vector4(Translation.ExtractTranslation(), Position.W);

            CalculateTransformationMatrix();
        }
        

        //TRANSFORMATION RESETTERS
        public void ResetRotation()
        {
            Rotation = Matrix4.Identity;

            CalculateTransformationMatrix();
        }
        public void ResetScale()
        {
            Scale = Matrix4.Identity;

            CalculateTransformationMatrix();
        }
        public void ResetTranslation()
        {
            Translation = Matrix4.Identity;
            Position = new Vector4(0, 0, 0, Position.W);

            CalculateTransformationMatrix();
        }

        private void CalculateTransformationMatrix() 
        {
            Transformations = Rotation * Scale * Translation;
        }
        
        //Centers the vertices of the renderable object when defined (might want to move this to a different area at some point) TODO, definitely move this into the texture tool
        private float[] CenterVertices(float[] vertices) 
        {
            //vertices will be stored in [x, y, z, textureX, textureY] format
            int stride = vertices.Length / Points;

            float centerX = Center.X;
            float centerY = Center.Y;
            float centerZ = Center.Z;



            for (int i = 0; i < Points; i++) {
                centerX += vertices[i * stride + 0];
                centerY += vertices[i * stride + 1];
                centerZ += vertices[i * stride + 2];
            }

            centerX /= Points;
            centerY /= Points;
            centerZ /= Points;

            for (int i = 0; i < Points; i++)
            {
                vertices[i * stride + 0] -= centerX;
                vertices[i * stride + 1] -= centerY;
                vertices[i * stride + 2] -= centerZ;
            }

            return vertices;
        }
    }
}
