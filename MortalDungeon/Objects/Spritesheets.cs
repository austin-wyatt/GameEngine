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
        public TextureName TextureName;
        public Spritesheet(string file, TextureName textureName)
        {
            File = file;
            TextureName = textureName;
        }
    }

    public static class Spritesheets 
    {
        public static Spritesheet TestSheet = new Spritesheet("Resources/SpritesheetTest.png", TextureName.SpritesheetTest);
        public static Spritesheet CaveSheet = new Spritesheet("Resources/CaveSpritesheet.png", TextureName.CaveSpritesheet);
        public static Spritesheet CharacterSheet = new Spritesheet("Resources/CharacterSpritesheet.png", TextureName.CharacterSpritesheet);
        public static Spritesheet UISheet = new Spritesheet("Resources/UISpritesheet.png", TextureName.UISpritesheet);
        public static Spritesheet IconSheet = new Spritesheet("Resources/IconSpritesheet.png", TextureName.IconSpritesheet);
        public static Spritesheet TileSheet = new Spritesheet("Resources/TileSpritesheet.png", TextureName.TileSpritesheet)
        {
            Offset = 128,
            Rows = 20,
            Columns = 20
        };
        public static Spritesheet StructureSheet = new Spritesheet("Resources/StructureSpritesheet.png", TextureName.StructureSpritesheet);
        
    }
}
