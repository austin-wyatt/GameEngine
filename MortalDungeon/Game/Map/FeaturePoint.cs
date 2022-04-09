using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Map
{
    [XmlType(TypeName = "FP")]
    [Serializable]
    public struct FeaturePoint
    {
        public int X;
        public int Y;


        [XmlElement("FPv")]
        public bool _visited;

        public FeaturePoint(int x, int y)
        {
            X = x;
            Y = y;

            _visited = false;
        }

        public FeaturePoint(TilePoint tilePoint)
        {
            Vector2i coords = FeatureEquation.PointToMapCoords(tilePoint);

            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public FeaturePoint(Tile tile)
        {
            this = new FeaturePoint(tile.TilePoint);
        }

        public FeaturePoint(BaseTile tile)
        {
            this = new FeaturePoint(tile.TilePoint);
        }

        public FeaturePoint(Vector2i coords)
        {
            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public FeaturePoint(FeaturePoint coords)
        {
            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public static bool operator ==(FeaturePoint a, FeaturePoint b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(FeaturePoint a, FeaturePoint b) => !(a == b);

        public static FeaturePoint operator -(FeaturePoint a, FeaturePoint b) => new FeaturePoint(a.X - b.X, a.Y - b.Y);
        public static FeaturePoint operator +(FeaturePoint a, FeaturePoint b) => new FeaturePoint(a.X + b.X, a.Y + b.Y);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public long GetUniqueHash()
        {
            return ((long)X << 32) + Y;
        }

        public override string ToString()
        {
            return $"{{{X}, {Y}}}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public Vector2i ToVector2i()
        {
            return new Vector2i(X, Y);
        }

        public TileMapPoint ToTileMapPoint()
        {
            return FeatureEquation.FeaturePointToTileMapCoords(this);
        }

        public TilePoint ToTilePoint()
        {
            return new TilePoint(this);
        }

        public static bool PointInPolygon(List<FeaturePoint> points, Vector2i point)
        {
            bool oddNodes = false;

            int j = points.Count - 1;

            for (int i = 0; i < points.Count; i++)
            {
                if ((float)points[i].Y < point.Y && (float)points[j].Y >= point.Y
                || (float)points[j].Y < point.Y && (float)points[i].Y >= point.Y)
                {
                    if (points[i].X + (float)((float)point.Y - points[i].Y) / ((float)points[j].Y - points[i].Y) * ((float)points[j].X - points[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }

                j = i;
            }

            return oddNodes;
        }

        public static bool PointInPolygon(List<Vector2i> points, Vector2i point)
        {
            bool oddNodes = false;

            int j = points.Count - 1;

            for (int i = 0; i < points.Count; i++)
            {
                if ((float)points[i].Y < point.Y && (float)points[j].Y >= point.Y
                || (float)points[j].Y < point.Y && (float)points[i].Y >= point.Y)
                {
                    if (points[i].X + (float)((float)point.Y - points[i].Y) / ((float)points[j].Y - points[i].Y) * ((float)points[j].X - points[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }

                j = i;
            }

            return oddNodes;
        }

        public static FeaturePoint MinPoint = new FeaturePoint(int.MinValue, int.MinValue);
    }

    public class FeaturePointWithParent
    {
        public FeaturePoint Point;
        public FeaturePoint Parent;
        public bool IsRoot;

        public FeaturePointWithParent(FeaturePoint point, FeaturePoint parent, bool isRoot = false)
        {
            Point = point;
            Parent = parent;

            IsRoot = isRoot;
        }
    }

    public enum FeatureType
    {
        None,
        Grass,
        Water_1,
        Water_2,
        Tree_1,
        Tree_2,
        StonePath
    }
}
