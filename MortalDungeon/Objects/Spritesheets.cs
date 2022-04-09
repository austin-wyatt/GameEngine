using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Objects
{
    [Serializable]
    public class Spritesheet : ISerializable
    {
        public string File;
        public int Offset = 64;
        public int Rows = 10;
        public int Columns = 10;
        public int TextureId;
        public string Name = "";

        public Spritesheet() { }
        public Spritesheet(string file, TextureName textureName)
        {
            File = file;
            TextureId = (int)textureName;
        }

        public void CompleteDeserialization()
        {

        }

        public void PrepareForSerialization()
        {

        }
    }

    public static class Spritesheets 
    {
        public static Spritesheet TestSheet = new Spritesheet("Resources/SpritesheetTest.png", TextureName.SpritesheetTest) 
        {
            Name = "TestSheet"
        };
        public static Spritesheet CharacterSheet = new Spritesheet("Resources/CharacterSpritesheet.png", TextureName.CharacterSpritesheet)
        {
            Name = "CharacterSheet"
        };
        public static Spritesheet CharacterSheetSDF = new Spritesheet("Resources/CharacterSpritesheetDistance.png", TextureName.CharacterSpritesheetSDF) 
        {
            Offset = 64,
            Rows = 16,
            Columns = 16,
            Name = "CharacterSheetSDF"
        };

        public static Spritesheet UISheet = new Spritesheet("Resources/UISpritesheet.png", TextureName.UISpritesheet)
        {
            Name = "UISheet"
        };
        public static Spritesheet IconSheet = new Spritesheet("Resources/IconSpritesheet.png", TextureName.IconSpritesheet)
        {
            Name = "IconSheet"
        };
        //public static Spritesheet TileSheet = new Spritesheet("Resources/TileSpritesheet.png", TextureName.TileSpritesheet)
        //{
        //    Offset = 128,
        //    Rows = 20,
        //    Columns = 20,
        //    Name = "TileSheet"
        //};
        public static Spritesheet TileSheet = new Spritesheet("Resources/TileSpritesheet_512_1.png", TextureName.TileSpritesheet)
        {
            Offset = 512,
            Rows = 5,
            Columns = 5,
            Name = "TileSheet"
        };

        public static readonly Spritesheet[] TileSheets = new Spritesheet[] { TileSheet, null, null };

        public static Spritesheet TileOverlaySpritesheet = new Spritesheet("Resources/TileOverlaySpritesheet.png", TextureName.TileOverlaySpritesheet)
        {
            Offset = 128,
            Rows = 20,
            Columns = 20,
            Name = "TileOverlaySheet"
        };

        public static Spritesheet UnitSpritesheet = new Spritesheet("Resources/UnitSpritesheet.png", TextureName.UnitSpritesheet)
        {
            Offset = 32,
            Rows = 32,
            Columns = 32,
            Name = "UnitSheet"
        };

        public static Spritesheet StructureSheet = new Spritesheet("Resources/StructureSpritesheet.png", TextureName.StructureSpritesheet)
        {
            Name = "StructureSheet"
        };
        public static Spritesheet ObjectSheet = new Spritesheet("Resources/ObjectSheet.png", TextureName.ObjectSpritesheet)
        {
            Name = "ObjectSheet"
        };

        public static Spritesheet ItemSpritesheet_1 = new Spritesheet("Resources/ItemSpritesheet_1.png", TextureName.ItemSpritesheet_1)
        {
            Name = "ItemSheet_1"
        };
        public static Spritesheet UISpritesheet_1 = new Spritesheet("Resources/UISpritesheet_1.png", TextureName.UISpritesheet_1)
        {
            Name = "UISpritesheet_1"
        };
        public static Spritesheet UIControlsSpritesheet = new Spritesheet("Resources/UIControlsSpritesheet.png", TextureName.UIControlsSpritesheet)
        {
            Offset = 16,
            Rows = 20,
            Columns = 20,
            Name = "UIControlsSheet"
        };

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
            AllSpritesheets.Add(ItemSpritesheet_1.TextureId, ItemSpritesheet_1);
            AllSpritesheets.Add(UIControlsSpritesheet.TextureId, UIControlsSpritesheet);

            AllSpritesheets.Add(CharacterSheetSDF.TextureId, CharacterSheetSDF);
        }
    }

    public static class Textures 
    {
        public static readonly Texture GEN_DIFFUSE_MAP = Texture.LoadFromFile("Resources/GEN_PURPOSE_DIFFUSE.png");

        public static Spritesheet GEN_AMBIENT_MAP = new Spritesheet("Resources/GEN_PURPOSE_DIFFUSE.png", TextureName.FogTexture)
        {
            Rows = 1,
            Columns = 1
        };

        public static Spritesheet TentTexture = new Spritesheet("Resources/3D models/Canvas texture.png", TextureName.SphereTexture)
        {
            Rows = 1,
            Columns = 1
        };
    }
}
