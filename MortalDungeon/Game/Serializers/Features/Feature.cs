using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public enum FeatureStateValues : long
    {
        Cleared = long.MaxValue,
        AvailableToClear = long.MaxValue - 1,
        NormalKillRequirements = long.MaxValue - 2,
        BossKillRequirements = long.MaxValue - 3,
        LootInteractionRequirements = long.MaxValue - 4,
        SpecialInteractionRequirements = long.MaxValue - 5,

        Discovered = long.MaxValue - 25,
        Explored = long.MaxValue - 26,
        TimeCleared = long.MaxValue - 27,

        NormalKillCount = long.MaxValue - 50,
        BossKillCount = long.MaxValue - 51,
        LootInteractionCount = long.MaxValue - 52,
        SpecialInteractionCount = long.MaxValue - 53,

        SpecifyFeatureInteraction = long.MaxValue - 100,
    }

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
    public enum FeatureInteraction
    {
        Refreshed = -8,
        Cleared = -7,
        Killed = -6,
        Revived = -5,
        Explored = -4,
        KilledBoss = -3,
        KilledSummon = -2,
        Entered = -1,
        None = 0,
    }

    [XmlType(TypeName = "FeT")]
    [Serializable]
    public class Feature
    {
        /// <summary>
        /// Values over certain offsets would determine what is going on these points. 
        /// Example: A value of 100000 might be the unit offset and that 100005 would mean to take the unit with id 5
        /// (still not sure if this is how it should work)
        /// 
        /// These should be offsets from the Origin point. When unpacking the feature in the engine the corrected 
        /// AffectedPoints will be applied to the feature.
        /// </summary>
        [XmlIgnore]
        public Dictionary<Vector3i, int> AffectedPoints = new Dictionary<Vector3i, int>();

        [XmlElement("FaP")]
        public DeserializableDictionary<Vector3i, int> _affectedPoints = new DeserializableDictionary<Vector3i, int>();

        /// <summary>
        /// This is a set of feature points that represent the extremities of the feature
        /// They should be sorted in such a way that each point creates a convex edge with the next point.
        /// </summary>
        [XmlElement("FbP")]
        public List<Vector3i> BoundingPoints = new List<Vector3i>();

        /// <summary>
        /// What "style" to apply to the inside of the bounding points. This would be like generic forest, generic desert, etc.
        /// If the value is 0 then no style will be applied.
        /// </summary>
        [XmlElement("FbPid")]
        public int BoundPointsId = 0;


        public int Id = 0;


        [XmlElement("FdN")]
        public string DescriptiveName = "";

        /// <summary>
        /// Feature bounds will contain a bounding box and then a list of more fine-grained points
        /// that make up the actual area.
        /// </summary>
        //public FeatureBounds FeatureBounds;


        [XmlElement("FlR")]
        public int LoadRadius = 10;

        [XmlElement("FbS")]
        public List<SerialiableBuildingSkeleton> BuildingSkeletons = new List<SerialiableBuildingSkeleton>();

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
        public List<StateIDValuePair> StateValues = new List<StateIDValuePair>();

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

        public Feature() { }


        public FeatureEquation CreateFeatureEquation()
        {
            FeatureEquation featureEquation = new FeatureEquation();

            featureEquation.LoadRadius = LoadRadius;

            foreach(var val in AffectedPoints)
            {
                var point = CubeMethods.CubeToOffset(val.Key);

                FeaturePoint newPoint = new FeaturePoint(point.X + Origin.X, point.Y + Origin.Y);

                featureEquation.AffectedPoints.TryAdd(newPoint, val.Value);
                featureEquation.AffectedMaps.Add(FeatureEquation.FeaturePointToTileMapCoords(newPoint));
            }

            foreach(var val in BoundingPoints)
            {
                var point = CubeMethods.CubeToOffset(val);

                FeaturePoint newPoint = new FeaturePoint(point.X + Origin.X, point.Y + Origin.Y);

                featureEquation.BoundingPoints.Add(newPoint);
                featureEquation.AffectedMaps.Add(FeatureEquation.FeaturePointToTileMapCoords(newPoint));
            }

            featureEquation.FeatureTemplate = FeatureType;
            featureEquation.BoundPointsId = BoundPointsId;

            featureEquation.StateValues = StateValues;

            featureEquation.Origin = Origin;

            featureEquation.FeatureID = HashCoordinates(Origin.X, Origin.Y);

            featureEquation.Layer = Layer;

            featureEquation.DescriptiveName = DescriptiveName;

            featureEquation.LoadPriority = LoadPriority;

            return featureEquation;
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
    }


    [XmlType(TypeName = "SBS")]
    [Serializable]
    public class SerialiableBuildingSkeleton
    {
        [XmlElement("SbC")]
        public FeaturePoint IdealCenter;

        [XmlElement("SbP")]
        public List<FeaturePoint> TilePattern = new List<FeaturePoint>();

        [XmlElement("SbR")]
        public int Rotations;

        [XmlElement("SbF")]
        public int BuildingID;

        public SerialiableBuildingSkeleton() { }
    }
}
