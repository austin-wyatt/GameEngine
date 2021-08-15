using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public enum StructureEnum 
    {
        Tree_1 = 2,
        Tree_2 = 3,

        Rock_1 = 10,
        Rock_2 = 11,
        Rock_3 = 12,

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

        public Structure(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(scene, spritesheet, spritesheetPos, position)
        {
            Name = "Structure";

            Type = (StructureEnum)spritesheetPos;
        }
    }
}
