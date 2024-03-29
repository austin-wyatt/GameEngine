﻿using System;
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

    public class RenderableObject : Transformations3D, IHasPosition
    {
        public float[] Vertices;
        public ObjectRenderType RenderType = ObjectRenderType.Color;
        public uint[] VerticesDrawOrder;
        public int Points;
        public Vector3 Center;

        //when a renderable object is loaded into a scene it's texture needs to be added to the texture list
        public TextureInfo Textures;

        //Every renderable object begins at the origin and is placed from there.
        public new Vector3 Position { get => _position; set => _position = value; }
        public Vector3 _position = new Vector3();

        public int Stride;

        //Textures and shaders will be loaded separately then assigned to the object
        public Shader ShaderReference;
        public Material Material = new Material();


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
            Vertices = vertices;
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

            Vertices = def.Vertices;

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

            Vertices = def.Vertices;

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


        //ROTATE FUNCTIONS
        public override void RotateX(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            RotationInfo.X += degrees;

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformations();
        }
        public override void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformations();
        }
        public override void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));

            RotationInfo.Z += degrees;

            Rotation.MultInPlace(ref rotationMatrix);
            
            CalculateTransformations();
        }

        public override void SetTranslation(Vector3 translations) 
        {
            Translation = Matrix4.CreateTranslation(translations);
            _position = translations;

            CalculateTransformations();
        }


        //TRANSFORMATION RESETTERS
        public override void ResetRotation()
        {
            Rotation = Matrix4.Identity;
            RotationInfo.X = 0;
            RotationInfo.Y = 0;
            RotationInfo.Z = 0;

            CalculateTransformations();
        }
        public override void ResetTranslation()
        {
            Translation = Matrix4.Identity;
            Position = new Vector3(0, 0, 0);

            CalculateTransformations();
        }

        public override void SetPosition(Vector3 position)
        {
            _position = position;

            base.SetPosition(position);
        }
    }
}
