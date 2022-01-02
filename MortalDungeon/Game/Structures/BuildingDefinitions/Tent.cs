using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    public class Tent : Building
    {
        public Tent() 
        {
            //CreateTilePattern();
            Type = (StructureEnum)BuildingEnum.Tent;
        }
        public Tent(CombatScene scene) : base(scene)
        {
            //CreateTilePattern();
            Type = (StructureEnum)BuildingEnum.Tent;
        }

        public override void CreateTilePattern()
        {
            base.CreateTilePattern();

            TilePattern.Add(CubeMethods.CubeDirections[Direction.NorthEast]);
            TilePattern.Add(CubeMethods.CubeDirections[Direction.SouthEast]);


            TilePattern.Add(CubeMethods.CubeDirections[Direction.NorthWest]);
            TilePattern.Add(CubeMethods.CubeDirections[Direction.SouthWest]);
            TilePattern.Add(CubeMethods.CubeDirections[Direction.None]);
        }

        /// <summary>
        /// Call this from the feature equation that loads this building
        /// </summary>
        public override void InitializeVisualComponent()
        {
            base.InitializeVisualComponent();

            VisibleThroughFog = true;

            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
            AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.Tent, default));
            BaseObject.BaseFrame.SetScale(0.5f, 0.5f, 0.25f);
            BaseObject.BaseFrame.RotateZ(Rotations * 60);

            LoadTexture(this);
        }

        public override void TileAction()
        {
            List<BaseTile> tiles = GetPatternTiles();

            foreach (BaseTile tile in tiles)
            {
                if (tile.Structure != null && tile.Structure != this)
                {
                    var structure = tile.Structure;
                    tile.RemoveStructure(structure);
                    structure.CleanUp();
                }

                tile.Properties.Classification = TileClassification.Terrain;
                tile.Properties.Type = TileType.Dirt;
                //tile.Color = new Vector4(1, 0, 0, 1);
                tile.Update();
            }
        }
    }
}
