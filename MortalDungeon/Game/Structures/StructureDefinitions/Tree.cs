using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Structures
{
    class Tree : Structure
    {
        public Tree(TileMap map, Tile tile, int treeType = -1, float scale = 1) : base(map.Controller.Scene, Spritesheets.StructureSheet, GetTreeType(treeType), tile.Position)
        {
            BaseObject.BaseFrame.RotateX(45);
            //BaseObject.BaseFrame.SetScaleAll(1 + (float)TileMap._randomNumberGen.NextDouble() / 2);
            BaseObject.BaseFrame.SetScaleAll(scale);

            CalculateInnateTileOffset();
            SetPositionOffset(tile.Position);

            VisibleThroughFog = true;
            SetTileMapPosition(tile);
            Name = "Tree";
            Pathable = false;

            HasContextMenu = true;

            Info.Height = 2;

            tile.Properties.BlockingTypes.Add(BlockingType.Vision | BlockingType.Abilities);
        }

        public override void SetTileMapPosition(Tile baseTile)
        {
            base.SetTileMapPosition(baseTile);

            baseTile.Properties.BlockingTypes.Add(BlockingType.Vision | BlockingType.Abilities);
        }

        public override void Removed()
        {
            base.Removed();

            if (Info.TileMapPosition != null)
            {
                Info.TileMapPosition.Properties.BlockingTypes.Remove(BlockingType.Vision | BlockingType.Abilities);
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();

            Info.TileMapPosition.Properties.BlockingTypes.Remove(BlockingType.Vision);
        }

        private static int GetTreeType(int treeType) 
        {
            if (treeType == -1)
            {
                return TileMap._randomNumberGen.Next() % 2 + 2;
            }
            else 
            {
                return treeType + 2;
            }
        }

        public override Tooltip CreateContextMenu()
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem("Chop down", (item) =>
            {
                Scene.CloseContextMenu();

                Info.TileMapPosition.RemoveStructure(this);
                CleanUp();
            });

            return menu;
        }
    }
}
