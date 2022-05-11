using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Map
{
    [XmlType(TypeName = "FP")]
    [Serializable]
    public struct FeaturePoint
    {
        public int X;
        public int Y;

        [XmlIgnore]
        public bool _visited;

        public static ObjectPool<List<FeaturePoint>> FeaturePointListPool = new ObjectPool<List<FeaturePoint>>();
        public static ObjectPool<FeaturePoint> FeaturePointPool = new ObjectPool<FeaturePoint>(500);

        public FeaturePoint(int x, int y)
        {
            X = x;
            Y = y;

            _visited = false;
        }

        public FeaturePoint(TilePoint tilePoint)
        {
            X = 0;
            Y = 0;

            FeatureEquation.PointToMapCoords(tilePoint, ref X, ref Y);

            _visited = false;
        }

        public FeaturePoint(Tile tile)
        {
            X = 0;
            Y = 0;

            FeatureEquation.PointToMapCoords(tile.TilePoint, ref X, ref Y);

            _visited = false;
        }

        public FeaturePoint(BaseTile tile)
        {
            X = 0;
            Y = 0;

            FeatureEquation.PointToMapCoords(tile.TilePoint, ref X, ref Y);

            _visited = false;
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

        public void Initialize(Tile tile)
        {
            FeatureEquation.PointToMapCoords(tile.TilePoint, ref X, ref Y);

            _visited = false;
        }

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
            return FeatureEquation.FeaturePointToTileMapCoords(ref this);
        }

        public Vector2i ToChunkPosition()
        {
            Vector2i chunkPosition = new Vector2i(X, Y);

            int tileMapChunkWidth = TileChunk.DefaultChunkWidth * TileMap.ChunksPerTileMap.X;
            int tileMapChunkHeight = TileChunk.DefaultChunkHeight * TileMap.ChunksPerTileMap.Y;

            if (X < 0)
            {
                chunkPosition.X = GMath.NegMod(X, tileMapChunkHeight);
            }
            if (Y < 0)
            {
                chunkPosition.Y = GMath.NegMod(Y, tileMapChunkHeight);
            }

            chunkPosition.X %= tileMapChunkWidth;
            chunkPosition.Y %= tileMapChunkHeight;

            chunkPosition.X /= TileChunk.DefaultChunkWidth;
            chunkPosition.Y /= TileChunk.DefaultChunkHeight;

            return chunkPosition;
        }

        public void ToTileMapPoint(ref TileMapPoint point)
        {
            FeatureEquation.FeaturePointToTileMapCoords(ref this, ref point);
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

        public static bool PointInPolygon(float[] points, int vertexOffset, float x, float y)
        {
            bool oddNodes = false;

            int j = points.Length - 1 - vertexOffset;

            for (int i = 0; i < points.Length; i+= vertexOffset)
            {
                if ((points[i + 1] < y) && (points[j + 1] >= y)
                || ((points[j + 1] < y) && (points[i + 1] >= y)))
                {
                    if (points[i] + (float)((float)y - points[i + 1]) / (points[j + 1] - points[i + 1]) * (points[j] - points[i]) < x)
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
