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
            OpenTK.Mathematics.Vector2i coords = FeatureEquation.PointToMapCoords(tilePoint);

            X = coords.X;
            Y = coords.Y;

            _visited = false;
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

        /// <summary>
        /// Returns whether the given point is contained within the polygon formed by the passed list of points
        /// </summary>
        /// <param name="points">The bounding points sorted to create a perimeter</param>
        /// <param name="point">The point to test</param>
        /// <returns></returns>
        //public static bool BoundsContains(List<FeaturePoint> points, Vector2i point)
        //{
        //    int intersections = 0;

        //    for (int side = 0; side < points.Count; side++)
        //    {
        //        int nextSide = (side + 1) % points.Count;

        //        if (MiscOperations.GFG.GetLinesIntersect(new Vector2(point.X, point.Y), new Vector2(point.X + 1000, point.Y + 100000),
        //            new Vector2(points[side].X, points[side].Y), new Vector2(points[nextSide].X, points[nextSide].Y)))
        //        {
        //            intersections++;
        //        }
        //    }

        //    if (intersections % 2 == 0)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

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
