using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.MiscOperations
{
    public class MouseRay
    {
        private Vector3 currentRay;

        private Camera camera;

        private Vector2i _windowSize;

        public MouseRay(Camera _camera, Vector2i windowSize)
        {
            camera = _camera;
            _windowSize = windowSize;
        }

        public Vector3 GetCurrentRay() 
        {
            return currentRay;
        }

        public void Update(Vector2 mouseCoordinates) 
        {
            currentRay = CalculateMouseRay(mouseCoordinates);
        }

        //private Vector3 CalculateMouseRay(Vector2 mouseCoordinates)
        //{
        //    float tmpX = (2 * mouseCoordinates.X) / _windowSize.X - 1;
        //    float tmpY = ((2 * mouseCoordinates.Y) / _windowSize.Y - 1) * -1;
        //    float z = 1;

        //    Vector3 ray_nds = new Vector3(tmpX, tmpY, z);
        //    Vector4 ray_clip = new Vector4(ray_nds.X, ray_nds.Y, -1, 1);
        //    Vector4 ray_eye = ray_clip * camera.GetProjectionMatrix().Inverted();

        //    ray_eye.Z = -1;
        //    ray_eye.W = 0;
        //    Vector4 tmp = ray_eye * camera.GetViewMatrix().Inverted();
        //    Vector3 worldRay = new Vector3(tmp.X, tmp.Y, tmp.Z);

        //    worldRay.Normalize();
        //    //worldRay.Z = -1;

        //    return worldRay;
        //}
        private Vector3 CalculateMouseRay(Vector2 mouseCoordinates)
        {
            Vector2 nomralizedCoords = GetNormalizedDeviceCoords(mouseCoordinates.X, mouseCoordinates.Y);
            Vector4 clipCoords = new Vector4(nomralizedCoords.X, nomralizedCoords.Y, -1, 1);
            Vector4 eyeCoords = ToEyeCoords(clipCoords);
            Vector3 worldRay = ToWorldCoords(eyeCoords);

            return worldRay;
        }

        private Vector4 ToEyeCoords(Vector4 clipCoords)
        {
            Matrix4 invertedProjection = Matrix4.Invert(camera.GetProjectionMatrix());
            Vector4 eyeCoords = clipCoords * invertedProjection;
            return new Vector4(eyeCoords.X, eyeCoords.Y, -1, 0);
        }

        private Vector3 ToWorldCoords(Vector4 eyeCoords)
        {
            Matrix4 invertedView = Matrix4.Invert(camera.GetViewMatrix());
            Vector4 rayWorld = eyeCoords * invertedView;
            Vector3 mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);

            mouseRay.Normalize();
            return mouseRay;
        }

        private Vector2 GetNormalizedDeviceCoords(float mouseX, float mouseY)
        {
            float x = (mouseX / _windowSize.X) * 2 - 1;
            float y = ((mouseY / _windowSize.Y) * 2 - 1) * -1;

            return new Vector2(x, y);
        }

        //not needed but good for referencing
        public Vector3 UnProject(float mouseX, float mouseY, float z, Camera camera, Vector2 Viewport)
        {
            Vector4 vec;

            vec.X = 2.0f * mouseX / Viewport.X - 1;
            vec.Y = -(2.0f * mouseY / Viewport.Y - 1);
            vec.Z = z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(camera.GetViewMatrix());
            Matrix4 projInv = Matrix4.Invert(camera.GetProjectionMatrix());

            vec *= projInv;
            vec *= viewInv;

            if (vec.W > 0.000001f || vec.W < -0.000001f)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec.Xyz;
        }
    }




    
}
