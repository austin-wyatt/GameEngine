using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Objects
{
    internal class Spritesheet
    {
        internal string File;
        internal int Offset = 64;
        internal int Rows = 10;
        internal int Columns = 10;
        internal TextureName TextureName;
        internal Spritesheet(string file, TextureName textureName)
        {
            File = file;
            TextureName = textureName;
        }
    }

    internal static class Spritesheets 
    {
        internal static Spritesheet TestSheet = new Spritesheet("Resources/SpritesheetTest.png", TextureName.SpritesheetTest);
        internal static Spritesheet CaveSheet = new Spritesheet("Resources/CaveSpritesheet.png", TextureName.CaveSpritesheet);
        internal static Spritesheet CharacterSheet = new Spritesheet("Resources/CharacterSpritesheet.png", TextureName.CharacterSpritesheet);
        internal static Spritesheet UISheet = new Spritesheet("Resources/UISpritesheet.png", TextureName.UISpritesheet);
        internal static Spritesheet IconSheet = new Spritesheet("Resources/IconSpritesheet.png", TextureName.IconSpritesheet);
        internal static Spritesheet TileSheet = new Spritesheet("Resources/TileSpritesheet.png", TextureName.TileSpritesheet)
        {
            Offset = 128,
            Rows = 20,
            Columns = 20
        };
        internal static Spritesheet StructureSheet = new Spritesheet("Resources/StructureSpritesheet.png", TextureName.StructureSpritesheet);
        internal static Spritesheet ObjectSheet = new Spritesheet("Resources/ObjectSheet.png", TextureName.ObjectSpritesheet);

        internal static Spritesheet TextureTestSheet = new Spritesheet("Resources/TestTexture.png", TextureName.TestTexture) 
        {
            Rows = 2,
            Columns = 2
        };

        internal static Spritesheet SphereTexture = new Spritesheet("Resources/Sphere texture.png", TextureName.SphereTexture)
        {
            Rows = 1,
            Columns = 1
        };
        internal static Spritesheet CubeTexture = new Spritesheet("Resources/cube texture.png", TextureName.CubeTexture)
        {
            Rows = 1,
            Columns = 1
        };

        internal static Spritesheet LightObstructionSheet = new Spritesheet("Resources/LightObstructionSheet.png", TextureName.LightObstructionSheet)
        {
            Rows = 10,
            Columns = 10,
            Offset = 32
        };

    }

    internal static class Textures 
    {
        internal static readonly Texture GEN_DIFFUSE_MAP = Texture.LoadFromFile("Resources/GEN_PURPOSE_DIFFUSE.png");
        internal static readonly Texture GEN_AMBIENT_MAP = Texture.LoadFromFile("Resources/GEN_PURPOSE_AMBIENT.png");

        internal static Spritesheet TentTexture = new Spritesheet("Resources/3D models/Canvas texture.png", TextureName.SphereTexture)
        {
            Rows = 1,
            Columns = 1
        };
    }
}
