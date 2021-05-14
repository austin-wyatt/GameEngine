using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Objects
{

    public class ObjectDefinition
    {
        public float[] Vertices;
        public uint[] Indices;
        public int Points;
        public string[] Textures;
        public Vector3 Center;
        public float[] Bounds;

        public ObjectDefinition(float[] vertices, uint[] indexes, int points, string[] textures, Vector3 center = new Vector3(), float[] bounds = null)
        {
            Indices = indexes;
            Points = points;
            Textures = textures;
            Center = center;
            Vertices = CenterVertices(vertices);


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
    }
    public static class CursorObjects
    {
        public static readonly ObjectDefinition MAIN_CURSOR = new ObjectDefinition(
            new float[]{
                0.5f, 0.5f, 0.0f, 1.0f, 0.0f, // top right
                 0.5f, -0.5f, 0.0f, 1.0f, 1.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // bottom left
                -0.5f, 0.5f, 0.0f, 0.0f, 0.0f // top left
            },
            new uint[]{ 
                0, 1, 3,
                1, 2, 3
            }, 
            4,
            "Resources/Cursor.png", 
            new Vector3(-1.5f, 1f, 0)
        );
    }

    public static class TestObjects
    {
        public static readonly ObjectDefinition BASIC_SQUARE = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f, 0.5f, 0.0f, 0.0f, 1.0f // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4, 
            "Resources/container.png"
        );

        public static readonly ObjectDefinition TEST_OBJECT = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f, 
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 
            -0.5f, 0.5f, 0.0f, 0.0f, 1.0f 
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            "Resources/container.png"
        );
    }

    public static class ButtonObjects
    {
        public static readonly ObjectDefinition BASIC_BUTTON = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -1.0f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -1.0f, 0.5f, 0.0f, 0.0f, 1.0f // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            "Resources/Button.png"
        );
    }

    public static class EnvironmentObjects
    {
        public static readonly ObjectDefinition TREE1 = new ObjectDefinition(
            new float[]{
            0.5f, 1.0f, 0.0f, 1.0f, 0.0f, // top right
            0.5f, -1.0f, 0.0f, 1.0f, 1.0f, // bottom right
            -0.5f, -1.0f, 0.0f, 0.0f, 1.0f, // bottom left
            -0.5f, 1.0f, 0.0f, 0.0f, 0.0f // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            "Resources/Tree.png",
            new Vector3(),
            new float[]{
                -0.12187499f, -0.9694444f, 0.0f,
                -0.095312476f, -0.8055556f, 0.0f,
                -0.0859375f, -0.5916667f, 0.0f,
                -0.08906251f, -0.36111116f, 0.0f,
                -0.22031248f, -0.44444442f, 0.0f,
                -0.36093748f, -0.5083333f, 0.0f,
                -0.4578125f, -0.4666667f, 0.0f,
                -0.4609375f, -0.35277772f, 0.0f,
                -0.390625f, -0.30555558f, 0.0f,
                -0.38437498f, -0.19722223f, 0.0f,
                -0.425f, -0.116666675f, 0.0f,
                -0.39218748f, -0.030555606f, 0.0f,
                -0.30312502f, 0.07499999f, 0.0f,
                -0.26875f, 0.18055558f, 0.0f,
                -0.19218749f, 0.21388888f, 0.0f,
                -0.1953125f, 0.32222223f, 0.0f,
                -0.16093749f, 0.39999998f, 0.0f,
                -0.1484375f, 0.5083333f, 0.0f,
                -0.07343751f, 0.64444447f, 0.0f,
                -0.009374976f, 0.71111107f, 0.0f,
                0.087499976f, 0.6944444f, 0.0f,
                0.15625f, 0.57777774f, 0.0f,
                0.18437505f, 0.4722222f, 0.0f,
                0.21249998f, 0.3611111f, 0.0f,
                0.22968745f, 0.28055555f, 0.0f,
                0.30781245f, 0.16666669f, 0.0f,
                0.32187498f, 0.06388891f, 0.0f,
                0.35312498f, -0.055555582f, 0.0f,
                0.35781252f, -0.15277779f, 0.0f,
                0.30937505f, -0.25833333f, 0.0f,
                0.49374998f, -0.3777778f, 0.0f,
                0.50468755f, -0.58055556f, 0.0f,
                0.4765625f, -0.5916667f, 0.0f,
                0.29375005f, -0.48611116f, 0.0f,
                0.16250002f, -0.41388893f, 0.0f,
                0.107812524f, -0.39166665f, 0.0f,
                0.12187505f, -0.57777774f, 0.0f,
                0.10625005f, -0.79999995f, 0.0f,
                0.07343745f, -0.9944445f, 0.0f,
                }
        );

        public static readonly ObjectDefinition HEXAGON_TILE = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f, 0.5f, 0.0f, 0.0f, 1.0f // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            "Resources/HexagonTile.png",
            new Vector3(),
            new float[]{
            -0.20468748f, -0.5055555f, 0.0f,
            -0.4875f, -0.26388884f, 0.0f,
            -0.49374998f, 0.16388887f, 0.0f,
            -0.17343748f, 0.48888886f, 0.0f,
            0.26718748f, 0.4861111f, 0.0f,
            0.50468755f, 0.14999998f, 0.0f,
            0.51093745f, -0.2527778f, 0.0f,
            0.21875f, -0.51388884f, 0.0f,
            }
        );

        public static readonly ObjectDefinition OCTAGON_TILE_TEST = new ObjectDefinition(
            new float[]{
                -0.1796875f, -0.49166667f, 0.0f, 0.3239436781943626f, 0.9863014267442344f,
                -0.503125f, -0.18333328f, 0.0f, 0.0f, 0.6821917551510579f,
                -0.5015625f, 0.19999999f, 0.0f, 0.0015649453052867979f, 0.3041095926890601f,
                -0.18906248f, 0.5083333f, 0.0f, 0.3145540263939418f, 0.0f,
                0.22500002f, 0.49166667f, 0.0f, 0.7292645322949374f, 0.01643832144116792f,
                0.49531245f, 0.17500001f, 0.0f, 1.0f, 0.328767109371363f,
                0.49374998f, -0.13888884f, 0.0f, 0.998435084741663f, 0.6383561392531408f,
                0.20624995f, -0.5055555f, 0.0f, 0.7104851185219463f, 1.0f,
                0.0f, 0.0f, 0.0f, 0.5f, 0.5f,
            },
            new uint[]{
            0, 1, 8,
            1, 2, 8,
            2, 3, 8,
            3, 4, 8,
            4, 5, 8,
            5, 6, 8,
            6, 7, 8,
            7, 0, 8,
            },
            9,
            "Resources/OctagonT.png",
            new Vector3(),
            new float[]{
                -0.1796875f, -0.49166667f, 0.0f,
                -0.503125f, -0.18333328f, 0.0f,
                -0.5015625f, 0.19999999f, 0.0f,
                -0.18906248f, 0.5083333f, 0.0f,
                0.22500002f, 0.49166667f, 0.0f,
                0.49531245f, 0.17500001f, 0.0f,
                0.49374998f, -0.13888884f, 0.0f,
                0.20624995f, -0.5055555f, 0.0f,
            }
        );
    }
}