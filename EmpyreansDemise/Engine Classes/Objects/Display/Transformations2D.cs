using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public class Transformations2D
    {
        public Matrix3 Transformations = Matrix3.Identity;

        public Matrix3 Translation = Matrix3.Identity;
        public Matrix3 Rotation = Matrix3.Identity;
        public Matrix3 Scale = Matrix3.Identity;
        public Matrix3 Shear = Matrix3.Identity;

        public Vector2 CurrentTranslation = new Vector2();

        public float CurrentRotation = 0;


        public void TranslateBy(Vector2 amount)
        {
            Translation.Row0.Z += amount.X;
            Translation.Row1.Z += amount.Y;

            CurrentTranslation.X = Translation.Row0.Z;
            CurrentTranslation.Y = Translation.Row1.Z;

            CalculateMatrix();
        }

        public void SetTranslation(Vector2 amount)
        {
            Translation.Row0.Z = amount.X;
            Translation.Row1.Z = amount.Y;

            CurrentTranslation.X = Translation.Row0.Z;
            CurrentTranslation.Y = Translation.Row1.Z;

            CalculateMatrix();
        }

        public void RotateBy(float radians)
        {
            CurrentRotation += radians;

            SetRotation(CurrentRotation);
        }

        public void SetRotation(float radians)
        {
            CurrentRotation = radians;

            Rotation.Row0.X = (float)MathHelper.Cos(CurrentRotation);
            Rotation.Row0.Y = (float)-MathHelper.Sin(CurrentRotation);
            Rotation.Row1.X = (float)MathHelper.Sin(CurrentRotation);
            Rotation.Row1.Y = (float)MathHelper.Cos(CurrentRotation);

            CalculateMatrix();
        }

        public void ScaleBy(Vector2 scale, Vector2 centerPoint = default)
        {
            Matrix3 tempMatrix = Matrix3.Identity;

            if (centerPoint.X != 0 || centerPoint.Y != 0)
            {
                tempMatrix.Row0.Z = centerPoint.X;
                tempMatrix.Row1.Z = centerPoint.Y;
                Scale.MultInPlace(ref tempMatrix);
            }

            Scale.Row0.X += scale.X;
            Scale.Row1.Y += scale.Y;

            if (centerPoint.X != 0 || centerPoint.Y != 0)
            {
                tempMatrix.Row0.Z = -centerPoint.X;
                tempMatrix.Row1.Z = -centerPoint.Y;
                Scale.MultInPlace(ref tempMatrix);
            }
            CalculateMatrix();
        }

        public void SetScale(Vector2 scale, Vector2 centerPoint = default)
        {
            Matrix3 tempMatrix = Matrix3.Identity;
            tempMatrix.Row0.Z = centerPoint.X;
            tempMatrix.Row1.Z = centerPoint.Y;
            Scale.MultInPlace(ref tempMatrix);

            Scale.Row0.X = scale.X;
            Scale.Row1.Y = scale.Y;

            tempMatrix.Row0.Z = -centerPoint.X;
            tempMatrix.Row1.Z = -centerPoint.Y;
            Scale.MultInPlace(ref tempMatrix);

            CalculateMatrix();
        }

        public void ShearBy(Vector2 shear)
        {
            Shear.Row0.Y += shear.X;
            Shear.Row1.X += shear.Y;

            CalculateMatrix();
        }

        public void SetShear(Vector2 shear)
        {
            Shear.Row0.Y = shear.X;
            Shear.Row1.X = shear.Y;

            CalculateMatrix();
        }

        public Vector2 GetShear()
        {
            return new Vector2(Shear.Row0.Y, Shear.Row1.X);
        }

        public Vector2 GetScale()
        {
            return new Vector2(Scale.Row0.X, Scale.Row1.Y);
        }

        public Vector2 GetTranslation()
        {
            return new Vector2(Translation.Row0.Z, Translation.Row1.Z);
        }
        

        public void CalculateMatrix()
        {
            Transformations = Matrix3.Identity;
            Transformations.MultInPlace(ref Scale);

            Transformations.MultInPlace(ref Rotation);

            Transformations.MultInPlace(ref Shear);

            Transformations.MultInPlace(ref Translation);

            //Multiply order = scaling * rotation * shear * translation
        }

        public void ResetTransformations()
        {
            Transformations = Matrix3.Identity;
            Scale = Matrix3.Identity;
            Rotation = Matrix3.Identity;
            Shear = Matrix3.Identity;
            Translation = Matrix3.Identity;
        }
    }
}
