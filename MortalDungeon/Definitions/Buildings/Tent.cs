using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.GameObjects;
using Empyrean.Game.Objects;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Buildings
{
    public class Tent : Building
    {
        public const int GlobalID = 1;

        public Tent() 
        {
            ID = GlobalID;

            TilePattern = new List<Vector3i> { new Vector3i(0, 0, 0), new Vector3i(-1, 0, 1), new Vector3i(-1, 1, 0), new Vector3i(1, -1, 0), new Vector3i(1, 0, -1) };
            Type = (StructureEnum)BuildingEnum.Tent;
        }

        /// <summary>
        /// Call this from the feature equation that loads this building
        /// </summary>
        public override void InitializeVisualComponent()
        {
            //base.InitializeVisualComponent();
            BaseObjects.Clear();

            VisibleThroughFog = true;

            SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));

            AddBaseObject(CreateBaseObject());
            BaseObject.BaseFrame.SetScale(0.5f, 0.5f, 0.25f);
            BaseObject.BaseFrame.RotateZ(Rotations * 60);

            BaseObject.BaseFrame.CameraPerspective = true;

            CalculateInnateTileOffset();

            LoadTexture(this);
        }

        public override BaseObject CreateBaseObject()
        {
            BaseObject obj = _3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.Tent, default);

            obj.BaseFrame.SetBaseColor(Color);

            return obj;
        }

        public override void CleanUp()
        {
            base.CleanUp();

            foreach (var tile in GetPatternTiles())
            {
                tile.Properties.BlockingTypes.Remove(BlockingType.Abilities);
            }
        }

        public override void TileAction()
        {
            List<Tile> tiles = GetPatternTiles();

            foreach (Tile tile in tiles)
            {
                if (tile.Structure != null && tile.Structure != this)
                {
                    var structure = tile.Structure;
                    tile.RemoveStructure(structure);
                    structure.CleanUp();
                }

                tile.Properties.Classification = TileClassification.ImpassableGround;
                tile.Properties.SetType(TileType.Stone_1);
                tile.Properties.BlockingTypes.Add(BlockingType.Abilities);
                //tile.Color = new Vector4(1, 0, 0, 1);
            }
        }
    }
}
