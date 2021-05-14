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
            Position = position;

            Bounds = new Bounds(bounds, display, windowSize);

            SetPosition(position);
        }

        public void SetPosition(Vector3 position) 
        {
            //Position = new Vector3(Math.Clamp(position.X, 0, _windowSize.X), Math.Clamp(position.Y, 0, _windowSize.Y), 0);
            Position = new Vector3(position.X, position.Y, position.Z);

            float X = (position.X / _windowSize.X) * 2 - 1; //converts point to local opengl coordinates
            float Y = ((position.Y / _windowSize.Y) * 2 - 1) * -1; //converts point to local opengl coordinates

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

        public void MoveObject(Vector3 position)
        {
            position.X = (position.X / _windowSize.X); //converts global coordinates into a proportion of the screen
            position.Y = (position.Y / _windowSize.Y); //converts global coordinates into a proportion of the screen

            Display.Translate(position);
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

        public Bounds(float[] vertices, RenderableObject display, Vector2i windowSize) 
        {
            Vertices = vertices;
            Display = display;
            _windowSize = windowSize;
        }

        public bool Contains(Vector2 point)
        {
            const int dimensions = 3;

            int intersections = 0;

            for (int side = 0; side < Vertices.Length / dimensions; side++)
            {
                int nextVertex = side + 1;
                if (side == Vertices.Length / dimensions - 1)
                {
                    nextVertex = 0;
                }

                PointF point1 = new PointF(point.X, point.Y);
                PointF point2 = new PointF(point.X, point.Y + 1000);

                PointF point3 = GetTransformedPoint(Vertices[side * dimensions], Vertices[side * dimensions + 1], 0);
                PointF point4 = GetTransformedPoint(Vertices[nextVertex * dimensions], Vertices[nextVertex * dimensions + 1], 0);


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

        public bool Contains(Vector3 point)
        {
            const int dimensions = 3;

            point.X = (point.X / _windowSize.X) * 2 - 1; //converts point to local opengl coordinates
            point.Y = ((point.Y / _windowSize.Y) * 2 - 1) * -1; //converts point to local opengl coordinates

            int intersections = 0;

            for (int side = 0; side < Vertices.Length / dimensions; side++)
            {
                int nextVertex = side + 1;
                if (side == Vertices.Length / dimensions - 1)
                {
                    nextVertex = 0;
                }

                PointF point1 = new PointF(point.X, point.Y);
                PointF point2 = new PointF(point.X, point.Y + 1000);

                PointF point3 = GetTransformedPoint(Vertices[side * dimensions], Vertices[side * dimensions + 1], Vertices[side * dimensions + 2]);
                PointF point4 = GetTransformedPoint(Vertices[nextVertex * dimensions], Vertices[nextVertex * dimensions + 1], Vertices[nextVertex * dimensions + 2]);


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

        private PointF GetTransformedPoint(float x, float y, float z) 
        {
            Vector4 transform = new Vector4(x, y, z, 1);

            transform *= Display.Rotation;
            transform *= Display.Scale;
            transform *= Display.Translation;

            return new PointF(transform.X, transform.Y);
        }
    }
}
