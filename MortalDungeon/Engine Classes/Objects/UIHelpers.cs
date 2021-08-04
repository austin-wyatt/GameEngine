using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public enum UIEventType
    {
        None,
        MouseUp,
        Hover,
        MouseDown,
        Grab,
        KeyDown,
        Focus,
        TimedHover
    }

    public enum UIAnchorPosition
    {
        Center,
        TopCenter,
        BottomCenter,
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
        LeftCenter,
        RightCenter
    }

    public static class UIHelpers
    {
        public struct UIBorders
        {
            public bool Left;
            public bool Right;
            public bool Top;
            public bool Bottom;
        }

        public const int BLOCK_WIDTH = 500;
        public const int BLOCK_HEIGHT = 500;

        public static class Borders
        {
            public static readonly UIBorders None = new UIBorders();
            public static readonly UIBorders All = new UIBorders { Bottom = true, Top = true, Left = true, Right = true };
            public static readonly UIBorders LeftOnly = new UIBorders { Left = true };
            public static readonly UIBorders RightOnly = new UIBorders { Right = true };
            public static readonly UIBorders TopOnly = new UIBorders { Top = true };
            public static readonly UIBorders BottomOnly = new UIBorders { Bottom = true };
            public static readonly UIBorders TopLeft = new UIBorders { Top = true, Left = true };
            public static readonly UIBorders BottomLeft = new UIBorders { Bottom = true, Left = true };
            public static readonly UIBorders TopRight = new UIBorders { Top = true, Right = true };
            public static readonly UIBorders BottomRight = new UIBorders { Bottom = true, Right = true };
            public static readonly UIBorders TopBottom = new UIBorders { Top = true, Bottom = true };

            public static readonly UIBorders OpenTop = new UIBorders { Bottom = true, Left = true, Right = true };
            public static readonly UIBorders OpenLeft = new UIBorders { Bottom = true, Top = true, Right = true };
            public static readonly UIBorders OpenBottom = new UIBorders { Top = true, Left = true, Right = true };
            public static readonly UIBorders OpenRight = new UIBorders { Bottom = true, Top = true, Left = true };
        }

        public static readonly Texture UI_BACKGROUND = Texture.LoadFromFile("Resources/FogTexture.png");

        public static readonly Vector3 BaseMargin = new Vector3(10, 10, 0);
        public static readonly Vector3 BaseVerticalMargin = new Vector3(0, 10, 0);
        public static readonly Vector3 BaseHorizontalMargin = new Vector3(10, 0, 0);
    }

    public class UIDimensions
    {
        public Vector2 _dimensions;
        public float X { get { return _dimensions.X; } set { _dimensions.X = value; } }
        public float Y { get { return _dimensions.Y; } set { _dimensions.Y = value; } }

        public static UIDimensions operator +(UIDimensions a, UIDimensions b) => new UIDimensions(a.X + b.X, a.Y + b.Y);
        public static UIDimensions operator -(UIDimensions a, UIDimensions b) => new UIDimensions(a.X - b.X, a.Y - b.Y);
        public static UIDimensions operator *(UIDimensions a, float f) => new UIDimensions(a.X * f, a.Y * f);
        public static UIDimensions operator /(UIDimensions a, float f) => new UIDimensions(a.X / f, a.Y / f);

        public static implicit operator UIScale(UIDimensions self)
        {
            return self.ToScale();
        }

        public static implicit operator Vector3(UIDimensions self)
        {
            return new Vector3(self.X, self.Y, 0);
        }

        public UIDimensions()
        {
            _dimensions = new Vector2();
        }

        public UIDimensions(Vector2 dimensions)
        {
            _dimensions = dimensions;
        }

        public UIDimensions(Vector3 dimensions)
        {
            _dimensions = new Vector2(dimensions.X, dimensions.Y);
        }

        public UIDimensions(float x, float y)
        {
            _dimensions = new Vector2(x, y);
        }

        public UIScale ToScale()
        {
            return new UIScale(_dimensions.X / WindowConstants.ScreenUnits.X, _dimensions.Y / WindowConstants.ScreenUnits.Y);
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }
    }

    public class UIScale
    {
        public Vector2 _scale;
        public float X { get { return _scale.X; } set { _scale.X = value; } }
        public float Y { get { return _scale.Y; } set { _scale.Y = value; } }


        public static UIScale operator +(UIScale a, UIScale b) => new UIScale(a.X + b.X, a.Y + b.Y);
        public static UIScale operator -(UIScale a, UIScale b) => new UIScale(a.X - b.X, a.Y - b.Y);
        public static UIScale operator *(UIScale a, float f) => new UIScale(a.X * f, a.Y * f);
        public static UIScale operator /(UIScale a, float f) => new UIScale(a.X / f, a.Y / f);

        public static implicit operator UIDimensions(UIScale self)
        {
            return self.ToDimensions();
        }

        public UIScale()
        {
            _scale = new Vector2();
        }

        public UIScale(Vector2 scale)
        {
            _scale = scale;
        }

        public UIScale(UIScale scale)
        {
            _scale = new Vector2(scale.X, scale.Y);
        }

        public UIScale(float x, float y)
        {
            _scale = new Vector2(x, y);
        }

        public UIDimensions ToDimensions()
        {
            return new UIDimensions(_scale.X * WindowConstants.ScreenUnits.X, _scale.Y * WindowConstants.ScreenUnits.Y);
        }

        public static float CoordToScale(float coord)
        {
            return coord / WindowConstants.ScreenUnits.X;
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }
    }

    public class BoundingArea
    {
        public float MinX = 0;
        public float MaxX = 0;
        public float MinY = 0;
        public float MaxY = 0;

        public bool InBoundingArea(Vector3 position) 
        {
            return !(position.X < MinX || position.X > MaxX || position.Y > MinY || position.Y < MaxY);
        }

        public void UpdateBoundingArea(float minX, float maxX, float minY, float maxY) 
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }
    }
}
