using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    public enum StructureEnum 
    {
        Tree_1 = 2,
        Tree_2 = 3,

        Rock_1 = 10,
        Rock_2 = 11,
        Rock_3 = 12,

        Wall_1 = 30,
        Wall_Corner = 31,
        Wall_Door = 32,
        Wall_Wood_1 = 40,
        Wall_Wood_Corner = 41,
        Wall_Wood_Door = 42,

        Cliff_1 = 90,
        Cliff_2 = 91,
        Cliff_3 = 92,
        Cliff_4 = 93,
        Cliff_5 = 94,
        Cliff_6 = 95,
    }
    public class Structure : Unit
    {
        public StructureEnum Type;

        public bool Pathable = false;
        public bool Passable = false; //when passable the height of the object is not factored into the pathable height

        public Structure(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(scene, spritesheet, spritesheetPos, position)
        {
            Name = "Structure";

            Type = (StructureEnum)spritesheetPos;
        }

        public override void SetTileMapPosition(BaseTile baseTile)
        {
            BaseTile prevTile = Info.TileMapPosition;

            if(prevTile != null)
                prevTile.Structure = null;

            baseTile.Structure = this;

            Info.TileMapPosition = baseTile;
        }
    }
}
