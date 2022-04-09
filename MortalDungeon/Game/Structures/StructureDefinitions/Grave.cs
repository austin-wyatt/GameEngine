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
    class Grave : Structure
    {
        public Grave(TileMap map, Tile tile, int graveType) : base(map.Controller.Scene, Spritesheets.StructureSheet, graveType, tile.Position + new Vector3(0, -250, 0.12f))
        {
            if (tile.Structure != null)
                return;

            BaseObject.BaseFrame.RotateX(35);
            BaseObject.BaseFrame.SetScaleAll(1 + (float)TileMap._randomNumberGen.NextDouble() / 4);

            Pathable = false;
            VisibleThroughFog = true;
            SetTileMapPosition(tile);

            tile.Properties.Type = TileType.Dirt;

            tile.Update();

            Name = "Grave";

            HasContextMenu = true;

            SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

            SetTeam(UnitTeam.Unknown);
        }


        public override Tooltip CreateContextMenu()
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem("Dig up", (item) =>
            {
                Scene.CloseContextMenu();

                Info.TileMapPosition.Properties.Type = TileType.Gravel;

                Info.TileMapPosition.Update();

                HasContextMenu = false;
            });

            return menu;
        }
    }
}
