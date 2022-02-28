using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public enum BoundingPointTypes
    {
        Trees = 1,
        Dirt = 3,
        HeightChange = 5,
        Stone = 6,
        Fill = 7,
    }

    [Serializable]
    public class BoundingPoints : ISerializable
    {
        public List<Vector3i> CubePoints = new List<Vector3i>();

        [XmlIgnore]
        public List<FeaturePoint> OffsetPoints = new List<FeaturePoint>();

        [XmlIgnore]
        public List<FeaturePoint> BoundingSquare = new List<FeaturePoint>();

        /// <summary>
        /// What "style" to apply to the inside of the bounding points. This would be like generic forest, generic desert, etc.
        /// If the value is 0 then no style will be applied.
        /// </summary>
        public int BoundingPointsId = 0;

        public bool SubscribeToEntrance = false;

        [XmlIgnore]
        public Dictionary<TileMapPoint, HashSet<MapBrushPoint>> AffectedPoints = new Dictionary<TileMapPoint, HashSet<MapBrushPoint>>();

        [XmlIgnore]
        public HashSet<TileMapPoint> AffectedMaps = new HashSet<TileMapPoint>();

        /// <summary>
        /// Whether these bounding points should be applied to already loaded tile maps.
        /// Defaults to false.
        /// </summary>
        public bool ApplyToStaleMaps = false;


        [XmlIgnore]
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [XmlElement(Namespace = "BPp")]
        public DeserializableDictionary<string, string> _parameters = new DeserializableDictionary<string, string>();

        public void OnLoad(int featureId)
        {
            ConsistentRandom rng = new ConsistentRandom(featureId);

            double randomVal;
            double density;
            int height;

            Vector2i affectedPoint = new Vector2i();

            foreach (var map in AffectedMaps)
            {
                for(int i = 0; i < TileMapManager.TILE_MAP_DIMENSIONS.X; i++)
                {
                    for (int j = 0; j < TileMapManager.TILE_MAP_DIMENSIONS.Y; j++)
                    {
                        affectedPoint.X = map.X * TileMapManager.TILE_MAP_DIMENSIONS.X + i;
                        affectedPoint.Y = map.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y + j;

                        if (OffsetPoints.Count > 2 && FeaturePoint.PointInPolygon(OffsetPoints, affectedPoint))
                        {
                            switch ((BoundingPointTypes)BoundingPointsId)
                            {
                                case BoundingPointTypes.Dirt:
                                    AddAffectedPoint(map, new MapBrushPoint()
                                    {
                                        X = i,
                                        Y = j,
                                    });
                                    break;
                                case BoundingPointTypes.Trees:
                                    density = 0.1;
                                    if (Parameters.TryGetValue("density", out var val))
                                    {
                                        if (double.TryParse(val, out var d))
                                        {
                                            density = d;
                                        }
                                    }

                                    if (rng.NextDouble() < density)
                                    {
                                        AddAffectedPoint(map, new MapBrushPoint()
                                        {
                                            X = i,
                                            Y = j,
                                            Value = rng.Next() % 2
                                        });
                                    }
                                    break;
                                case BoundingPointTypes.HeightChange:
                                    height = 0;
                                    if (Parameters.TryGetValue("height", out var tileHeight))
                                    {
                                        if (int.TryParse(tileHeight, out var d))
                                        {
                                            height = d;
                                        }
                                    }

                                    AddAffectedPoint(map, new MapBrushPoint()
                                    {
                                        X = i,
                                        Y = j,
                                        Value = height
                                    });
                                    break;
                                case BoundingPointTypes.Stone:
                                    if (rng.NextDouble() > 0.5)
                                    {
                                        AddAffectedPoint(map, new MapBrushPoint()
                                        {
                                            X = i,
                                            Y = j,
                                            Value = 0
                                        });
                                    }
                                    else
                                    {
                                        AddAffectedPoint(map, new MapBrushPoint()
                                        {
                                            X = i,
                                            Y = j,
                                            Value = 1
                                        });
                                    }
                                    break;
                                case BoundingPointTypes.Fill:
                                    AddAffectedPoint(map, new MapBrushPoint()
                                    {
                                        X = i,
                                        Y = j,
                                    });
                                    break;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
        }

        public void ApplyToMap(TileMap map)
        {
            double density;
            int height;

            if (AffectedPoints.TryGetValue(map.TileMapCoords, out var points))
            {
                foreach (var point in points)
                {
                    var tile = map.GetLocalTile(point.X, point.Y);

                    if (tile == null)
                        continue;

                    switch ((BoundingPointTypes)BoundingPointsId)
                    {
                        case BoundingPointTypes.Dirt:
                            tile.Properties.Type = TileType.Dirt;
                            break;
                        case BoundingPointTypes.Trees:
                            var tree = new Tree(tile.TileMap, tile, point.Value);
                            break;
                        case BoundingPointTypes.HeightChange:
                            if (point.Value > 0)
                            {
                                tile.Properties.Height = point.Value;

                                Vector3 pos = new Vector3(tile.Position.X, tile.Position.Y, 0);

                                tile.SetPosition(pos + new Vector3(0, 0, point.Value * 0.2f));
                            }
                            break;
                        case BoundingPointTypes.Stone:
                            if (point.Value == 0)
                            {
                                tile.Properties.Type = TileType.Stone_1;
                            }
                            else if(point.Value == 1)
                            {
                                tile.Properties.Type = TileType.Stone_2;
                            }
                            break;

                        case BoundingPointTypes.Fill:
                            tile.Properties.Type = TileType.Fill;

                            if (point.Value == 0)
                            {
                                tile.SetColor(_Colors.LightBlue);
                            }
                            //else if (rng > 0.25)
                            //{
                            //    tile.SetColor(_Colors.LightBlue + new Vector4(0, -0.05f, 0, 0));
                            //}
                            //else
                            //{
                            //    tile.SetColor(_Colors.LightBlue + new Vector4(0, -0.1f, 0, 0));
                            //}

                            break;
                        default:
                            continue;
                    }
                }
            }

            map.UpdateTile();
        }

        private void AddAffectedPoint(TileMapPoint mapPoint, MapBrushPoint affectedPoint)
        {
            if (AffectedPoints.TryGetValue(mapPoint, out var set))
            {
                set.Add(affectedPoint);
            }
            else
            {
                HashSet<MapBrushPoint> brushSet = new HashSet<MapBrushPoint> { affectedPoint };
                AffectedPoints.Add(mapPoint, brushSet);
            }
        }

        public void PrepareForSerialization()
        {
            _parameters = new DeserializableDictionary<string, string>(Parameters);
        }

        public void CompleteDeserialization()
        {
            _parameters.FillDictionary(Parameters);
        }
    }
}
