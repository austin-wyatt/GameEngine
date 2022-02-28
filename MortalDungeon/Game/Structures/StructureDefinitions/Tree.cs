using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    class Tree : Structure
    {
        public Tree(TileMap map, BaseTile tile, int treeType = -1) : base(map.Controller.Scene, Spritesheets.StructureSheet, GetTreeType(treeType), tile.Position + new Vector3(0, -200, 0.22f))
        {
            BaseObject.BaseFrame.RotateX(25);
            BaseObject.BaseFrame.SetScaleAll(1 + (float)TileMap._randomNumberGen.NextDouble() / 2);

            VisibleThroughFog = true;
            SetTileMapPosition(tile);
            Name = "Tree";
            Pathable = false;

            HasContextMenu = true;

            Info.Height = 2;

            LightObstruction.ObstructionType = Lighting.LightObstructionType.Tree;
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
