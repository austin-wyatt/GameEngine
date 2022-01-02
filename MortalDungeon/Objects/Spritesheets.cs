using MortalDungeon.Engine_Classes;
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

        public Spritesheet() { }
        public Spritesheet(string file, TextureName textureName)
        {
            File = file;
            TextureName = textureName;
        }
    }

    public static class Spritesheets 
    {
        public static Spritesheet TestSheet = new Spritesheet("Resources/SpritesheetTest.png", TextureName.SpritesheetTest);
        public static Spritesheet CharacterSheet = new Spritesheet("Resources/CharacterSpritesheet.png", TextureName.CharacterSpritesheet);
        public static Spritesheet CharacterSheetSDF = new Spritesheet("Resources/CharacterSpritesheetDistance.png", TextureName.CharacterSpritesheetSDF) 
        {
            Offset = 64,
            Rows = 16,
            Columns = 16
        };

        public static Spritesheet UISheet = new Spritesheet("Resources/UISpritesheet.png", TextureName.UISpritesheet);
        public static Spritesheet IconSheet = new Spritesheet("Resources/IconSpritesheet.png", TextureName.IconSpritesheet);
        public static Spritesheet TileSheet = new Spritesheet("Resources/TileSpritesheet.png", TextureName.TileSpritesheet)
        {
            Offset = 128,
            Rows = 20,
            Columns = 20
        };
        public static Spritesheet StructureSheet = new Spritesheet("Resources/StructureSpritesheet.png", TextureName.StructureSpritesheet);
        public static Spritesheet ObjectSheet = new Spritesheet("Resources/ObjectSheet.png", TextureName.ObjectSpritesheet);


        public static Spritesheet SphereTexture = new Spritesheet("Resources/Sphere texture.png", TextureName.SphereTexture)
        {
            Rows = 1,
            Columns = 1
        };
        public static Spritesheet CubeTexture = new Spritesheet("Resources/cube texture.png", TextureName.CubeTexture)
        {
            Rows = 1,
            Columns = 1
        };

        public static Dictionary<TextureName, Spritesheet> AllSpritesheets = new Dictionary<TextureName, Spritesheet>();

        static Spritesheets()
        {
            AllSpritesheets.Add(TestSheet.TextureName, TestSheet);
            AllSpritesheets.Add(ObjectSheet.TextureName, ObjectSheet);
            AllSpritesheets.Add(StructureSheet.TextureName, StructureSheet);
            AllSpritesheets.Add(TileSheet.TextureName, TileSheet);
            AllSpritesheets.Add(IconSheet.TextureName, IconSheet);
            AllSpritesheets.Add(UISheet.TextureName, UISheet);
            AllSpritesheets.Add(CharacterSheet.TextureName, CharacterSheet);

            AllSpritesheets.Add(CharacterSheetSDF.TextureName, CharacterSheetSDF);
            AllSpritesheets.Add(SphereTexture.TextureName, SphereTexture);
            AllSpritesheets.Add(CubeTexture.TextureName, CubeTexture);
        }
    }

    public static class Textures 
    {
        public static readonly Texture GEN_DIFFUSE_MAP = Texture.LoadFromFile("Resources/GEN_PURPOSE_DIFFUSE.png");
        public static readonly Texture GEN_AMBIENT_MAP = Texture.LoadFromFile("Resources/GEN_PURPOSE_AMBIENT.png");

        public static Spritesheet TentTexture = new Spritesheet("Resources/3D models/Canvas texture.png", TextureName.SphereTexture)
        {
            Rows = 1,
            Columns = 1
        };
    }
}
