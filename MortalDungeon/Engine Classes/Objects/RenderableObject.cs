using System;
using System.Collections.Generic;
using System.Text;
using MortalDungeon.Engine_Classes.Lighting;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Engine_Classes
{
    internal enum ObjectRenderType 
    {
        Unknown,
        Color,
        Texture,
        Particle
    }

    internal static class Colors 
    {
        internal static Vector4 Black = new Vector4(0, 0, 0, 1);
        internal static Vector4 White = new Vector4(1, 1, 1, 1);
        internal static Vector4 Red = new Vector4(1, 0, 0, 1);
        internal static Vector4 Green = new Vector4(0, 1, 0, 1);
        internal static Vector4 Blue = new Vector4(0, 0, 1, 1);
        internal static Vector4 Tan = new Vector4(0.68f, 0.66f, 0.48f, 1);

        internal static Vector4 TranslucentRed = new Vector4(1, 0, 0, 0.5f);
        internal static Vector4 MoreTranslucentRed = new Vector4(1, 0, 0, 0.35f);
        internal static Vector4 TranslucentBlue = new Vector4(0, 0, 1, 0.4f);
        internal static Vector4 MoreTranslucentBlue = new Vector4(0, 0, 1, 0.35f);

        internal static Vector4 LessAggressiveRed = new Vector4(0.62f, 0.18f, 0.18f, 1);

        internal static Vector4 UILightGray = new Vector4(0.85f, 0.85f, 0.85f, 1);
        internal static Vector4 UIDefaultGray = new Vector4(0.5f, 0.5f, 0.5f, 1);
        internal static Vector4 UIHoveredGray = new Vector4(0.4f, 0.4f, 0.4f, 1);
        internal static Vector4 UISelectedGray = new Vector4(0.3f, 0.3f, 0.3f, 1);
        internal static Vector4 UIDisabledGray = new Vector4(0.71f, 0.71f, 0.71f, 1);
        internal static Vector4 UITextBlack = new Vector4(0.1f, 0.1f, 0.1f, 1);

        internal static Vector4 IconHover = new Vector4(0.85f, 0.85f, 0.85f, 1);
        internal static Vector4 IconSelected = new Vector4(0.78f, 0.78f, 0.78f, 1);
        internal static Vector4 IconDisabled = new Vector4(0.7f, 0.7f, 0.7f, 1);

        internal static Vector4 Transparent = new Vector4(0, 0, 0, 0);
    }

    //internal struct ShaderInfo 
    //{
    //    internal ShaderInfo(string vertex, string fragment) 
    //    {
    //        Vertex = vertex;
    //        Fragment = fragment;
    //    }
    //    internal string Vertex;
    //    internal string Fragment;
    //}

    internal struct RotationData
    {
        internal float X;
        internal float Y;
        internal float Z;
    }

    internal class RenderableObject
    {
        internal float[] Vertices;
        internal ObjectRenderType RenderType = ObjectRenderType.Color;
        internal uint[] VerticesDrawOrder;
        internal int Points;
        internal Vector3 Center;

        //when a renderable object is loaded into a scene it's texture needs to be added to the texture list
        internal TextureInfo Textures;

        //Every renderable object begins at the origin and is placed from there.
        internal Vector4 Position = new Vector4(0, 0, 0, 1.0f);

        internal int Stride;

        //Textures and shaders will be loaded separately then assigned to the object
        internal Shader ShaderReference;
        internal Material Material = new Material();


        //transformations
        internal Matrix4 Translation = Matrix4.Identity;
        internal Matrix4 Rotation = Matrix4.Identity;
        internal Matrix4 Scale = Matrix4.Identity;

        internal Matrix4 Transformations = Matrix4.Identity;

        internal RotationData RotationInfo = new RotationData() { X = 0, Y = 0, Z = 0 };

        internal Vector4 BaseColor = new Vector4();
        internal Vector4 InterpolatedColor = new Vector4();
        internal float ColorProportion = 0;

        internal List<Color> AppliedColors = new List<Color>(); 

        internal bool CameraPerspective = false;

        internal float SpritesheetPosition = 0;
        internal Vector2 SideLengths = new Vector2(1, 1);

        internal float VerticeType = 0;

        internal RenderableObject(float[] vertices, uint[] verticesDrawOrder, int points, TextureInfo textures, Vector4 color, ObjectRenderType renderType, Shader shaderReference, Vector3 center = new Vector3()) 
        {
            Center = center;
            Points = points;
            Vertices = CenterVertices(vertices);
            Textures = textures;
            RenderType = renderType;
            VerticesDrawOrder = verticesDrawOrder;
            ShaderReference = shaderReference;

            SetBaseColor(color);

            Stride = GetVerticesSize(vertices) / Points;
        }

        internal RenderableObject(ObjectDefinition def, Vector4 color, ObjectRenderType renderType, Shader shaderReference)
        {
            Center = def.Center;
            Points = def.Points;
            Textures = def.Textures;
            RenderType = renderType;
            VerticesDrawOrder = def.Indices;
            ShaderReference = shaderReference;
            SpritesheetPosition = def.SpritesheetPosition;
            SideLengths = def.SideLengths;

            VerticeType = def.VerticeType;

            if (def.ShouldCenter())
            {
                Vertices = CenterVertices(def.Vertices);
            }
            else
            {
                Vertices = def.Vertices;
            }

            SetBaseColor(color);

            Stride = GetVerticesSize(def.Vertices) / Points;
        }

        internal RenderableObject(RenderableObject oldObj)
        {
            Center = oldObj.Center;
            Points = oldObj.Points;
            Textures = oldObj.Textures;
            Material = new Material(oldObj.Material);
            RenderType = oldObj.RenderType;
            VerticesDrawOrder = oldObj.VerticesDrawOrder;
            ShaderReference = oldObj.ShaderReference;
            SpritesheetPosition = oldObj.SpritesheetPosition;
            SideLengths = oldObj.SideLengths;
            Vertices = oldObj.Vertices;

            VerticeType = oldObj.VerticeType;

            SetBaseColor(new Vector4(oldObj.BaseColor));

            ColorProportion = oldObj.ColorProportion;

            Stride = oldObj.Stride;

            Translation = new Matrix4(new Vector4(oldObj.Translation.Row0), new Vector4(oldObj.Translation.Row1), new Vector4(oldObj.Translation.Row2), new Vector4(oldObj.Translation.Row3));
            Rotation = new Matrix4(new Vector4(oldObj.Rotation.Row0), new Vector4(oldObj.Rotation.Row1), new Vector4(oldObj.Rotation.Row2), new Vector4(oldObj.Rotation.Row3));
            Scale = new Matrix4(new Vector4(oldObj.Scale.Row0), new Vector4(oldObj.Scale.Row1), new Vector4(oldObj.Scale.Row2), new Vector4(oldObj.Scale.Row3));

            Position = new Vector4(oldObj.Position);
        }

        internal RenderableObject(ObjectDefinition def, Vector4 color, Shader shaderReference)
        {
            Center = def.Center;
            Points = def.Points;
            Textures = def.Textures;
            VerticesDrawOrder = def.Indices;
            ShaderReference = shaderReference;
            SpritesheetPosition = def.SpritesheetPosition;
            SideLengths = def.SideLengths;

            VerticeType = def.VerticeType;

            if (def.ShouldCenter())
            {
                Vertices = CenterVertices(def.Vertices);
            }
            else
            {
                Vertices = def.Vertices;
            }

            SetBaseColor(color);

            Stride = GetVerticesSize(def.Vertices) / Points;
        }

        internal int GetRenderDataOffset(ObjectRenderType renderType = ObjectRenderType.Unknown) 
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

        internal int GetVerticesSize()
        {
            return Vertices.Length * sizeof(float);
        }

        internal int GetVerticesSize(float[] vertices)
        {
            return vertices.Length * sizeof(float);
        }

        internal int GetVerticesDrawOrderSize()
        {
            return VerticesDrawOrder.Length * sizeof(uint);
        }

        internal float[] GetPureVertexData()
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

        internal void SetBaseColor(Vector4 color) 
        {
            BaseColor = color;

            CalculateInterpolatedColor();
        }

        private bool _useAppliedColors = true;
        internal void UseAppliedColors(bool use) 
        {
            if (_useAppliedColors != use)
            {
                _useAppliedColors = use;
                CalculateInterpolatedColor();
            }
        }

        internal void AddAppliedColor(Color color) 
        {
            AppliedColors.Add(color);

            CalculateInterpolatedColor();
        }

        internal void RemoveAppliedColor(Color color) 
        {
            AppliedColors.Remove(color);

            CalculateInterpolatedColor();
        }

        internal void CalculateInterpolatedColor() 
        {
            InterpolatedColor = new Vector4(BaseColor);

            float alpha = BaseColor.W;
            int count = 0;

            if (_useAppliedColors) 
            {
                for (int i = 0; i < AppliedColors.Count; i++) 
                {
                    if (!AppliedColors[i].Use)
                        continue;


                    InterpolatedColor.X += AppliedColors[i].R;
                    InterpolatedColor.Y += AppliedColors[i].G;
                    InterpolatedColor.Z += AppliedColors[i].B;

                    if (i == 0)
                    {
                        alpha = AppliedColors[i].A;
                    }
                    else 
                    {
                        alpha += AppliedColors[i].A;
                    }

                    count++;
                }

                if (count > 0) 
                {
                    alpha /= count;
                    InterpolatedColor.X /= count + 1;
                    InterpolatedColor.Y /= count + 1;
                    InterpolatedColor.Z /= count + 1;
                }
            }

            InterpolatedColor.W = alpha;
        }

        //TRANSLATE FUNCTIONS
        internal void TranslateX(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += f;
            Position.X = currentTranslation.X;

            SetTranslation(currentTranslation);
        }
        internal void TranslateY(float f) 
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Y += f;
            Position.Y = currentTranslation.Y;

            SetTranslation(currentTranslation);
        }
        internal void TranslateZ(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Z += f;
            Position.Z = currentTranslation.Z;

            SetTranslation(currentTranslation);
        }
        internal void Translate(Vector3 translation)
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
        internal void ScaleAll (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X *= f;
            currentScale.Y *= f;
            currentScale.Z *= f;

            SetScale(currentScale);
        }
        internal void SetScaleAll(float f)
        {
            Vector3 currentScale = new Vector3(f, f, f);

            SetScale(currentScale);
        }

        internal void SetScale(float x, float y, float z)
        {
            Vector3 currentScale = new Vector3(x, y, z);

            SetScale(currentScale);
        }

        internal void ScaleAddition(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X += f;
            currentScale.Y += f;
            currentScale.Z += f;

            SetScale(currentScale);
        }
        internal void ScaleX (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[0] *= f;

            SetScale(currentScale);
        }
        internal void ScaleY(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[1] *= f;

            SetScale(currentScale);
        }
        internal void ScaleZ(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[2] *= f;

            SetScale(currentScale);
        }

        //ROTATE FUNCTIONS
        internal void RotateX(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            RotationInfo.X += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        internal void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        internal void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Z += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }

        //TRANSFORMATION SETTERS
        //internal void SetRotation(Vector3 translations)
        //{
        //    Translation = Matrix4.CreateTranslation(translations);
        //}
        internal Vector3 CurrentScale = new Vector3(1, 1, 1);
        internal void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);
            CurrentScale = scale;

            CalculateTransformationMatrix();
        }
        internal void SetTranslation(Vector3 translations) 
        {
            Translation = Matrix4.CreateTranslation(translations);
            Position = new Vector4(Translation.ExtractTranslation(), Position.W);

            CalculateTransformationMatrix();
        }
        

        //TRANSFORMATION RESETTERS
        internal void ResetRotation()
        {
            Rotation = Matrix4.Identity;

            CalculateTransformationMatrix();
        }
        internal void ResetScale()
        {
            Scale = Matrix4.Identity;

            CalculateTransformationMatrix();
        }
        internal void ResetTranslation()
        {
            Translation = Matrix4.Identity;
            Position = new Vector4(0, 0, 0, Position.W);

            CalculateTransformationMatrix();
        }

        internal Vector3 GetRotation() 
        {
            Vector3 rotations = Transformations.ExtractRotation().ToEulerAngles();

            rotations.X = MathHelper.RadiansToDegrees(rotations.X);
            rotations.Y = MathHelper.RadiansToDegrees(rotations.Y);
            rotations.Z = MathHelper.RadiansToDegrees(rotations.Z);

            return rotations;
        }

        internal Vector3 GetRotationRadians()
        {
            return Transformations.ExtractRotation().ToEulerAngles();
        }

        private void CalculateTransformationMatrix() 
        {
            Transformations = Scale * Rotation * Translation;
        }
        
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
