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
        public int ID = 0;
        /// <summary>
        /// Stored as cube coordinates that are applied relative to the actual tile position that is holding the structure.
        /// The TileAction function is called for each tile in the TilePattern list to determine what should be done to the tiles.
        /// </summary>
        public List<Vector3i> TilePattern = new List<Vector3i>();
        public int Rotations = 0;

        public FeaturePoint IdealCenter;
        public Vector3i ActualCenter;

        public List<GameObject> SupportingObjects = new List<GameObject>();

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


        public override void SetTileMapPosition(Tile baseTile)
        {
            Tile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.RemoveStructure(this);

            baseTile.AddStructure(this);

            Info.TileMapPosition = baseTile;

            TileAction();

            VisionGenerator.SetPosition(baseTile.TilePoint);

            Scene.OnStructureMoved();
        }

        public override void InitializeVisualComponent()
        {
            //base.InitializeVisualComponent();

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
            List<Tile> tiles = GetPatternTiles();

            foreach (Tile tile in tiles)
            {
                tile.Properties.Classification = TileClassification.ImpassableGround;
            }
        }

        public List<Tile> GetPatternTiles()
        {
            List<Tile> list = new List<Tile>();

            if (Info.TileMapPosition == null)
                return list;

            foreach(var point in TilePattern)
            {
                FeaturePoint featurePoint = new FeaturePoint(CubeMethods.CubeToOffset(point - ActualCenter + CubeMethods.OffsetToCube(Info.TileMapPosition.ToFeaturePoint())));

                var tile = TileMapHelpers.GetTile(featurePoint);

                if(tile != null)
                {
                    list.Add(tile);
                }
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

        public void RotateBuilding(int rotations)
        {
            Rotations += rotations;
            for (int i = 0; i < TilePattern.Count; i++)
            {
                TilePattern[i] = CubeMethods.RotateCube(TilePattern[i], rotations);
            }

            BaseObject.BaseFrame.RotateZ(60 * rotations);
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
}
