﻿using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class BaseObject
    {
        internal int ID;
        internal string Name;
        internal Vector3 Position; //uses global position (based off of screen width and height), use GetDisplay().Position for local coordinates 
        internal Bounds Bounds;
        internal Vector3 PositionalOffset = new Vector3();

        internal Dictionary<AnimationType, Animation> Animations = new Dictionary<AnimationType, Animation>();
        internal AnimationType CurrentAnimationType = AnimationType.Idle; //static textures will use the idle animation


        internal bool LockToWindow = false;
        internal bool Render = true;

        internal bool EnableLighting = true;

        private Vector3 _dimensions;
        internal Animation _currentAnimation;

        internal RenderableObject BaseFrame;

        internal OutlineParameters OutlineParameters = new OutlineParameters();

        internal RenderData RenderData = new RenderData() { AlphaThreshold = Rendering.RenderingConstants.DefaultAlphaThreshold };

        internal Vector3 Dimensions 
        {
            get 
            {
                return _dimensions * new Vector3(BaseFrame.Scale.M11, BaseFrame.Scale.M22, BaseFrame.Scale.M33);
            }
            set 
            {
                _dimensions = value;
            }
        }

        internal BaseObject(List<Animation> animations, int id, string name, Vector3 position, float[] bounds = null) 
        {
            ID = id;
            Name = name;
            Position = new Vector3(position);

            for (int i = 0; i < animations.Count; i++)
            {
                Animations[animations[i].Type] = new Animation(animations[i]);
            }

            _currentAnimation = Animations[AnimationType.Idle];
            BaseFrame = _currentAnimation.Frames[0];

            if (bounds == null)
            {
                Bounds = new Bounds(BaseFrame.GetPureVertexData(), BaseFrame);
            }
            else
            {
                Bounds = new Bounds(bounds, BaseFrame);
            }

            _dimensions = Bounds.GetDimensionData();

            SetPosition(position);
        }

        internal BaseObject(RenderableObject obj, int id, string name, Vector3 position, float[] bounds = null)
        {
            ID = id;
            Name = name;
            Position = new Vector3(position);

            Animation temp = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = 0,
                Repeats = -1
            };

            temp.CurrentFrame = obj;

            Animations[AnimationType.Idle] = temp;

            _currentAnimation = Animations[AnimationType.Idle];
            BaseFrame = _currentAnimation.Frames[0];

            if (bounds == null)
            {
                Bounds = new Bounds(BaseFrame.GetPureVertexData(), BaseFrame);
            }
            else
            {
                Bounds = new Bounds(bounds, BaseFrame);
            }

            _dimensions = Bounds.GetDimensionData();

            SetPosition(position);
        }

        internal BaseObject() { } //don't use this for creating objects

        //sets the position using units (1 thousandth of the width/height of the screen
        internal void SetPosition(Vector3 position) 
        {
            //Position = new Vector3(Math.Clamp(position.X, 0, _windowSize.X), Math.Clamp(position.Y, 0, _windowSize.Y), 0);
            Position = new Vector3(position.X, position.Y, position.Z) + PositionalOffset;

            position = WindowConstants.ConvertGlobalToLocalCoordinates(position);

            if (LockToWindow)
            {
                position.X = Math.Clamp(position.X, -1.0f, 1.0f);
                position.Y = Math.Clamp(position.Y, -1.0f, 1.0f);
            }

            BaseFrame.SetTranslation(position);
        }

        internal void SetPosition(Vector2 position)
        {
            Position = new Vector3(Math.Clamp(position.X, 0, WindowConstants.ClientSize.X), Math.Clamp(position.Y, 0, WindowConstants.ClientSize.Y), 0);

            float X = (position.X / WindowConstants.ClientSize.X) * 2 - 1; //converts point to local opengl coordinates
            float Y = ((position.Y / WindowConstants.ClientSize.Y) * 2 - 1) * -1; //converts point to local opengl coordinates

            Vector3 newPos = new Vector3(X, Y, 0);

            if (LockToWindow) 
            {
                newPos.X = Math.Clamp(newPos.X, -1.0f, 1.0f);
                newPos.Y = Math.Clamp(newPos.Y, -1.0f, 1.0f);
            }

            BaseFrame.SetTranslation(newPos);
        }

        //uses global coordinates
        internal void MoveObject(Vector3 position)
        {
            //position.X = ((position.X / WindowConstants.ScreenUnits.X) + 1) * 2; //converts proportion of screen into global coordinates?
            //position.Y = ((position.Y / WindowConstants.ScreenUnits.Y) + 1) * 2; //converts proportion of screen into global coordinates?
            position.X = position.X / WindowConstants.ScreenUnits.X; //converts proportion of screen into global coordinates?
            position.Y = position.Y / WindowConstants.ScreenUnits.Y; //converts proportion of screen into global coordinates?

            BaseFrame.Translate(position);
        }

        internal void RemakeBounds(RenderableObject display, float[] bounds = null) 
        {
            Bounds = new Bounds(bounds, display);
            SetPosition(Position);
        }

        internal void SetAnimation(AnimationType type, Action onFinish = null) 
        {
            _currentAnimation.Reset();

            CurrentAnimationType = type;
            _currentAnimation = Animations[type];

            _currentAnimation.Reset();
            if (onFinish != null) 
            {
                _currentAnimation.OnFinish = onFinish;
            }
        }

        internal void SetAnimation(int genericType, Action onFinish = null) 
        {
            SetAnimation((AnimationType)genericType, onFinish);
        }

        internal RenderableObject GetDisplay() 
        {
            return _currentAnimation.CurrentFrame;
        }
    }

    internal class Bounds
    {
        internal float[] Vertices;
        internal RenderableObject Display;
        //the square of the radius of a sphere that can be used to quickly determine whether to do a full check of the bounds of the object
        internal float BoundingSphere;

        internal Bounds(float[] vertices, RenderableObject display, float boundingSphere = 1f) 
        {
            Vertices = vertices;
            Display = display;
            BoundingSphere = boundingSphere;
        }

        internal bool Contains(Vector2 point, Camera camera = null)
        {
            const int dimensions = 3;

            int intersections = 0;

            bool skipFinalVertex = Math.Abs(Vertices[Vertices.Length - 3]) < 0.01f && Math.Abs(Vertices[Vertices.Length - 2]) < 0.01f; //check if the last vertex is the texture anchor point (0,0)

            for (int side = 0; side < Vertices.Length / dimensions; side++)
            {
                if(side == Vertices.Length / dimensions - 1 && skipFinalVertex)
                {
                    break;
                }

                int nextVertex = side + 1;
                if (side == Vertices.Length / dimensions - (skipFinalVertex ? 2 : 1)) //if the final vertex was the texture anchor point then act as if the second to last point was the last point
                {
                    nextVertex = 0;
                }

                PointF point1 = new PointF(point.X, point.Y);
                PointF point2 = new PointF(point.X, point.Y + 1000);

                PointF point3 = GetTransformedPoint(Vertices[side * dimensions], Vertices[side * dimensions + 1], 0, camera);
                PointF point4 = GetTransformedPoint(Vertices[nextVertex * dimensions], Vertices[nextVertex * dimensions + 1], 0, camera);

                if (MiscOperations.MiscOperations.GFG.get_line_intersection(point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y))
                {
                    intersections++;
                }
            }

            if (intersections % 2 == 0)
            {
                return false;
            }

            return true;
        }

        internal bool Contains3D(Vector3 pointNear, Vector3 pointFar, Camera camera)
        {
            if(!Display.CameraPerspective)
            {
                return false;
            }

            //first get the point at the Z position of the object
            float xUnit = pointFar.X - pointNear.X;
            float yUnit = pointFar.Y - pointNear.Y;

            float percentageAlongLine = (Display.Position.Z - pointNear.Z) / (pointFar.Z - pointNear.Z);

            Vector3 pointAtZ = new Vector3(pointNear.X + xUnit * percentageAlongLine, pointNear.Y + yUnit * percentageAlongLine, Display.Position.Z);

          
            // check if the point is in the bounding sphere, if it isn't we know it won't be inside of the bounds
            if (Math.Abs(pointAtZ.X - Display.Position.X) > BoundingSphere || Math.Abs(pointAtZ.Y - Display.Position.Y) > BoundingSphere)
            {
                return false;
            }
            
            //check bounds of object
            return Contains(pointAtZ.Xy, camera);
        }

        internal Vector3 GetDimensionData() 
        {
            const int dimensions = 3;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < Vertices.Length / dimensions; i++)
            {
                int Index = i * dimensions;
                minX = Vertices[Index] < minX ? Vertices[Index] : minX;
                maxX = Vertices[Index] > maxX ? Vertices[Index] : maxX;

                minY = Vertices[Index + 1] < minY ? Vertices[Index + 1] : minY;
                maxY = Vertices[Index + 1] > maxY ? Vertices[Index + 1] : maxY;

                minZ = Vertices[Index + 2] < minZ ? Vertices[Index + 2] : minZ;
                maxZ = Vertices[Index + 2] > maxZ ? Vertices[Index + 2] : maxZ;
            }

            Vector3 returnVec = new Vector3((maxX - minX) / 2 * WindowConstants.ScreenUnits.X, (maxY - minY) / 2 * WindowConstants.ScreenUnits.Y, maxZ - minZ);

            return returnVec;
        }


        private PointF GetTransformedPoint(float x, float y, float z, Camera camera = null) 
        {
            Vector4 transform = new Vector4(x, y, z, 1);


            transform *= Display.Rotation;
            transform *= Display.Scale;
            transform *= Display.Translation;

            //Console.WriteLine("transformed coordinates: " + transform.X + ", " + transform.Y + ", " + transform.Z );

            return new PointF(transform.X, transform.Y);
        }

        internal void PrintBounds(Camera camera)
        {
            const int dimensions = 3;

            for (int side = 0; side < Vertices.Length / dimensions; side++)
            {
                int nextVertex = side + 1;
                if (side == Vertices.Length / dimensions - 1)
                {
                    nextVertex = 0;
                }

                PointF point3 = GetTransformedPoint(Vertices[side * dimensions], Vertices[side * dimensions + 1], Vertices[side * dimensions + 2], camera);
                Console.WriteLine("Point " + side + ": " + point3.X + ", " + point3.Y);
            }
        }
    }

    internal class OutlineParameters 
    {
        internal int OutlineThickness = 0;
        internal int InlineThickness = 0;
        internal Vector4 OutlineColor = Colors.Black;
        internal Vector4 InlineColor = Colors.Black;

        internal int BaseOutlineThickness = 0;
        internal int BaseInlineThickness = 0;

        /// <summary>
        /// Sets the inline thickness and base value to the thickness parameter
        /// </summary>
        /// <param name="thickness"></param>
        internal void SetAllInline(int thickness) 
        {
            InlineThickness = thickness;
            BaseInlineThickness = thickness;
        }

        /// <summary>
        /// Sets the outline thickness and base value to the thickness parameter
        /// </summary>
        /// <param name="thickness"></param>
        internal void SetAllOutline(int thickness) 
        {
            OutlineThickness = thickness;
            BaseOutlineThickness = thickness;
        }
    }

    internal class RenderData
    {
        internal float AlphaThreshold = Rendering.RenderingConstants.DefaultAlphaThreshold;

        internal static RenderData DefaultRenderData = new RenderData() { AlphaThreshold = Rendering.RenderingConstants.DefaultAlphaThreshold };
    }

}
