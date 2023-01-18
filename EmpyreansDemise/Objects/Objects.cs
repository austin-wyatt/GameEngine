using Empyrean.Game.Objects;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Empyrean.Objects
{
    public enum TextureName 
    {
        Unknown,
        SpritesheetTest,
        CaveSpritesheet,
        CharacterSpritesheet,
        CharacterSpritesheetSDF,
        UISpritesheet,
        IconSpritesheet,
        TileSpritesheet,
        StructureSpritesheet,
        ObjectSpritesheet,
        LightObstructionSheet,

        FogTexture,
        TestTexture,
        SphereTexture,
        CubeTexture,

        DynamicTexture,

        Lighting,
        LightObstructionMap,

        TileOverlaySpritesheet,
        UnitSpritesheet,
        ItemSpritesheet_1,
        UISpritesheet_1,
        UIControlsSpritesheet,

        Cursor,

        Dirt,
        Grass,
        Stone_1,
        Stone_2,
        X
    }
    public class TextureInfo
    {
        public int[] TextureIds;
        public string[] TextureFilenames; //filename of texture/spritesheet
        public int[] TexturePositions; //which index of the spritesheet a texture resides in

        public Spritesheet Spritesheet;

        public TextureInfo() { }
        public TextureInfo(string texture, Spritesheet spritesheet = null)
        {
            TextureIds = new int[] { (int)TextureName.Unknown };
            TexturePositions = new int[] { 0 };
            TextureFilenames = new string[] { texture };

            Spritesheet = spritesheet;
        }
        public TextureInfo(TextureName texture, Spritesheet spritesheet = null) 
        {
            TextureIds = new int[] { (int)texture };
            TexturePositions = new int[] { 0 };

            Spritesheet = spritesheet;
        }
        public TextureInfo(int[] textures, int[] positions, Spritesheet spritesheet = null)
        {
            TextureIds = textures;
            TexturePositions = positions;

            Spritesheet = spritesheet;
        }

        public TextureInfo(TextureName texture, int position, Spritesheet spritesheet = null)
        {
            TextureIds = new int[] { (int)texture };
            TexturePositions = new int[] { position };

            Spritesheet = spritesheet;
        }

        public TextureInfo(Spritesheet spritesheet, int[] positions )
        {
            int[] textures = new int[positions.Length];
            string[] textureFilename = new string[positions.Length];
            for(int i = 0; i < textures.Length; i++)
            {
                textures[i] = spritesheet.TextureId;
                textureFilename[i] = spritesheet.File;
            }
            TextureIds = textures;
            TexturePositions = positions;
            TextureFilenames = textureFilename;

            Spritesheet = spritesheet;
        }
    }

    public class ObjectDefinition
    {
        public float[] Vertices;
        public uint[] Indices;
        public int Points;
        public TextureInfo Textures;
        public Vector3 Center;
        public float[] Bounds;
        public float[] fastVertices;

        public float VerticeType = 0;

        public float SpritesheetPosition = 0;
        public Vector2 SideLengths = new Vector2(1, 1);
        public ObjectDefinition(float[] vertices, uint[] indexes, int points, TextureInfo textures, Vector3 center = new Vector3(), float[] bounds = null, bool centerVertices = true)
        {
            Indices = indexes;
            Points = points;
            Textures = textures;
            Center = center;
            Vertices = vertices;

            if (bounds == null) 
            {
                List<float> tempBounds = new List<float>();
                int stride = Vertices.Length / points;
                for(int i = 0; i < points; i++)
                {
                    tempBounds.Add(Vertices[i * stride]);
                    tempBounds.Add(Vertices[i * stride + 1]);
                    tempBounds.Add(Vertices[i * stride + 2]);
                }

                Bounds = tempBounds.ToArray();
            }
            else
            {
                Bounds = bounds;
            }

        }
        public ObjectDefinition() { }
    }
}