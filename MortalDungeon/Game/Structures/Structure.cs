using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    internal enum StructureEnum 
    {
        Unknown = 0,

        Tree_1 = 2,
        Tree_2 = 3,

        Rock_1 = 10,
        Rock_2 = 11,
        Rock_3 = 12,

        Grave_1 = 20,
        Grave_2 = 21,
        Grave_3 = 22,

        Wall_1 = 30,
        Wall_Corner = 31,
        Wall_Door = 32,
        Wall_Wood_1 = 40,
        Wall_Wood_Corner = 41,
        Wall_Wood_Door = 42,
        Wall_Iron_1 = 50,
        Wall_Iron_Door = 51,

        Cliff_1 = 90,
        Cliff_2 = 91,
        Cliff_3 = 92,
        Cliff_4 = 93,
        Cliff_5 = 94,
        Cliff_6 = 95,

        Tent = 1000
    }
    internal class Structure : Unit
    {
        internal StructureEnum Type;

        internal bool Pathable = false;
        internal bool Passable = false; //when passable the height of the object is not factored into the pathable height

        /// <summary>
        /// This will initialize nothing. Any structures created with this must create a valid GameObject before attempting to be rendered.
        /// </summary>
        internal Structure() 
        {
            Name = "Structure";

            Type = StructureEnum.Unknown;
        }
        internal Structure(CombatScene scene) : base(scene)
        {
            Name = "Structure";

            Type = StructureEnum.Unknown;
        }
        internal Structure(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(scene, spritesheet, spritesheetPos, position)
        {
            Name = "Structure";

            Type = (StructureEnum)spritesheetPos;
        }

        internal override void SetTileMapPosition(BaseTile baseTile)
        {
            BaseTile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.RemoveStructure(this);

            baseTile.AddStructure(this);

            Info.TileMapPosition = baseTile;

            LightObstruction.SetPosition(baseTile);
            VisionGenerator.SetPosition(baseTile.TilePoint);

            Scene.OnStructureMoved();
        }

        internal override void CleanUp()
        {
            base.CleanUp();

            Info = null;
            AI = null;
        }
    }
}
