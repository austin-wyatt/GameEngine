using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    /// <summary>
    /// Multi-tile structure. The 
    /// </summary>
    internal class Building : Structure
    {

        /// <summary>
        /// Stored as cube coordinates that are applied relative to the actual tile position that is holding the structure.
        /// The TileAction function is called for each tile in the TilePattern list to determine what should be done to the tiles.
        /// </summary>
        internal List<Vector3i> TilePattern = new List<Vector3i>();
        internal int Rotations = 0;

        internal List<GameObject> SupportingObject = new List<GameObject>();

        internal BuildingSkeleton SkeletonReference = null;


        /// <summary>
        /// This will initialize nothing. Any buildings created with this must create a valid GameObject before attempting to be rendered.
        /// </summary>
        internal Building() { }
        internal Building(CombatScene scene) : base(scene) { }

        internal Building(CombatScene scene, Spritesheet spritesheet, int spritesheetPos) : base(scene, spritesheet, spritesheetPos) 
        {

        }

        internal virtual void CreateTilePattern() 
        {
            TilePattern.Clear();
        }
        

        internal override void SetTileMapPosition(BaseTile baseTile)
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

        internal override void CleanUp()
        {
            if (SkeletonReference != null)
            {
                SkeletonReference.Loaded = false;
                SkeletonReference.Handle = null;
                SkeletonReference = null;
            }

            base.CleanUp();
        }

        internal virtual void TileAction() 
        {
            List<BaseTile> tiles = GetPatternTiles();

            foreach (BaseTile tile in tiles) 
            {
                tile.Properties.Classification = TileClassification.Terrain;
            }
        }

        internal List<BaseTile> GetPatternTiles() 
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
        internal void RotateTilePattern(int rotations) 
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
        internal List<FeaturePoint> GetPatternFeaturePoints(FeaturePoint buildingLocation) 
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
    internal class BuildingSkeleton
    {
        internal FeaturePoint IdealCenter;
        internal HashSet<FeaturePoint> TilePattern;
        internal int Rotations;

        internal bool Loaded;
        internal Building Handle;

        /// <summary>
        /// True when the skeleton is acted upon during application of a feature equation until OnAppliedToMaps is called 
        /// </summary>
        internal bool _skeletonTouchedThisCycle;

        internal static BuildingSkeleton Empty;

        #region overrides

        #endregion
    }
}
