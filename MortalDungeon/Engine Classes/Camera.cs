using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Engine_Classes
{
    public class Camera
    {
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        private float _pitch;

        private float _yaw = -MathHelper.PiOver2; // Without this you would be started rotated 90 degrees right

        private float _fov = MathHelper.PiOver2;
        //private float _fov = 0.78f;
        //private float _fov = 0.35f;

        public Matrix4 ProjectionMatrix;

        public Action onUpdate = null;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public Vector3 Position;

        public float AspectRatio;

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public float GetFOVRadians() 
        {
            return _fov;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.1f, 75f);
        }

        public void UpdateProjectionMatrix() 
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.1f, 75f);
        }

        private void UpdateVectors()
        {
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));

            Update?.Invoke(this);
        }


        private int _lateralRotations = 4; //start in the positive Y direction
        private int _verticalSteps = 1;
        public float CameraAngle = 0;
        public void RotateByAmount(int lateralStep = 0, int verticalStep = 0)
        {
            _lateralRotations += lateralStep;

            if (_lateralRotations > 15)
            {
                _lateralRotations = _lateralRotations % 16;
            }
            else if(_lateralRotations < 0)
            {
                _lateralRotations = 16 + _lateralRotations % 16;
            }

            _verticalSteps += verticalStep;

            if(_verticalSteps < 1)
            {
                _verticalSteps = 1;
            }
            else if(_verticalSteps > 5)
            {
                _verticalSteps = 5;
            }

            float alpha = (float)Math.PI / 8 * _lateralRotations;
            float beta = (float)-Math.PI / 2 - (float)-Math.PI / 16 * _verticalSteps;

            Vector3 rotatedVec = new Vector3((float)(Math.Cos(alpha) * Math.Cos(beta)), (float)(Math.Sin(alpha) * Math.Cos(beta)), (float)Math.Sin(beta));

            _front = Vector3.Normalize(rotatedVec);

            beta += (float)Math.PI / 2;

            rotatedVec = new Vector3((float)(Math.Cos(alpha) * Math.Cos(beta)), (float)(Math.Sin(alpha) * Math.Cos(beta)), (float)Math.Sin(beta));
            _up = Vector3.Normalize(rotatedVec);

            CameraAngle = alpha - (float)Math.PI / 2;

            Update?.Invoke(this);
            Rotate?.Invoke(this);
        }

        public void SetPosition(Vector3 pos) 
        {
            Position = pos;

            Update?.Invoke(this);
        }

        public delegate void CameraEventHandler(Camera camera);
        public CameraEventHandler Update;
        public CameraEventHandler Rotate;
    }
}