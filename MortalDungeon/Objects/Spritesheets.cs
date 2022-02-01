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
        public int TextureId;

        public Spritesheet() { }
        public Spritesheet(string file, TextureName textureName)
        {
            File = file;
            TextureId = (int)textureName;
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

        public static Spritesheet TileOverlaySpritesheet = new Spritesheet("Resources/TileOverlaySpritesheet.png", TextureName.TileOverlaySpritesheet)
        {
            Offset = 128,
            Rows = 20,
            Columns = 20
        };

        public static Spritesheet UnitSpritesheet = new Spritesheet("Resources/UnitSpritesheet.png", TextureName.UnitSpritesheet)
        {
            Offset = 32,
            Rows = 32,
            Columns = 32
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

        public static Spritesheet Cursor_1 = new Spritesheet("Resources/Cursor_1.png", TextureName.Cursor)
        {
            Offset = 19,
            Rows = 2,
            Columns = 2
        };

        public static Dictionary<int, Spritesheet> AllSpritesheets = new Dictionary<int, Spritesheet>();

        static Spritesheets()
        {
            AllSpritesheets.Add(TestSheet.TextureId, TestSheet);
            AllSpritesheets.Add(ObjectSheet.TextureId, ObjectSheet);
            AllSpritesheets.Add(StructureSheet.TextureId, StructureSheet);
            AllSpritesheets.Add(TileSheet.TextureId, TileSheet);
            AllSpritesheets.Add(IconSheet.TextureId, IconSheet);
            AllSpritesheets.Add(UISheet.TextureId, UISheet);
            AllSpritesheets.Add(CharacterSheet.TextureId, CharacterSheet);
            AllSpritesheets.Add(UnitSpritesheet.TextureId, UnitSpritesheet);

            AllSpritesheets.Add(CharacterSheetSDF.TextureId, CharacterSheetSDF);
            AllSpritesheets.Add(SphereTexture.TextureId, SphereTexture);
            AllSpritesheets.Add(CubeTexture.TextureId, CubeTexture);
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
