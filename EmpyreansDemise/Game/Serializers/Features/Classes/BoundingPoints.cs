using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Map;
using Empyrean.Game.Save;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public enum BoundingPointTypes
    {
        Trees = 1,
        Dirt = 3,
        HeightChange = 5,
        Stone = 6,
        Fill = 7,
    }

    public enum BoundingAnchorType
    {
        Fill
    }

    [XmlType(Namespace="_ba")]
    [Serializable]
    public class BoundingAnchor
    {
        public BoundingAnchorType Type = BoundingAnchorType.Fill;
        public Vector3i Point;
    }

    [Serializable]
    public class BoundingPoints : ISerializable
    {
        public List<Vector3i> CubePoints = new List<Vector3i>();

        public FeaturePoint Origin;

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

        [XmlIgnore]
        public FeatureEquation FeatureEquation;

        /// <summary>
        /// Whether these bounding points should be applied to already loaded tile maps.
        /// Defaults to false.
        /// </summary>
        public bool ApplyToStaleMaps = false;

        /// <summary>
        /// Anchors get applied after lines between the bounding points are drawn when creating the feature equation
        /// </summary>
        public List<BoundingAnchor> Anchors = new List<BoundingAnchor>();


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

            HashSet<Cube> wallList = new HashSet<Cube>();
            CubeMethods.GetLineLerp(CubePoints[0], CubePoints[^1], wallList);

            for (int i = 1; i < CubePoints.Count; i++)
            {
                CubeMethods.GetLineLerp(CubePoints[i - 1], CubePoints[i], wallList);
            }

            HashSet<Cube> filledPoints = new HashSet<Cube>();
            bool fillSuccessful = true;

            for (int i = 0; i < Anchors.Count; i++)
            {
                switch (Anchors[i].Type)
                {
                    case BoundingAnchorType.Fill:
                        fillSuccessful = fillSuccessful && CubeMethods.FloodFill(wallList, new Cube(Anchors[i].Point), filledPoints, 200000);
                        break;
                }
            }

            Vector3i origin = CubeMethods.OffsetToCube(Origin);

            foreach (var cube in wallList)
            {
                AddCubeToAffectedPoints(cube, ref rng, ref origin);
            }

            if (fillSuccessful)
            {
                foreach (var cube in filledPoints)
                {
                    AddCubeToAffectedPoints(cube, ref rng, ref origin);
                }
            }
        }

        public void AddCubeToAffectedPoints(Cube cube, ref ConsistentRandom rng, ref Vector3i origin)
        {
            double density;
            float height;


            FeaturePoint point = CubeMethods.CubeToFeaturePoint(cube.Point + origin);

            TileMapPoint map = point.ToTileMapPoint();
            FeaturePoint mapTopLeft = map.ToFeaturePoint();

            point.X -= mapTopLeft.X;
            point.Y -= mapTopLeft.Y;

            FeatureEquation.AffectedMaps.Add(map);
            AffectedMaps.Add(map);

            switch ((BoundingPointTypes)BoundingPointsId)
            {
                case BoundingPointTypes.Dirt:
                    AddAffectedPoint(map, new MapBrushPoint()
                    {
                        X = point.X,
                        Y = point.Y,
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
                            X = point.X,
                            Y = point.Y,
                            Value = rng.Next() % 2
                        });
                    }
                    break;
                case BoundingPointTypes.HeightChange:
                    height = 0;
                    if (Parameters.TryGetValue("height", out var tileHeight))
                    {
                        if (float.TryParse(tileHeight, out var d))
                        {
                            height = d;
                        }
                    }

                    AddAffectedPoint(map, new MapBrushPoint()
                    {
                        X = point.X,
                        Y = point.Y,
                        fValue = height
                    });
                    break;
                case BoundingPointTypes.Stone:
                    if (rng.NextDouble() > 0.5)
                    {
                        AddAffectedPoint(map, new MapBrushPoint()
                        {
                            X = point.X,
                            Y = point.Y,
                            Value = 0
                        });
                    }
                    else
                    {
                        AddAffectedPoint(map, new MapBrushPoint()
                        {
                            X = point.X,
                            Y = point.Y,
                            Value = 1
                        });
                    }
                    break;
                case BoundingPointTypes.Fill:
                    AddAffectedPoint(map, new MapBrushPoint()
                    {
                        X = point.X,
                        Y = point.Y,
                    });
                    break;
                default:
                    break;
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

                    ConsistentRandom rng = new ConsistentRandom(point.X + point.Y);

                    switch ((BoundingPointTypes)BoundingPointsId)
                    {
                        case BoundingPointTypes.Dirt:
                            tile.Properties.SetType(TileType.Dirt, fromFeature: true);
                            break;
                        case BoundingPointTypes.Trees:
                            var tree = new Tree(tile.TileMap, tile, point.Value, 1 + (float)rng.NextDouble() / 2);
                            break;
                        case BoundingPointTypes.HeightChange:
                            if (point.fValue > 0)
                            {
                                tile.SetHeight(point.fValue);
                            }
                            break;
                        case BoundingPointTypes.Stone:
                            tile.Properties.SetType(TileType.Stone_1, fromFeature: true);

                            //if (point.Value == 0)
                            //{
                            //    tile.Properties.SetType(TileType.Stone_1, fromFeature: true);
                            //}
                            //else if(point.Value == 1)
                            //{
                            //    tile.Properties.SetType(TileType.Stone_2, fromFeature: true);
                            //}

                            tile.SetHeight(tile.Properties.Height + (float)rng.NextDouble() * 0.05f + 0.5f);
                            break;

                        case BoundingPointTypes.Fill:
                            tile.Properties.SetType(TileType.Fill, fromFeature: true);

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
