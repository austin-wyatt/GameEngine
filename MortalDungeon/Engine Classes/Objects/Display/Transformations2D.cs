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

        public float CurrentRotation = 0;

        public void TranslateBy(Vector2 amount)
        {
            Translation.Row0.Z += amount.X;
            Translation.Row1.Z += amount.Y;

            CalculateMatrix();
        }

        public void SetTranslation(Vector2 amount)
        {
            Translation.Row0.Z = amount.X;
            Translation.Row1.Z = amount.Y;

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

        public void ScaleBy(Vector2 scale)
        {
            Scale.Row0.X += scale.X;
            Scale.Row1.Y += scale.Y;

            CalculateMatrix();
        }

        public void SetScale(Vector2 scale)
        {
            Scale.Row0.X = scale.X;
            Scale.Row1.Y = scale.Y;

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
            Transformations = Translation * Shear * Scale * Rotation;
            //Transformations = Translation;

            //Multiply order = translation * shear * scaling * rotation
        }
    }
}
