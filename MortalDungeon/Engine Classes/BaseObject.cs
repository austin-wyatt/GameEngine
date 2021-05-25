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
        private Vector2i _windowSize;
        public int ID;
        public string Name;
        public RenderableObject Display;
        public Vector3 Position; //uses global position (based off of screen width and height), use Display.Position for local coordinates 
        public Bounds Bounds;

        public bool LockToWindow = false;


        public BaseObject(Vector2i windowSize, RenderableObject display, int id, string name, Vector3 position, float[] bounds = null) 
        {
            _windowSize = windowSize;
            ID = id;
            Name = name;
            Display = display;
            Position = new Vector3(position);

            if(bounds == null)
            {
                Bounds = new Bounds(display.GetPureVertexData(), display, windowSize);
            }
            else
            {
                Bounds = new Bounds(bounds, display, windowSize);
            }

            SetPosition(position);
        }

        //sets the position using units (1 thousandth of the width/height of the screen
        public void SetPosition(Vector3 position) 
        {
            //Position = new Vector3(Math.Clamp(position.X, 0, _windowSize.X), Math.Clamp(position.Y, 0, _windowSize.Y), 0);
            Position = new Vector3(position.X, position.Y, position.Z);

            float X = (position.X / 1000) * 2 - 1; //converts point to local opengl coordinates
            float Y = ((position.Y / 1000) * 2 - 1) * -1; //converts point to local opengl coordinates

            //float X = (position.X / _windowSize.X) * 2 - 1; //converts point to local opengl coordinates
            //float Y = ((position.Y / _windowSize.Y) * 2 - 1) * -1; //converts point to local opengl coordinates

            position.X = X;
            position.Y = Y;

            if (LockToWindow)
            {
                position.X = Math.Clamp(position.X, -1.0f, 1.0f);
                position.Y = Math.Clamp(position.Y, -1.0f, 1.0f);
            }

            Display.SetTranslation(position);
        }

        public void SetPosition(Vector2 position)
        {
            Position = new Vector3(Math.Clamp(position.X, 0, _windowSize.X), Math.Clamp(position.Y, 0, _windowSize.Y), 0);

            float X = (position.X / _windowSize.X) * 2 - 1; //converts point to local opengl coordinates
            float Y = ((position.Y / _windowSize.Y) * 2 - 1) * -1; //converts point to local opengl coordinates

            Vector3 newPos = new Vector3(X, Y, 0);

            if (LockToWindow) 
            {
                newPos.X = Math.Clamp(newPos.X, -1.0f, 1.0f);
                newPos.Y = Math.Clamp(newPos.Y, -1.0f, 1.0f);
            }

            Display.SetTranslation(newPos);
        }

        //uses global coordinates
        public void MoveObject(Vector3 position)
        {
            position.X = (position.X / 1000); //converts global coordinates into a proportion of the screen
            position.Y = (position.Y / 1000); //converts global coordinates into a proportion of the screen

            //position.X = (position.X / _windowSize.X); //converts global coordinates into a proportion of the screen
            //position.Y = (position.Y / _windowSize.Y); //converts global coordinates into a proportion of the screen

            Display.Translate(position);
        }

        public void RemakeBounds(RenderableObject display, float[] bounds = null) 
        {
            Bounds = new Bounds(bounds, display, _windowSize);
            SetPosition(Position);
        }


        public Action<BaseObject> OnClick;
        //public virtual void OnClick() 
        //{
        //    Console.WriteLine("Object " + Name + " clicked.");
        //}
    }

    public class Bounds
    {
        public float[] Vertices;
        public RenderableObject Display;
        private Vector2i _windowSize;
        //the square of the radius of a sphere that can be used to quickly determine whether to do a full check of the bounds of the object
        public float BoundingSphere;

        public Bounds(float[] vertices, RenderableObject display, Vector2i windowSize, float boundingSphere = 1f) 
        {
            Vertices = vertices;
            Display = display;
            _windowSize = windowSize;
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


}
