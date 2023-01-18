using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public abstract class TransformationBase
    {
        public Matrix4 Transformations = Matrix4.Identity;
        public Matrix4 Translation = Matrix4.Identity;
        public Matrix4 Rotation = Matrix4.Identity;
        public Matrix4 Scale = Matrix4.Identity;

        public abstract void Translate(Vector3 translation);
        public abstract void SetTranslation(Vector3 translation);

        public abstract void TranslateX(float f);
        public abstract void TranslateY(float f);
        public abstract void TranslateZ(float f);

        public abstract void ScaleAll(float f);
        public abstract void SetScaleAll(float f);

        public abstract void SetScale(float x, float y, float z);

        public abstract void ScaleAddition(float f);
        public abstract void ScaleX(float f);
        public abstract void ScaleY(float f);
        public abstract void ScaleZ(float f);

        public abstract void SetScale(Vector3 scale);

        public abstract void RotateX(float degrees);
        public abstract void RotateY(float degrees);
        public abstract void RotateZ(float degrees);

        public abstract void ResetRotation();
        public abstract void ResetScale();
        public abstract void ResetTranslation();

        public void CalculateTransformations()
        {
            Transformations = Scale * Rotation * Translation;
        }

        public void ResetTransformations()
        {
            Transformations = Matrix4.Identity;
            Scale = Matrix4.Identity;
            Rotation = Matrix4.Identity;
            Translation = Matrix4.Identity;
        }

        public void SetSize(UIScale size, bool scaleAspectRatio)
        {
            float aspectRatio = scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            SetScaleAll(1);

            if (aspectRatio != 1)
                ScaleX(aspectRatio);

            ScaleX(ScaleFactor.X);
            ScaleY(ScaleFactor.Y);
        }

        /// <summary>
        /// Set the translation of the object in game coordinates instead of render coordinates
        /// </summary>
        public virtual void SetPosition(Vector3 position)
        {
            position = WindowConstants.ConvertScreenSpaceToLocalCoordinates(position);

            SetTranslation(position);
        }
    }
}
