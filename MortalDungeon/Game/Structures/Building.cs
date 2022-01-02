using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Structures
{
    public enum BuildingEnum
    {
        Tent
    }

    /// <summary>
    /// Multi-tile structure. The 
    /// </summary>
    public class Building : Structure
    {

        /// <summary>
        /// Stored as cube coordinates that are applied relative to the actual tile position that is holding the structure.
        /// The TileAction function is called for each tile in the TilePattern list to determine what should be done to the tiles.
        /// </summary>
        public List<Vector3i> TilePattern = new List<Vector3i>();
        public int Rotations = 0;

        public List<GameObject> SupportingObject = new List<GameObject>();

        public BuildingSkeleton SkeletonReference = null;

        public int BuildingProfileId = 0;

        /// <summary>
        /// This will initialize nothing. Any buildings created with this must create a valid GameObject before attempting to be rendered.
        /// </summary>
        public Building() { }
        public Building(CombatScene scene) : base(scene) { }

        public Building(CombatScene scene, Spritesheet spritesheet, int spritesheetPos) : base(scene, spritesheet, spritesheetPos)
        {

        }

        public virtual void CreateTilePattern()
        {
            TilePattern.Clear();
        }


        public override void SetTileMapPosition(BaseTile baseTile)
        {
            BaseTile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.RemoveStructure(this);

            baseTile.AddStructure(this);

            Info.TileMapPosition = baseTile;

            TileAction();

            LightObstruction.SetPosition(baseTile);
            VisionGenerator.SetPosition(baseTile.TilePoint);

            Scene.OnStructureMoved();
        }

        public override void CleanUp()
        {
            if (SkeletonReference != null)
            {
                SkeletonReference.Loaded = false;
                SkeletonReference.Handle = null;
                SkeletonReference = null;
            }

            base.CleanUp();
        }

        public override void InitializeVisualComponent()
        {
            base.InitializeVisualComponent();

            VisibleThroughFog = true;

            //AddBaseObject(BuildingProfiles[BuildingProfileId].CreateBaseObject());
            //LoadTexture(this)


            //then the building profile would contain the code defined below.
            //
            //SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
            //AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.Tent, default));
            //BaseObject.BaseFrame.SetScale(0.5f, 0.5f, 0.25f);
            //BaseObject.BaseFrame.RotateZ(Rotations * 60);
        }

        public virtual void TileAction()
        {
            List<BaseTile> tiles = GetPatternTiles();

            foreach (BaseTile tile in tiles)
            {
                tile.Properties.Classification = TileClassification.Terrain;
            }
        }

        public List<BaseTile> GetPatternTiles()
        {
            List<BaseTile> list = new List<BaseTile>();

            if (Info.TileMapPosition == null)
                return list;

            Vector2i tilePoint = new Vector2i(Info.TileMapPosition.TilePoint.X, Info.TileMapPosition.TilePoint.Y);

            if (SkeletonReference != null)
            {
                var featurePoint = Info.TileMapPosition.ToFeaturePoint();
                Vector2i tileOffset = new Vector2i(featurePoint.X - SkeletonReference.IdealCenter.X, featurePoint.Y - SkeletonReference.IdealCenter.Y);

                tilePoint -= tileOffset;
            }


            for (int i = 0; i < TilePattern.Count; i++)
            {
                Vector2i tileCoords = CubeMethods.CubeToOffset(CubeMethods.OffsetToCube(tilePoint) + TilePattern[i]);

                //if ((tileCoords.X + tilePoint.X) % 2 == 0)
                //    tileCoords.Y++;

                BaseTile tile = GetTileMap()[tileCoords.X, tileCoords.Y];

                if (tile == null)
                    continue;

                list.Add(tile);
            }

            return list;
        }

        /// <summary>
        /// Rotates the pattern by 60 degrees N times
        /// </summary>
        public void RotateTilePattern(int rotations)
        {
            Rotations += rotations;
            for (int i = 0; i < TilePattern.Count; i++)
            {
                TilePattern[i] = CubeMethods.RotateCube(TilePattern[i], rotations);
            }
        }

        /// <summary>
        /// Returns the ideal case list of points if the passed buildingLocation is the center point of the building.
        /// </summary>
        public List<FeaturePoint> GetPatternFeaturePoints(FeaturePoint buildingLocation)
        {
            List<FeaturePoint> list = new List<FeaturePoint>();

            for (int i = 0; i < TilePattern.Count; i++)
            {
                Vector2i tileCoords = CubeMethods.CubeToOffset(CubeMethods.OffsetToCube(buildingLocation) + TilePattern[i]);

                FeaturePoint point = new FeaturePoint(tileCoords);

                list.Add(point);
            }

            return list;

        }
    }

    /// <summary>
    /// The minimum possible information needed to recreate a building.
    /// </summary>
    public class BuildingSkeleton
    {
        public FeaturePoint IdealCenter;
        public HashSet<FeaturePoint> TilePattern;
        public int Rotations;

        public bool Loaded;
        public Building Handle;

        /// <summary>
        /// True when the skeleton is acted upon during application of a feature equation until OnAppliedToMaps is called 
        /// </summary>
        public bool _skeletonTouchedThisCycle;

        public static BuildingSkeleton Empty;

        public static BuildingSkeleton CreateFromSerialized(SerialiableBuildingSkeleton skele)
        {
            BuildingSkeleton newSkele = new BuildingSkeleton();

            newSkele.TilePattern = skele.TilePattern.ToHashSet();
            newSkele.IdealCenter = skele.IdealCenter;
            newSkele.Rotations = skele.Rotations;

            //using the BuildingID, create the skeleton's handle.

            return newSkele;
        }
    }
}
