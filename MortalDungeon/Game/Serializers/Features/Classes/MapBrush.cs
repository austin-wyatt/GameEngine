using Empyrean.Engine_Classes;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public enum MapBrushType
    {
        Dirt,
        Trees //Parameters: density (double, 0.1)
    }


    public struct MapBrushPoint
    {
        public int X;
        public int Y;
        public int Value;
        public float fValue;

        public override bool Equals(object obj)
        {
            return obj is MapBrushPoint point &&
                   X == point.X &&
                   Y == point.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public long GetUniqueHash()
        {
            return ((long)X << 32) + Y;
        }
    }
    
    [XmlType(TypeName = "feMB")]
    [Serializable]
    public class MapBrush : ISerializable
    {
        /// <summary>
        /// Map Coordinates X
        /// </summary>
        public int X;
        /// <summary>
        /// Map Coordinates Y
        /// </summary>
        public int Y;

        public int BrushId;

        public ParameterDict Parameters = new ParameterDict();

        [XmlIgnore]
        public List<MapBrushPoint> AffectedPoints = new List<MapBrushPoint>();

        public MapBrush() { }

        public MapBrush(MapBrush brushToCopy)
        {
            BrushId = brushToCopy.BrushId;
            Parameters.Parameters.Clear();

            foreach(var parameter in brushToCopy.Parameters.Parameters)
            {
                Parameters.Parameters.Add(parameter.Key, parameter.Value);
            }
        }

        public void OnLoaded(int featureId)
        {
            ConsistentRandom rng = new ConsistentRandom(featureId + X + Y);

            double density;

            //fill affected points if applicable
            switch ((MapBrushType)BrushId)
            {
                case MapBrushType.Trees:
                    //fill affected points here
                    density = 0.1;
                    if (Parameters.TryGetValue("density", out var val))
                    {
                        if (double.TryParse(val, out var d))
                        {
                            density = d;
                        }
                    }

                    for(int i = 0; i < TileMapManager.TILE_MAP_DIMENSIONS.X; i++)
                    {
                        for(int j = 0; j < TileMapManager.TILE_MAP_DIMENSIONS.Y; j++)
                        {
                            if (rng.NextDouble() < density)
                            {
                                AffectedPoints.Add(new MapBrushPoint()
                                {
                                    X = i,
                                    Y = j,
                                    Value = rng.Next() % 2
                                });
                            }
                        }
                    }
                    

                    break;
                default:
                    return;
            }
        }

        public void ApplyToMap(TileMap map)
        {
            switch ((MapBrushType)BrushId)
            {
                case MapBrushType.Dirt:
                    for(int i = 0; i < map.Tiles.Count; i++)
                    {
                        map.Tiles[i].Properties.SetType(TileType.Dirt, fromFeature: true);
                    }
                    break;
                case MapBrushType.Trees:
                    for(int i = 0; i < AffectedPoints.Count; i++)
                    {
                        var tile = map.GetLocalTile(AffectedPoints[i].X, AffectedPoints[i].Y);

                        ConsistentRandom rng = new ConsistentRandom(AffectedPoints[i].X + AffectedPoints[i].Y 
                            + map.TileMapCoords.X + map.TileMapCoords.Y);
                        var tree = new Tree(tile.TileMap, tile, AffectedPoints[i].Value, 1 + (float)rng.NextDouble() / 2);
                    }
                    break;
                default:
                    return;
            }
        }

        public void CompleteDeserialization()
        {
            Parameters.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            Parameters.PrepareForSerialization();
        }

        public override bool Equals(object obj)
        {
            return obj is MapBrush brush &&
                   X == brush.X &&
                   Y == brush.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public class BlendParameters
    {
        //calculate anchor point when loading and calculate the distance
        //between the anchor and the tile when applying the brush to the map
    }
}
