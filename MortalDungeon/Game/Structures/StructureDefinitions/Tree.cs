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
        public Tree(TileMap map, BaseTile tile) : base(map.Controller.Scene, Spritesheets.StructureSheet, GetTreeType(), tile.Position + new Vector3(0, -200, 0.22f))
        {
            BaseObject.BaseFrame.RotateX(25);
            BaseObject.BaseFrame.SetScaleAll(1 + (float)TileMap._randomNumberGen.NextDouble() / 2);

            VisibleThroughFog = true;
            TileMapPosition = tile;
            Name = "Tree";
            Pathable = false;

            SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

            SetTeam(UnitTeam.Neutral);
            Height = 2;
        }

        private static int GetTreeType() 
        {
            return TileMap._randomNumberGen.Next() % 2 + 2;
        }

        public override Tooltip CreateContextMenu()
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem("Chop down", (item) =>
            {
                Scene.CloseContextMenu();

                TileMapPosition.RemoveStructure(this);
            });

            return menu;
        }
    }
}
