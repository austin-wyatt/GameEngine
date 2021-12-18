using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Objects
{
    internal enum TextureName 
    {
        Unknown,
        SpritesheetTest,
        CaveSpritesheet,
        CharacterSpritesheet,
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
        LightObstructionMap
    }
    internal class TextureInfo
    {
        internal TextureName[] Textures;
        internal string[] TextureFilenames; //filename of texture/spritesheet
        internal int[] TexturePositions; //which index of the spritesheet a texture resides in

        internal Spritesheet Spritesheet;

        internal TextureInfo(string texture, Spritesheet spritesheet = null)
        {
            Textures = new TextureName[] { TextureName.Unknown };
            TexturePositions = new int[] { 0 };
            TextureFilenames = new string[] { texture };

            Spritesheet = spritesheet;
        }
        internal TextureInfo(TextureName texture, Spritesheet spritesheet = null) 
        {
            Textures = new TextureName[] { texture };
            TexturePositions = new int[] { 0 };

            Spritesheet = spritesheet;
        }
        internal TextureInfo(TextureName[] textures, int[] positions, Spritesheet spritesheet = null)
        {
            Textures = textures;
            TexturePositions = positions;

            Spritesheet = spritesheet;
        }

        internal TextureInfo(TextureName texture, int position, Spritesheet spritesheet = null)
        {
            Textures = new TextureName[] { texture };
            TexturePositions = new int[] { position };

            Spritesheet = spritesheet;
        }

        internal TextureInfo(Spritesheet spritesheet, int[] positions )
        {
            TextureName[] textures = new TextureName[positions.Length];
            string[] textureFilename = new string[positions.Length];
            for(int i = 0; i < textures.Length; i++)
            {
                textures[i] = spritesheet.TextureName;
                textureFilename[i] = spritesheet.File;
            }
            Textures = textures;
            TexturePositions = positions;
            TextureFilenames = textureFilename;

            Spritesheet = spritesheet;
        }
    }

    internal class ObjectDefinition
    {
        internal float[] Vertices;
        internal uint[] Indices;
        internal int Points;
        internal TextureInfo Textures;
        internal Vector3 Center;
        internal float[] Bounds;
        internal float[] fastVertices;

        private bool _centerVertices;

        internal float VerticeType = 0;

        internal float SpritesheetPosition = 0;
        internal Vector2 SideLengths = new Vector2(1, 1);
        internal ObjectDefinition(float[] vertices, uint[] indexes, int points, TextureInfo textures, Vector3 center = new Vector3(), float[] bounds = null, bool centerVertices = true)
        {
            Indices = indexes;
            Points = points;
            Textures = textures;
            Center = center;
            if(centerVertices)
                Vertices = CenterVertices(vertices);
            else
                Vertices = vertices;

            _centerVertices = centerVertices;

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
        internal ObjectDefinition() { }
        //Centers the vertices of the renderable object when defined (might want to move this to a different area at some point)
        internal float[] CenterVertices(float[] vertices)
        {
            //vertices will be stored in [x, y, z, textureX, textureY] format
            int stride = vertices.Length / Points;

            float centerX = Center.X;
            float centerY = Center.Y;
            float centerZ = Center.Z;



            for (int i = 0; i < Points; i++)
            {
                centerX += vertices[i * stride + 0];
                centerY += vertices[i * stride + 1];
                centerZ += vertices[i * stride + 2];
            }

            centerX /= Points;
            centerY /= Points;
            centerZ /= Points;

            for (int i = 0; i < Points; i++)
            {
                vertices[i * stride + 0] -= centerX;
                vertices[i * stride + 1] -= centerY;
                vertices[i * stride + 2] -= centerZ;
            }

            return vertices;
        }

        internal bool ShouldCenter() 
        {
            return _centerVertices;
        }
    }
}