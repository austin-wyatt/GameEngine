using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    /// <summary>
    /// Contains handling for a 3D transformation matrix
    /// </summary>
    public class Transformations3D : TransformationBase
    {
        public Vector3 CurrentScale = new Vector3(1, 1, 1);
        public Vector3 Position = new Vector3();

        public Transformations3D()
        {

        }

        #region Translations
        public override void Translate(Vector3 translation)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += translation.X;
            currentTranslation.Y += translation.Y;
            currentTranslation.Z += translation.Z;

            SetTranslation(currentTranslation);
        }

        public override void SetTranslation(Vector3 translation)
        {
            Translation = Matrix4.CreateTranslation(translation);

            Position = translation;

            CalculateTransformations();
        }

        public override void TranslateX(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.X += f;

            SetTranslation(currentTranslation);
        }
        public override void TranslateY(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Y += f;

            SetTranslation(currentTranslation);
        }
        public override void TranslateZ(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation.Z += f;

            SetTranslation(currentTranslation);
        }


        #endregion

        #region Scale
        public override void ScaleAll(float f)
        {
            CurrentScale.X *= f;
            CurrentScale.Y *= f;
            CurrentScale.Z *= f;

            SetScale(CurrentScale);
        }
        public override void SetScaleAll(float f)
        {
            Vector3 currentScale = new Vector3(f, f, f);

            SetScale(currentScale);
        }

        public override void SetScale(float x, float y, float z)
        {
            Vector3 currentScale = new Vector3(x, y, z);

            SetScale(currentScale);
        }

        public override void ScaleAddition(float f)
        {
            CurrentScale.X += f;
            CurrentScale.Y += f;
            CurrentScale.Z += f;

            SetScale(CurrentScale);
        }
        public override void ScaleX(float f)
        {
            CurrentScale[0] *= f;

            SetScale(CurrentScale);
        }
        public override void ScaleY(float f)
        {
            CurrentScale[1] *= f;

            SetScale(CurrentScale);
        }
        public override void ScaleZ(float f)
        {
            CurrentScale[2] *= f;

            SetScale(CurrentScale);
        }

        public override void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);
            CurrentScale = scale;

            CalculateTransformations();
        }
        #endregion

        #region Rotations
        public override void RotateX(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformations();
        }
        public override void RotateY(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformations();
        }
        public override void RotateZ(float degrees)
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));

            Rotation.MultInPlace(ref rotationMatrix);

            CalculateTransformations();
        }
        #endregion

        #region Transformation resetters
        public override void ResetRotation()
        {
            Rotation = Matrix4.Identity;

            CalculateTransformations();
        }
        public override void ResetScale()
        {
            Scale = Matrix4.Identity;
            CurrentScale.X = 1;
            CurrentScale.Y = 1;
            CurrentScale.Z = 1;

            CalculateTransformations();
        }
        public override void ResetTranslation()
        {
            Translation = Matrix4.Identity;
            Position = new Vector3(0, 0, 0);

            CalculateTransformations();
        }
        #endregion
    }
}
