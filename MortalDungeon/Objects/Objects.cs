using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Objects
{
    public class TextureInfo
    {
        public string[] Textures; //filename of texture/spritesheet
        public int[] TexturePositions; //which index of the spritesheet a texture resides in

        public Spritesheet Spritesheet;

        public TextureInfo(string texture, Spritesheet spritesheet = null) 
        {
            Textures = new string[] { texture };
            TexturePositions = new int[] { 0 };

            Spritesheet = spritesheet;
        }
        public TextureInfo(string[] textures, int[] positions, Spritesheet spritesheet = null)
        {
            Textures = textures;
            TexturePositions = positions;

            Spritesheet = spritesheet;
        }

        public TextureInfo(string texture, int position, Spritesheet spritesheet = null)
        {
            Textures = new string[] { texture };
            TexturePositions = new int[] { position };

            Spritesheet = spritesheet;
        }

        public TextureInfo(Spritesheet spritesheet, int[] positions )
        {
            string[] textures = new string[positions.Length];
            for(int i = 0; i < textures.Length; i++)
            {
                textures[i] = spritesheet.File;
            }
            Textures = textures;
            TexturePositions = positions;

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

        private bool _centerVertices;

        public ObjectIDs ID = ObjectIDs.Unknown;

        public ObjectDefinition(float[] vertices, uint[] indexes, int points, TextureInfo textures, Vector3 center = new Vector3(), float[] bounds = null, bool centerVertices = true)
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
        public ObjectDefinition() { }
        //Centers the vertices of the renderable object when defined (might want to move this to a different area at some point)
        public float[] CenterVertices(float[] vertices)
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

        public bool ShouldCenter() 
        {
            return _centerVertices;
        }
    }
}