using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Objects
{
    public class Spritesheet
    {
        public string File;
        public int Offset = 64;
        public int Rows = 10;
        public int Columns = 10;
        public Spritesheet(string file)
        {
            File = file;
        }
    }

    public static class Spritesheets 
    {
        public static Spritesheet TestSheet = new Spritesheet("Resources/SpritesheetTest.png");
        public static Spritesheet CaveSheet = new Spritesheet("Resources/CaveSpritesheet.png");
        public static Spritesheet CharacterSheet = new Spritesheet("Resources/CharacterSpritesheet.png");
        public static Spritesheet UISheet = new Spritesheet("Resources/UISpritesheet.png");
    }
}
