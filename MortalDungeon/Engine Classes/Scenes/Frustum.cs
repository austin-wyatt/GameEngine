using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public class Frustum
    {
        private float[] clip_matrix = new float[16];
        private float[,] frustum = new float[6, 4];

        public enum ClippingPlanes
        {
            Right,
            Left,
            Bottom,
            Top,
            Back,
            Front
        }

        private void NormalizePlane(float[,] frustum, int side)
        {
            float magnitude = (float)Math.Sqrt((frustum[side, 0] * frustum[side, 0]) + (frustum[side, 1] * frustum[side, 1]) + (frustum[side, 2] * frustum[side, 2]));
            frustum[side, 0] /= magnitude;
            frustum[side, 1] /= magnitude;
            frustum[side, 2] /= magnitude;
            frustum[side, 3] /= magnitude;
        }

        public bool TestPoint(float x, float y, float z)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustum[i, 0] * x + frustum[i, 1] * y + frustum[i, 2] * z + frustum[i, 3] <= 0.0f)
                {
                    return false;
                }
            }
            return true;
        }

        public bool TestSphere(float x, float y, float z, float radius)
        {
            for (int p = 0; p < 6; p++)
            {
                float d = frustum[p, 0] * x + frustum[p, 1] * y + frustum[p, 2] * z + frustum[p, 3];
                if (d <= -radius)
                {
                    return false;
                }
            }
            return true;
        }

        public bool TestCube(float x, float y, float z, float size)
        {
            for (int i = 0; i < 6; i++)
            {
                if (frustum[i, 0] * (x - size) + frustum[i, 1] * (y - size) + frustum[i, 2] * (z - size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x + size) + frustum[i, 1] * (y - size) + frustum[i, 2] * (z - size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x - size) + frustum[i, 1] * (y + size) + frustum[i, 2] * (z - size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x + size) + frustum[i, 1] * (y + size) + frustum[i, 2] * (z - size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x - size) + frustum[i, 1] * (y - size) + frustum[i, 2] * (z + size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x + size) + frustum[i, 1] * (y - size) + frustum[i, 2] * (z + size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x - size) + frustum[i, 1] * (y + size) + frustum[i, 2] * (z + size) + frustum[i, 3] > 0)
                    continue;
                if (frustum[i, 0] * (x + size) + frustum[i, 1] * (y + size) + frustum[i, 2] * (z + size) + frustum[i, 3] > 0)
                    continue;
                return false;
            }
            return true;
        }


        public void CalculateFrustum(Matrix4 projectionMatrix, Matrix4 modelViewMatrix)
        {
            clip_matrix[0] = (modelViewMatrix.M11 * projectionMatrix.M11) + (modelViewMatrix.M12 * projectionMatrix.M21) + (modelViewMatrix.M13 * projectionMatrix.M31) + (modelViewMatrix.M14 * projectionMatrix.M41);
            clip_matrix[1] = (modelViewMatrix.M11 * projectionMatrix.M12) + (modelViewMatrix.M12 * projectionMatrix.M22) + (modelViewMatrix.M13 * projectionMatrix.M32) + (modelViewMatrix.M14 * projectionMatrix.M42);
            clip_matrix[2] = (modelViewMatrix.M11 * projectionMatrix.M13) + (modelViewMatrix.M12 * projectionMatrix.M23) + (modelViewMatrix.M13 * projectionMatrix.M33) + (modelViewMatrix.M14 * projectionMatrix.M43);
            clip_matrix[3] = (modelViewMatrix.M11 * projectionMatrix.M14) + (modelViewMatrix.M12 * projectionMatrix.M24) + (modelViewMatrix.M13 * projectionMatrix.M34) + (modelViewMatrix.M14 * projectionMatrix.M44);

            clip_matrix[4] = (modelViewMatrix.M21 * projectionMatrix.M11) + (modelViewMatrix.M22 * projectionMatrix.M21) + (modelViewMatrix.M23 * projectionMatrix.M31) + (modelViewMatrix.M24 * projectionMatrix.M41);
            clip_matrix[5] = (modelViewMatrix.M21 * projectionMatrix.M12) + (modelViewMatrix.M22 * projectionMatrix.M22) + (modelViewMatrix.M23 * projectionMatrix.M32) + (modelViewMatrix.M24 * projectionMatrix.M42);
            clip_matrix[6] = (modelViewMatrix.M21 * projectionMatrix.M13) + (modelViewMatrix.M22 * projectionMatrix.M23) + (modelViewMatrix.M23 * projectionMatrix.M33) + (modelViewMatrix.M24 * projectionMatrix.M43);
            clip_matrix[7] = (modelViewMatrix.M21 * projectionMatrix.M14) + (modelViewMatrix.M22 * projectionMatrix.M24) + (modelViewMatrix.M23 * projectionMatrix.M34) + (modelViewMatrix.M24 * projectionMatrix.M44);

            clip_matrix[8] = (modelViewMatrix.M31 * projectionMatrix.M11) + (modelViewMatrix.M32 * projectionMatrix.M21) + (modelViewMatrix.M33 * projectionMatrix.M31) + (modelViewMatrix.M34 * projectionMatrix.M41);
            clip_matrix[9] = (modelViewMatrix.M31 * projectionMatrix.M12) + (modelViewMatrix.M32 * projectionMatrix.M22) + (modelViewMatrix.M33 * projectionMatrix.M32) + (modelViewMatrix.M34 * projectionMatrix.M42);
            clip_matrix[10] = (modelViewMatrix.M31 * projectionMatrix.M13) + (modelViewMatrix.M32 * projectionMatrix.M23) + (modelViewMatrix.M33 * projectionMatrix.M33) + (modelViewMatrix.M34 * projectionMatrix.M43);
            clip_matrix[11] = (modelViewMatrix.M31 * projectionMatrix.M14) + (modelViewMatrix.M32 * projectionMatrix.M24) + (modelViewMatrix.M33 * projectionMatrix.M34) + (modelViewMatrix.M34 * projectionMatrix.M44);

            clip_matrix[12] = (modelViewMatrix.M41 * projectionMatrix.M11) + (modelViewMatrix.M42 * projectionMatrix.M21) + (modelViewMatrix.M43 * projectionMatrix.M31) + (modelViewMatrix.M44 * projectionMatrix.M41);
            clip_matrix[13] = (modelViewMatrix.M41 * projectionMatrix.M12) + (modelViewMatrix.M42 * projectionMatrix.M22) + (modelViewMatrix.M43 * projectionMatrix.M32) + (modelViewMatrix.M44 * projectionMatrix.M42);
            clip_matrix[14] = (modelViewMatrix.M41 * projectionMatrix.M13) + (modelViewMatrix.M42 * projectionMatrix.M23) + (modelViewMatrix.M43 * projectionMatrix.M33) + (modelViewMatrix.M44 * projectionMatrix.M43);
            clip_matrix[15] = (modelViewMatrix.M41 * projectionMatrix.M14) + (modelViewMatrix.M42 * projectionMatrix.M24) + (modelViewMatrix.M43 * projectionMatrix.M34) + (modelViewMatrix.M44 * projectionMatrix.M44);

            frustum[(int)ClippingPlanes.Right, 0] = clip_matrix[3] - clip_matrix[0];
            frustum[(int)ClippingPlanes.Right, 1] = clip_matrix[7] - clip_matrix[4];
            frustum[(int)ClippingPlanes.Right, 2] = clip_matrix[11] - clip_matrix[8];
            frustum[(int)ClippingPlanes.Right, 3] = clip_matrix[15] - clip_matrix[12];
            NormalizePlane(frustum, (int)ClippingPlanes.Right);

            frustum[(int)ClippingPlanes.Left, 0] = clip_matrix[3] + clip_matrix[0];
            frustum[(int)ClippingPlanes.Left, 1] = clip_matrix[7] + clip_matrix[4];
            frustum[(int)ClippingPlanes.Left, 2] = clip_matrix[11] + clip_matrix[8];
            frustum[(int)ClippingPlanes.Left, 3] = clip_matrix[15] + clip_matrix[12];
            NormalizePlane(frustum, (int)ClippingPlanes.Left);

            frustum[(int)ClippingPlanes.Bottom, 0] = clip_matrix[3] + clip_matrix[1];
            frustum[(int)ClippingPlanes.Bottom, 1] = clip_matrix[7] + clip_matrix[5];
            frustum[(int)ClippingPlanes.Bottom, 2] = clip_matrix[11] + clip_matrix[9];
            frustum[(int)ClippingPlanes.Bottom, 3] = clip_matrix[15] + clip_matrix[13];
            NormalizePlane(frustum, (int)ClippingPlanes.Bottom);

            frustum[(int)ClippingPlanes.Top, 0] = clip_matrix[3] - clip_matrix[1];
            frustum[(int)ClippingPlanes.Top, 1] = clip_matrix[7] - clip_matrix[5];
            frustum[(int)ClippingPlanes.Top, 2] = clip_matrix[11] - clip_matrix[9];
            frustum[(int)ClippingPlanes.Top, 3] = clip_matrix[15] - clip_matrix[13];
            NormalizePlane(frustum, (int)ClippingPlanes.Top);

            frustum[(int)ClippingPlanes.Back, 0] = clip_matrix[3] - clip_matrix[2];
            frustum[(int)ClippingPlanes.Back, 1] = clip_matrix[7] - clip_matrix[6];
            frustum[(int)ClippingPlanes.Back, 2] = clip_matrix[11] - clip_matrix[10];
            frustum[(int)ClippingPlanes.Back, 3] = clip_matrix[15] - clip_matrix[14];
            NormalizePlane(frustum, (int)ClippingPlanes.Back);

            frustum[(int)ClippingPlanes.Front, 0] = clip_matrix[3] + clip_matrix[2];
            frustum[(int)ClippingPlanes.Front, 1] = clip_matrix[7] + clip_matrix[6];
            frustum[(int)ClippingPlanes.Front, 2] = clip_matrix[11] + clip_matrix[10];
            frustum[(int)ClippingPlanes.Front, 3] = clip_matrix[15] + clip_matrix[14];
            NormalizePlane(frustum, (int)ClippingPlanes.Front);
        }
    }
}
