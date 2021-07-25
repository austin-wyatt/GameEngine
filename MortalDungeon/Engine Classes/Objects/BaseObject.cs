using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class BaseObject
    {
        public int ID;
        public string Name;
        public Vector3 Position; //uses global position (based off of screen width and height), use GetDisplay().Position for local coordinates 
        public Bounds Bounds;
        public Vector3 PositionalOffset = new Vector3();

        public Dictionary<AnimationType, Animation> Animations = new Dictionary<AnimationType, Animation>();
        public AnimationType CurrentAnimationType = AnimationType.Idle; //static textures will use the idle animation


        public bool LockToWindow = false;
        public bool Render = true;
        public bool Clickable = true;
        private Vector3 _dimensions;
        public Animation _currentAnimation;
        public Vector3 _localSpacePosition = new Vector3();

        public RenderableObject BaseFrame;

        public OutlineParameters OutlineParameters = new OutlineParameters();

        public Vector3 Dimensions 
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

        public BaseObject(List<Animation> animations, int id, string name, Vector3 position, float[] bounds = null) 
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

            _localSpacePosition = new Vector3(position);

            SetPosition(position);
        }

        public BaseObject() { } //don't use this for creating objects

        //sets the position using units (1 thousandth of the width/height of the screen
        public void SetPosition(Vector3 position) 
        {
            //Position = new Vector3(Math.Clamp(position.X, 0, _windowSize.X), Math.Clamp(position.Y, 0, _windowSize.Y), 0);
            Position = new Vector3(position.X, position.Y, position.Z) + PositionalOffset;

            position = WindowConstants.ConvertGlobalToLocalCoordinates(position);

            if (LockToWindow)
            {
                position.X = Math.Clamp(position.X, -1.0f, 1.0f);
                position.Y = Math.Clamp(position.Y, -1.0f, 1.0f);
            }

            _localSpacePosition = new Vector3(position);

            BaseFrame.SetTranslation(position);
        }

        public void SetPosition(Vector2 position)
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
        public void MoveObject(Vector3 position)
        {
            //position.X = ((position.X / WindowConstants.ScreenUnits.X) + 1) * 2; //converts proportion of screen into global coordinates?
            //position.Y = ((position.Y / WindowConstants.ScreenUnits.Y) + 1) * 2; //converts proportion of screen into global coordinates?
            position.X = position.X / WindowConstants.ScreenUnits.X; //converts proportion of screen into global coordinates?
            position.Y = position.Y / WindowConstants.ScreenUnits.Y; //converts proportion of screen into global coordinates?

            BaseFrame.Translate(position);
        }

        public void RemakeBounds(RenderableObject display, float[] bounds = null) 
        {
            Bounds = new Bounds(bounds, display);
            SetPosition(Position);
        }

        public void SetAnimation(AnimationType type, Action onFinish = null) 
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

        public void SetAnimation(int genericType, Action onFinish = null) 
        {
            SetAnimation((AnimationType)genericType, onFinish);
        }

        public RenderableObject GetDisplay() 
        {
            return _currentAnimation.CurrentFrame;
        }
    }

    public class Bounds
    {
        public float[] Vertices;
        public RenderableObject Display;
        //the square of the radius of a sphere that can be used to quickly determine whether to do a full check of the bounds of the object
        public float BoundingSphere;

        public Bounds(float[] vertices, RenderableObject display, float boundingSphere = 1f) 
        {
            Vertices = vertices;
            Display = display;
            BoundingSphere = boundingSphere;
        }

        public bool Contains(Vector2 point, Camera camera = null)
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

        public bool Contains3D(Vector3 pointNear, Vector3 pointFar, Camera camera)
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

        public Vector3 GetDimensionData() 
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

        public void PrintBounds(Camera camera)
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

    public class OutlineParameters 
    {
        public int OutlineThickness = 0;
        public int InlineThickness = 0;
        public Vector4 OutlineColor = Colors.Black;
        public Vector4 InlineColor = Colors.Black;

        public int BaseOutlineThickness = 0;
        public int BaseInlineThickness = 0;

        /// <summary>
        /// Sets the inline thickness and base value to the thickness parameter
        /// </summary>
        /// <param name="thickness"></param>
        public void SetAllInline(int thickness) 
        {
            InlineThickness = thickness;
            BaseInlineThickness = thickness;
        }

        /// <summary>
        /// Sets the outline thickness and base value to the thickness parameter
        /// </summary>
        /// <param name="thickness"></param>
        public void SetAllOutline(int thickness) 
        {
            OutlineThickness = thickness;
            BaseOutlineThickness = thickness;
        }
    }
}
