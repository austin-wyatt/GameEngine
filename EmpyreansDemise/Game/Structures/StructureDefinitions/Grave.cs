using Empyrean.Engine_Classes;
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

            tile.Properties.SetType(TileType.Dirt);

            Name = "Grave";

            HasContextMenu = true;


            SetTeam(UnitTeam.Unknown);
        }


        public override Tooltip CreateContextMenu()
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem("Dig up", (item) =>
            {
                Scene.CloseContextMenu();

                Info.TileMapPosition.Properties.SetType(TileType.Gravel, true);

                HasContextMenu = false;
            });

            return menu;
        }
    }
}
