using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.LuaHandling;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    /// <summary>
    /// Feature interactions are shorthand ways to set multiple FeatureStateValues.
    /// The interaction you want should go into the SpecifyFeatureInteraction ObjectHash
    /// and then the feature interaction you want to use should go into the Data slot.
    /// 
    /// Currently this is supposed to be how we specify that a unit was killed (and similar
    /// interactions) but that will instead be handled by using generic state values. To 
    /// specify that a unit has been killed, for example, the StateId is the feature id,
    /// the ObjectHash is the unit's object hash, and the Data is the FeatureInteraction
    /// for Killed.
    /// 
    /// Feature interactions that hook into the interaction functionality will specifically be negative values.
    /// 
    /// </summary>

    [XmlType(TypeName = "FeT")]
    [Serializable]
    public class Feature : ISerializable
    {
        [XmlElement("FaP")]
        public List<AffectedPoint> AffectedPoints = new List<AffectedPoint>();

        [XmlElement("FbP")]
        public List<BoundingPoints> BoundingPoints = new List<BoundingPoints>();

        public List<FeatureUnit> FeatureUnits = new List<FeatureUnit>();

        [XmlElement("FmBs")]
        public HashSet<MapBrush> MapBrushes = new HashSet<MapBrush>();

        public int Id = 0;

        //List<MapFillData>


        [XmlElement("FdN")]
        public string DescriptiveName = "";


        [XmlElement("FlR")]
        public int LoadRadius = 10;

        [XmlElement("FbS")]
        public List<SerializableBuildingSkeleton> BuildingSkeletons = new List<SerializableBuildingSkeleton>();

        //[XmlElement("Fup")]
        //public List<UnitParameter> UnitParameters = new List<UnitParameter>();


        [XmlElement("FoR")]
        public FeaturePoint Origin = new FeaturePoint();

        /// <summary>
        /// This corresponds to which feature set to apply to these affected points. <para/>
        /// Example: If this points to the BanditCamp feature then the generated feature will be a BanditCamp 
        /// with these AffectedPoints
        /// </summary>
        [XmlElement("Feft")]
        public int FeatureType = 0;

        /// <summary>
        /// These get applied when the feature is loaded. These should generally be subscribers for different events.
        /// These should also be removed when the feature is unloaded probably.
        /// </summary>
        [XmlElement("Fesv")]
        public List<Instructions> Instructions = new List<Instructions>();

        /// <summary>
        /// The priority in which the feature is loaded. Higher means loaded first.
        /// </summary>
        [XmlElement("Felp")]
        public int LoadPriority = 0;

        /// <summary>
        /// The layer that the feature is located on.
        /// </summary>
        [XmlElement("Fela")]
        public int Layer = 0;

        [XmlElement("Feasn")]
        public int AnimationSetId = 0;

        [XmlElement("Fems")]
        public int MapSize = 0;

        [XmlElement("Fents")]
        public int NameTextEntry = 0;

        [XmlElement("Fegns")]
        public HashSet<string> GroupNames = new HashSet<string>();

        public Feature() { }


        public FeatureEquation CreateFeatureEquation()
        {
            FeatureEquation featureEquation = new FeatureEquation();

            featureEquation.LoadRadius = LoadRadius;

            #region Affected Points
            foreach (var val in AffectedPoints)
            {
                var point = CubeMethods.CubeToOffset(val.Point);

                FeaturePoint newPoint = new FeaturePoint(point.X + Origin.X, point.Y + Origin.Y);

                featureEquation.AffectedPoints.TryAdd(newPoint, val.Value);
                featureEquation.AffectedMaps.Add(FeatureEquation.FeaturePointToTileMapCoords(newPoint));

                featureEquation.Parameters.Add(newPoint, val.Parameters);

                if (val.Parameters.TryGetValue("rfc", out var featureParamString))
                {
                    featureParamString = featureParamString.Replace(" ", "");

                    string[] featureParams = featureParamString.Split(",");

                    var clearParameters = new ClearParamaters();
                    clearParameters.ObjectHash = newPoint.GetUniqueHash();

                    foreach (var param in featureParams)
                    {
                        string[] finalParam = param.Split("=");

                        if(finalParam.Length == 2)
                        {
                            clearParameters.ExpectedValues.Add(finalParam[0], finalParam[1]);
                        }
                    }

                    featureEquation.ClearParamaters.Add(clearParameters);
                }

                if (val.Parameters.TryGetValue("LOAD_SCRIPT", out var script))
                {
                    LuaManager.ApplyScript(script);
                }
            }
            #endregion

            #region Unit Affected Points
            foreach (var val in FeatureUnits)
            {
                var point = CubeMethods.CubeToOffset(val.AffectedPoint.Point);

                FeaturePoint newPoint = new FeaturePoint(point.X + Origin.X, point.Y + Origin.Y);

                featureEquation.AffectedPoints.TryAdd(newPoint, val.UnitId + (int)FeatureEquationPointValues.UnitStart);
                featureEquation.AffectedMaps.Add(FeatureEquation.FeaturePointToTileMapCoords(newPoint));

                featureEquation.Parameters.Add(newPoint, val.AffectedPoint.Parameters);

                if (val.AffectedPoint.Parameters.TryGetValue("rfc", out var featureParamString))
                {
                    featureParamString = featureParamString.Replace(" ", "");

                    string[] featureParams = featureParamString.Split(",");

                    var clearParameters = new ClearParamaters();
                    clearParameters.ObjectHash = newPoint.GetUniqueHash();

                    foreach (var param in featureParams)
                    {
                        string[] finalParam = param.Split("=");

                        if (finalParam.Length == 2)
                        {
                            clearParameters.ExpectedValues.Add(finalParam[0], finalParam[1]);
                        }
                    }

                    featureEquation.ClearParamaters.Add(clearParameters);
                }

                if (val.AffectedPoint.Parameters.TryGetValue("LOAD_SCRIPT", out var script))
                {
                    LuaManager.ApplyScript(script);
                }
            }
            #endregion

            #region Bounding Points
            foreach (var bound in BoundingPoints)
            {
                TileMapPoint top = null;
                TileMapPoint bot = null;
                TileMapPoint left = null;
                TileMapPoint right = null;

                int minX = int.MinValue;
                int minY = int.MinValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;

                bound.OffsetPoints.Clear();

                foreach (var val in bound.CubePoints)
                {
                    var point = CubeMethods.CubeToOffset(val);

                    FeaturePoint newPoint = new FeaturePoint(point.X + Origin.X, point.Y + Origin.Y);

                    var tileMapPoint = FeatureEquation.FeaturePointToTileMapCoords(newPoint);

                    #region get extreme tile maps
                    if (top == null)
                    {
                        top = tileMapPoint;
                        bot = tileMapPoint;
                        left = tileMapPoint;
                        right = tileMapPoint;
                    }
                    else
                    {
                        if (tileMapPoint.X > right.X)
                        {
                            right = tileMapPoint;
                        }
                        if (tileMapPoint.X < left.X)
                        {
                            left = tileMapPoint;
                        }
                        if (tileMapPoint.Y > bot.Y)
                        {
                            bot = tileMapPoint;
                        }
                        if (tileMapPoint.Y < top.Y)
                        {
                            top = tileMapPoint;
                        }
                    }
                    #endregion

                    #region get extreme feature points
                    if (minX == int.MinValue)
                    {
                        minX = newPoint.X;
                        minY = newPoint.Y;
                        maxX = newPoint.X;
                        maxY = newPoint.Y;
                    }
                    else
                    {
                        if (newPoint.X > maxX)
                        {
                            maxX = newPoint.X;
                        }
                        if (newPoint.X < minX)
                        {
                            minX = newPoint.X;
                        }
                        if (newPoint.Y > maxY)
                        {
                            maxY = newPoint.Y;
                        }
                        if (newPoint.Y < minY)
                        {
                            minY = newPoint.Y;
                        }
                    }
                    #endregion

                    bound.OffsetPoints.Add(newPoint);
                }

                bound.BoundingSquare.Clear();

                bound.BoundingSquare.Add(new FeaturePoint(minX, maxY));
                bound.BoundingSquare.Add(new FeaturePoint(maxX, maxY));
                bound.BoundingSquare.Add(new FeaturePoint(maxX, minY));
                bound.BoundingSquare.Add(new FeaturePoint(minX, minY));

                featureEquation.BoundingPoints.Add(bound);

                //get the farthest north, south, east, and west affected maps and use those to form a bounding box
                //then walk every map inside that box and text its top left, top right, bottom left, and bottom right points
                //to see if they are within the bounding points. If they are then add to the affected maps

                if (top != null)
                {
                    int horizontalMapCount = right.X - left.X + 1;
                    int verticalMapCount = bot.Y - top.Y + 1;

                    int tileMapWidth = TileMapManager.TILE_MAP_DIMENSIONS.X;
                    int tileMapHeight = TileMapManager.TILE_MAP_DIMENSIONS.Y;

                    int stepWidth = 3;
                    int stepHeight = 3;
                    int steps = tileMapWidth / stepWidth + 1;

                    Vector2i topLeftPoint = new Vector2i(left.X * tileMapWidth, top.Y * tileMapHeight);

                    for (int x = 0; x < horizontalMapCount; x++)
                    {
                        for (int y = 0; y < verticalMapCount; y++)
                        {
                            for (int i = 0; i < steps; i++) //width of the current map
                            {
                                for (int j = 0; j < steps; j++) //height of the current map
                                {
                                    Vector2i currPoint = new Vector2i(topLeftPoint.X + x * tileMapWidth + i * stepWidth,
                                        topLeftPoint.Y + y * tileMapHeight + j * stepHeight);

                                    //check every 5 tiles to see if one is found to be inside the bounds
                                    if (FeaturePoint.PointInPolygon(bound.OffsetPoints, currPoint))
                                    {
                                        var tileMapPoint = FeatureEquation.FeaturePointToTileMapCoords(new FeaturePoint(currPoint));
                                        featureEquation.AffectedMaps.Add(tileMapPoint);
                                        bound.AffectedMaps.Add(tileMapPoint);

                                        i = steps + 1; //we've found a point on the current map inside of the bounds so break
                                        j = steps + 1; //out of the current map
                                    }
                                }
                            }
                        }
                    }
                }

                bound.OnLoad(Id);
            }
            #endregion

            #region Building Skeletons
            int skeleCount = 0;
            foreach(var prototypeSkele in BuildingSkeletons)
            {
                var newSkeleton = new SerializableBuildingSkeleton(prototypeSkele);
                newSkeleton.Prototype = prototypeSkele;

                featureEquation.BuildingSkeletons.Add(newSkeleton);

                Building building = newSkeleton.CreateBuilding();
                building.InitializeVisualComponent();
                building.InitializeUnitInfo();

                newSkeleton.Handle = building;

                foreach (var cubeCoord in building.TilePattern)
                {
                    var point = CubeMethods.CubeToOffset(cubeCoord + newSkeleton.IdealCenter + CubeMethods.OffsetToCube(Origin));

                    FeaturePoint newPoint = new FeaturePoint(point);

                    featureEquation.AffectedPoints.TryAdd(newPoint, (int)FeatureEquationPointValues.BuildingStart + skeleCount);
                    featureEquation.AffectedMaps.Add(FeatureEquation.FeaturePointToTileMapCoords(newPoint));
                }

                skeleCount++;
            }
            #endregion

            #region Map Brushes
            Vector2i minBrush = new Vector2i(int.MaxValue, int.MaxValue);
            Vector2i maxBrush = new Vector2i(int.MinValue, int.MinValue);

            var originMapPoint = Origin.ToTileMapPoint();

            foreach (var brush in MapBrushes)
            {
                featureEquation.MapBrushes.TryAdd(new TileMapPoint(brush.X, brush.Y), brush);
                brush.OnLoaded(Id);

                if(brush.X - originMapPoint.X < minBrush.X)
                {
                    minBrush.X = brush.X - originMapPoint.X;
                }
                if (brush.Y - originMapPoint.Y < minBrush.Y)
                {
                    minBrush.Y = brush.Y - originMapPoint.Y;
                }
                if (brush.X - originMapPoint.X > maxBrush.X)
                {
                    maxBrush.X = brush.X - originMapPoint.X;
                }
                if (brush.Y - originMapPoint.Y > maxBrush.Y)
                {
                    maxBrush.Y = brush.Y - originMapPoint.Y;
                }
            }

            var brushLoadRadius = Math.Max(maxBrush.X - minBrush.X, maxBrush.Y - minBrush.Y) * TileMapManager.TILE_MAP_DIMENSIONS.X;

            if(brushLoadRadius > LoadRadius)
            {
                LoadRadius = brushLoadRadius;
            }
            #endregion

            featureEquation.FeatureTemplate = FeatureType;

            featureEquation.Instructions = Instructions;

            featureEquation.Origin = Origin;

            featureEquation.FeatureID = Id;

            featureEquation.Layer = Layer;

            featureEquation.DescriptiveName = DescriptiveName;

            featureEquation.LoadPriority = LoadPriority;

            featureEquation.NumberGen = new ConsistentRandom(new ConsistentRandom(Origin.X).Next() + new ConsistentRandom(Origin.Y).Next());

            featureEquation.NameTextEntry = NameTextEntry;

            if (featureEquation.CheckCleared())
            {
                FeatureLedger.SetFeatureStateValue(Id, FeatureStateValues.Cleared, 1);
            }

            return featureEquation;
        }

        //public void ApplyFeatureParams(HashSet<string> parameters, AffectedPoint point, FeaturePoint appliedPoint)
        //{
        //    if (parameters.Contains("rfc")) //required for clear
        //    {

        //    }
        //}

        public void CalculateLoadRadius()
        {
            Vector2i minPoint = new Vector2i(int.MaxValue, int.MaxValue);
            Vector2i maxPoint = new Vector2i(int.MinValue, int.MinValue);

            #region Bounding points
            minPoint = new Vector2i(int.MaxValue, int.MaxValue);
            maxPoint = new Vector2i(int.MinValue, int.MinValue);

            foreach (var boundingPoint in BoundingPoints)
            {
                foreach(var point in boundingPoint.CubePoints)
                {
                    Vector2i offset = CubeMethods.CubeToOffset(point + CubeMethods.OffsetToCube(Origin));

                    if(offset.X < minPoint.X)
                    {
                        minPoint.X = offset.X;
                    }
                    if (offset.X > maxPoint.X)
                    {
                        maxPoint.X = offset.X;
                    }
                    if (offset.Y < minPoint.Y)
                    {
                        minPoint.Y = offset.Y;
                    }
                    if (offset.Y > maxPoint.Y)
                    {
                        maxPoint.Y = offset.Y;
                    }
                }
            }

            var boundingPointLoadRadius = Math.Max(maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y);

            if (boundingPointLoadRadius > LoadRadius)
            {
                LoadRadius = boundingPointLoadRadius;
            }
            #endregion

            #region Map brushes
            Vector2i minBrush = new Vector2i(int.MaxValue, int.MaxValue);
            Vector2i maxBrush = new Vector2i(int.MinValue, int.MinValue);

            var originMapPoint = Origin.ToTileMapPoint();

            foreach (var brush in MapBrushes)
            {
                if (brush.X - originMapPoint.X < minBrush.X)
                {
                    minBrush.X = brush.X - originMapPoint.X;
                }
                if (brush.Y - originMapPoint.Y < minBrush.Y)
                {
                    minBrush.Y = brush.Y - originMapPoint.Y;
                }
                if (brush.X - originMapPoint.X > maxBrush.X)
                {
                    maxBrush.X = brush.X - originMapPoint.X;
                }
                if (brush.Y - originMapPoint.Y > maxBrush.Y)
                {
                    maxBrush.Y = brush.Y - originMapPoint.Y;
                }
            }

            var brushLoadRadius = Math.Max(maxBrush.X - minBrush.X, maxBrush.Y - minBrush.Y) * TileMapManager.TILE_MAP_DIMENSIONS.X;

            if (brushLoadRadius > LoadRadius)
            {
                LoadRadius = brushLoadRadius;
            }
            #endregion
        }
        public static long HashCoordinates(int x, int y)
        {
            long val = ((long)x << 32) + y;
            return val;
        }

        public static Vector2i UnhashCoordinates(long hashedCoords)
        {
            Vector2i coords = new Vector2i();

            coords.X = (int)(hashedCoords >> 32);

            long val = (long)coords.X << 32;
            coords.Y = (int)(hashedCoords - val);

            return coords;
        }

        public void PrepareForSerialization()
        {
            foreach (var item in AffectedPoints)
            {
                item.PrepareForSerialization();
            }

            foreach (var point in BoundingPoints)
            {
                point.PrepareForSerialization();
            }

            foreach (var item in FeatureUnits)
            {
                item.PrepareForSerialization();
            }

            foreach(var item in MapBrushes)
            {
                item.PrepareForSerialization();
            }

            //_affectedPoints = new DeserializableDictionary<Vector3i, int>(AffectedPoints);
        }

        public void CompleteDeserialization()
        {
            //_affectedPoints.FillDictionary(AffectedPoints);
            //_affectedPoints = new DeserializableDictionary<Vector3i, int>();

            foreach (var item in AffectedPoints)
            {
                item.CompleteDeserialization();
            }

            foreach (var point in BoundingPoints)
            {
                point.CompleteDeserialization();
            }

            foreach (var item in FeatureUnits)
            {
                item.CompleteDeserialization();
            }

            foreach (var item in MapBrushes)
            {
                item.CompleteDeserialization();
            }
        }
    }


    [XmlType(TypeName = "SBS")]
    [Serializable]
    public class SerializableBuildingSkeleton
    {
        /// <summary>
        /// The offset from the origin of the feature
        /// </summary>
        [XmlElement("SbC")]
        public Vector3i IdealCenter;

        [XmlElement("SbR")]
        public int Rotations;

        [XmlElement("SbF")]
        public int BuildingID;

        [XmlElement("Sbna")]
        public string DescriptiveName;

        [XmlElement("Sbsv")]
        public List<Instructions> Instructions = new List<Instructions>();

        [XmlIgnore]
        public bool Loaded;

        [XmlIgnore]
        public Building Handle;

        [XmlIgnore]
        public SerializableBuildingSkeleton Prototype;

        /// <summary>
        /// True when the skeleton is acted upon during application of a feature equation until OnAppliedToMaps is called 
        /// </summary>
        [XmlIgnore]
        public bool _skeletonTouchedThisCycle;

        public SerializableBuildingSkeleton() { }

        public SerializableBuildingSkeleton(SerializableBuildingSkeleton building)
        {
            BuildingID = building.BuildingID;
            Rotations = building.Rotations;
            DescriptiveName = building.DescriptiveName;
            IdealCenter = building.IdealCenter;
            Instructions = building.Instructions;

        }

        public SerializableBuildingSkeleton(Building building)
        {
            BuildingID = building.ID;
            Rotations = building.Rotations;
            DescriptiveName = building.Name;
        }

        public Building CreateBuilding()
        {
            var building = BuildingManager.GetBuildingByID(BuildingID);
            building.RotateTilePattern(Rotations);

            return building;
        }
    }
}
