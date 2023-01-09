using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Empyrean.Engine_Classes.Lighting;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using OpenTK.Mathematics;

namespace Empyrean.Engine_Classes
{
    public enum ObjectRenderType 
    {
        Unknown,
        Color,
        Texture,
        Particle
    }

    public static class _Colors 
    {
        public static Vector4 Black = new Vector4(0, 0, 0, 1);
        public static Vector4 White = new Vector4(1, 1, 1, 1);
        public static Vector4 Red = new Vector4(1, 0, 0, 1);
        public static Vector4 Green = new Vector4(0, 1, 0, 1);
        public static Vector4 Blue = new Vector4(0, 0, 1, 1);
        public static Vector4 Tan = new Vector4(0.68f, 0.66f, 0.48f, 1);
        public static Vector4 DarkTan = new Vector4(0.258f, 0.251f, 0.184f, 1);
        public static Vector4 Purple = new Vector4(0.66f, 0.03f, 0.71f, 1);
        public static Vector4 Yellow = new Vector4(1, 1, 0, 1);

        public static Vector4 LightBlue = new Vector4(0, 0.70f, 1, 1);

        public static Vector4 GrassGreen = new Vector4(0.1f, 0.30f, 0, 1);

        public static Vector4 TranslucentRed = new Vector4(1, 0, 0, 0.5f);
        public static Vector4 MoreTranslucentRed = new Vector4(1, 0, 0, 0.35f);
        public static Vector4 TranslucentBlue = new Vector4(0, 0, 1, 0.25f);
        public static Vector4 MoreTranslucentBlue = new Vector4(0, 0, 1, 0.35f);

        public static Vector4 TranslucentTan = new Vector4(0.68f, 0.66f, 0.48f, 0.5f);

        public static Vector4 LessAggressiveRed = new Vector4(0.62f, 0.18f, 0.18f, 1);

        public static Vector4 UILightGray = new Vector4(0.85f, 0.85f, 0.85f, 1);
        public static Vector4 UIDefaultGray = new Vector4(0.5f, 0.5f, 0.5f, 1);
        public static Vector4 UIHoveredGray = new Vector4(0.4f, 0.4f, 0.4f, 1);
        public static Vector4 UISelectedGray = new Vector4(0.3f, 0.3f, 0.3f, 1);
        public static Vector4 UIDisabledGray = new Vector4(0.71f, 0.71f, 0.71f, 1);
        //public static Vector4 UITextBlack = new Vector4(0.1f, 0.1f, 0.1f, 1);
        public static Vector4 UITextBlack = new Vector4(0.03f, 0.03f, 0.03f, 1);

        public static Vector4 IconHover = new Vector4(0.85f, 0.85f, 0.85f, 1);
        public static Vector4 IconSelected = new Vector4(0.78f, 0.78f, 0.78f, 1);
        public static Vector4 IconDisabled = new Vector4(0.7f, 0.7f, 0.7f, 1);

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

    public class RenderableObject : TransformationBase, IHasPosition
    {
        public float[] Vertices;
        public ObjectRenderType RenderType = ObjectRenderType.Color;
        public uint[] VerticesDrawOrder;
        public int Points;
        public Vector3 Center;

        //when a renderable object is loaded into a scene it's texture needs to be added to the texture list
        public TextureInfo Textures;

        //Every renderable object begins at the origin and is placed from there.
        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 _position = new Vector3();

        public int Stride;

        //Textures and shaders will be loaded separately then assigned to the object
        public Shader ShaderReference;
        public Material Material = new Material();


        //transformations
        public Matrix4 Translation = Matrix4.Identity;
        public Matrix4 Rotation = Matrix4.Identity;
        public Matrix4 Scale = Matrix4.Identity;

        public RotationData RotationInfo = new RotationData() { X = 0, Y = 0, Z = 0 };

        public Vector4 BaseColor = new Vector4();
        public Vector4 InterpolatedColor = new Vector4();
        public float ColorProportion = 0;

        public HashSet<_Color> AppliedColors = new HashSet<_Color>(); 

        public bool CameraPerspective = false;

        public float SpritesheetPosition = 0;
        public Vector2 SideLengths = new Vector2(1, 1);

        public float VerticeType = 0;

        public RenderableObject() { }

        public RenderableObject(float[] vertices, uint[] verticesDrawOrder, int points, TextureInfo textures, Vector4 color, ObjectRenderType renderType, Shader shaderReference, Vector3 center = new Vector3()) 
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

        public RenderableObject(ObjectDefinition def, Vector4 color, ObjectRenderType renderType, Shader shaderReference)
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

        public RenderableObject(RenderableObject oldObj)
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

            Position = new Vector3(oldObj.Position);
        }

        public RenderableObject(ObjectDefinition def, Vector4 color, Shader shaderReference)
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

        public void SetBaseColor(Vector4 color) 
        {
            BaseColor = color;

            CalculateInterpolatedColor();
        }

        private bool _useAppliedColors = true;
        public void UseAppliedColors(bool use) 
        {
            if (_useAppliedColors != use)
            {
                _useAppliedColors = use;
                CalculateInterpolatedColor();
            }
        }

        private object _colorLock = new object();

        public void AddAppliedColor(_Color color) 
        {
            lock (_colorLock)
            {
                AppliedColors.Add(color);
            }

            CalculateInterpolatedColor();
        }

        public void RemoveAppliedColor(_Color color) 
        {
            lock (_colorLock)
            {
                AppliedColors.Remove(color);
            }

            CalculateInterpolatedColor();
        }

        public void CalculateInterpolatedColor() 
        {
            InterpolatedColor = new Vector4(BaseColor);

            float alpha = BaseColor.W;
            int count = 0;

            if (_useAppliedColors) 
            {
                lock (_colorLock)
                {
                    foreach(var color in AppliedColors)
                    {
                        if (!color.Use)
                            continue;


                        InterpolatedColor.X += color.R;
                        InterpolatedColor.Y += color.G;
                        InterpolatedColor.Z += color.B;

                        if (count == 0)
                        {
                            alpha = color.A;
                        }
                        else
                        {
                            alpha += color.A;
                        }

                        count++;
                    }
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
        public void TranslateX(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += f;

            SetTranslation(currentTranslation);
        }
        public void TranslateY(float f) 
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Y += f;

            SetTranslation(currentTranslation);
        }
        public void TranslateZ(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Z += f;

            SetTranslation(currentTranslation);
        }
        public void Translate(Vector3 translation)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += translation.X;
            currentTranslation.Y += translation.Y;
            currentTranslation.Z += translation.Z;


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

        public void SetScale(float x, float y, float z)
        {
            Vector3 currentScale = new Vector3(x, y, z);

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

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformationMatrix();
        }
        public void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformationMatrix();
        }
        public void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));

            RotationInfo.Z += degrees;

            Rotation.MultInPlace(ref rotationMatrix);
            

            CalculateTransformationMatrix();
        }

        //TRANSFORMATION SETTERS
        public Vector3 CurrentScale = new Vector3(1, 1, 1);
        public void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);
            CurrentScale = scale;

            CalculateTransformationMatrix();
        }
        public void SetTranslation(Vector3 translations) 
        {
            Translation = Matrix4.CreateTranslation(translations);
            _position = translations;

            CalculateTransformationMatrix();
        }


        //TRANSFORMATION RESETTERS
        public void ResetRotation()
        {
            Rotation = Matrix4.Identity;
            RotationInfo.X = 0;
            RotationInfo.Y = 0;
            RotationInfo.Z = 0;

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
            Position = new Vector3(0, 0, 0);

            CalculateTransformationMatrix();
        }

        public Vector3 GetRotation() 
        {
            Vector3 rotations = Transformations.ExtractRotation().ToEulerAngles();

            rotations.X = MathHelper.RadiansToDegrees(rotations.X);
            rotations.Y = MathHelper.RadiansToDegrees(rotations.Y);
            rotations.Z = MathHelper.RadiansToDegrees(rotations.Z);

            return rotations;
        }

        public Vector3 GetRotationRadians()
        {
            return Transformations.ExtractRotation().ToEulerAngles();
        }

        public void CalculateTransformationMatrix() 
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

        public void SetPosition(Vector3 position)
        {
            _position = position;
        }

        public void SetPosition(float x, float y, float z)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;
        }
    }
}
