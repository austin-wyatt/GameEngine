using MortalDungeon.Engine_Classes;
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
    class Rock : Structure
    {
        internal Rock(TileMap map, BaseTile tile) : base(map.Controller.Scene, Spritesheets.StructureSheet, GetRockType(), tile.Position + new Vector3(0, -200, 0.12f))
        {
            if (tile.Structure != null)
                return;

            BaseObject.BaseFrame.RotateX(15);
            BaseObject.BaseFrame.SetScaleAll(1 + (float)TileMap._randomNumberGen.NextDouble() / 2);

            Pathable = false;
            VisibleThroughFog = true;
            SetTileMapPosition(tile);
            Name = "Rock";

            HasContextMenu = true;

            SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

            SetTeam(UnitTeam.Unknown);
            if (Type == StructureEnum.Rock_2)
            {
                Info.Height = 1;
            }
            else
            {
                Info.Height = 2;
            }
        }

        private static int GetRockType()
        {
            return TileMap._randomNumberGen.Next() % 3 + (int)StructureEnum.Rock_1;
        }

        internal override Tooltip CreateContextMenu()
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem("Mine", (item) =>
            {
                Scene.CloseContextMenu();

                Info.TileMapPosition.RemoveStructure(this);
            });

            return menu;
        }
    }
}
