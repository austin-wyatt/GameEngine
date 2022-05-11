using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    /// <summary>
    /// Contains all vertex (any combination of position, texture, and normal that is necessary) 
    /// data and transformation data. <para/>
    /// 
    /// Setting vertex, draw order, and stride information is the responsibility of the implementing class. <para/>
    /// 
    /// This class is intended to be a barebones (and more updated) version of RenderableObject which should
    /// hopefully provide some more flexibility for non-standard objects (such as code generated meshes and whatnot)
    /// </summary>
    public class TransformableMesh : TransformationBase
    {
        public float[] Vertices;

        /// <summary>
        /// The order in which the vertices should be drawn to create triangles.
        /// </summary>
        public uint[] VertexDrawOrder;

        /// <summary>
        /// The size in bytes per vertex. If the vertex data includes position, texture, and normal
        /// then the stride would be (3 + 2 + 3) * sizeof(float) = 32.
        /// </summary>
        public int Stride;

        protected Matrix4 Translation = Matrix4.Identity;
        protected Matrix4 Rotation = Matrix4.Identity;
        protected Matrix4 Scale = Matrix4.Identity;

        public Vector3 CurrentScale = new Vector3(1, 1, 1);
        public Vector3 Position = new Vector3();

        public TransformableMesh()
        {

        }

        #region Translations
        public void Translate(Vector3 translation)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += translation.X;
            currentTranslation.Y += translation.Y;
            currentTranslation.Z += translation.Z;

            Translation = Matrix4.CreateTranslation(currentTranslation);

            Position = currentTranslation;

            CalculateTransformations();
        }

        public void SetTranslation(Vector3 translation)
        {
            Translation = Matrix4.CreateTranslation(translation);

            Position = translation;

            CalculateTransformations();
        }
        #endregion

        #region Scale
        public void ScaleAll(float f)
        {
            CurrentScale.X *= f;
            CurrentScale.Y *= f;
            CurrentScale.Z *= f;

            SetScale(CurrentScale);
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
            CurrentScale.X += f;
            CurrentScale.Y += f;
            CurrentScale.Z += f;

            SetScale(CurrentScale);
        }
        public void ScaleX(float f)
        {
            CurrentScale[0] *= f;

            SetScale(CurrentScale);
        }
        public void ScaleY(float f)
        {
            CurrentScale[1] *= f;

            SetScale(CurrentScale);
        }
        public void ScaleZ(float f)
        {
            CurrentScale[2] *= f;

            SetScale(CurrentScale);
        }

        public void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);
            CurrentScale = scale;

            CalculateTransformations();
        }
        #endregion

        #region Rotations
        public void RotateX(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));

            Rotation *= rotationMatrix;

            CalculateTransformations();
        }
        public void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));

            Rotation *= rotationMatrix;

            CalculateTransformations();
        }
        public void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));

            Rotation *= rotationMatrix;

            CalculateTransformations();
        }
        #endregion

        protected void CalculateTransformations()
        {
            Transformations = Scale * Rotation * Translation;
            //Transformations = Translation * Scale * Rotation;
        }
    }
}
