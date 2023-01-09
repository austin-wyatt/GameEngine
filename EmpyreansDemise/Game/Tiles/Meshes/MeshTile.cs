using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Tiles.Meshes
{
    public struct DirectionalVertex
    {
        /// <summary>
        /// The direction of the tile that contains the equivalent vertex.
        /// </summary>
        public Direction VertexDirection;
        public int EquivalentVertex;

        public DirectionalVertex(Direction direction, int vertex)
        {
            VertexDirection = direction;
            EquivalentVertex = vertex;
        }
    }
    public struct EquivalentVertices
    {
        public int PrimaryVertex;
        public DirectionalVertex[] DirectionalVertices;
    }

    public class MeshTile
    {
        //how far offset all of the external vertices should be to ensure the center of the hexagon is at 0.5, 0.5
        //the magic numbers are "sqrt(3) / 2 * height" which is the formula for the height of a hexagon
        private const float HEIGHT_OFFSET_FULL = (1 - 0.8660254f) / 2;
        private const float HEIGHT_OFFSET_HALF = (1 - 0.4330127f) / 2;

        public const float TILE_HEIGHT = 0.8660254f;
        public const float TILE_WIDTH = 1f;

        public const float CHUNK_HEIGHT = 9.0932667397f;
        public const float CHUNK_WIDTH = 7.75f;


        /// <summary>
        /// The order (offset included) of the vertices of the outer hexagon of a mesh tile
        /// </summary>
        public static readonly int[] BOUNDING_VERTICES = new int[]
        {
            0,
            2 * VERTEX_OFFSET,
            4 * VERTEX_OFFSET,
            6 * VERTEX_OFFSET,
            8 * VERTEX_OFFSET,
            10 * VERTEX_OFFSET,
            0
        };

        public const int VERTEX_COUNT = 25;
        public const int FACES_PER_TILE = 36;

        public static float[] VERTICES =
        {
            1/4f, 1 - HEIGHT_OFFSET_FULL, 0, //1
            2/4f, 1 - HEIGHT_OFFSET_FULL, 0, //2
            3/4f, 1 - HEIGHT_OFFSET_FULL, 0, //3
            7/8f, (1 - HEIGHT_OFFSET_FULL + 0.5f) * 0.5f, 0, //4
            1f, 0.5f, 0, //5
            7/8f, (HEIGHT_OFFSET_FULL + 0.5f) * 0.5f, 0, //6
            3/4f, HEIGHT_OFFSET_FULL, 0, //7
            2/4f, HEIGHT_OFFSET_FULL, 0, //8
            1/4f, HEIGHT_OFFSET_FULL, 0, //9
            1/8f, (HEIGHT_OFFSET_FULL + 0.5f) * 0.5f, 0, //10
            0, 0.5f, 0, //11
            1/8f, (1 - HEIGHT_OFFSET_FULL + 0.5f) * 0.5f, 0, //12


            3/8f, 1 - HEIGHT_OFFSET_HALF, 0, //13
            4/8f, 1 - HEIGHT_OFFSET_HALF, 0, //14
            5/8f, 1 - HEIGHT_OFFSET_HALF, 0, //15
            11/16f, (1 - HEIGHT_OFFSET_HALF + 0.5f) * 0.5f, 0, //16
            12/16f, 0.5f, 0, //17
            11/16f, (HEIGHT_OFFSET_HALF + 0.5f) * 0.5f, 0, //18
            5/8f, HEIGHT_OFFSET_HALF, 0, //19
            4/8f, HEIGHT_OFFSET_HALF, 0, //20
            3/8f, HEIGHT_OFFSET_HALF, 0, //21
            5/16f, (HEIGHT_OFFSET_HALF + 0.5f) * 0.5f, 0, //22
            4/16f, 0.5f, 0, //23
            5/16f, (1 - HEIGHT_OFFSET_HALF + 0.5f) * 0.5f, 0, //24


            0.5f, 0.5f, 0, //25
        };

        /// <summary>
        /// 0 indexed list of equivalent vertices. Only 1, 2, 3, 4, and 5 are included since these provide full coverage of all vertices <para/>
        /// To use, index the array with the primary vertex id.
        /// </summary>
        public static EquivalentVertices[] EquivalentVertices = new EquivalentVertices[]
        {
            new EquivalentVertices(), //empty entry so that the array can be directly indexed by vertex id
            new EquivalentVertices()
            {
                PrimaryVertex = 1,
                DirectionalVertices = new DirectionalVertex[]
                {
                    new DirectionalVertex(Direction.North, 7),
                }
            },
            new EquivalentVertices()
            {
                PrimaryVertex = 2,
                DirectionalVertices = new DirectionalVertex[]
                {
                    new DirectionalVertex(Direction.North, 6),
                    new DirectionalVertex(Direction.NorthEast, 10),
                }
            },
            new EquivalentVertices()
            {
                PrimaryVertex = 3,
                DirectionalVertices = new DirectionalVertex[]
                {
                    new DirectionalVertex(Direction.NorthEast, 9),
                }
            },
            new EquivalentVertices()
            {
                PrimaryVertex = 4,
                DirectionalVertices = new DirectionalVertex[]
                {
                    new DirectionalVertex(Direction.NorthEast, 8),
                    new DirectionalVertex(Direction.SouthEast, 0),
                }
            },
            new EquivalentVertices()
            {
                PrimaryVertex = 5,
                DirectionalVertices = new DirectionalVertex[]
                {
                    new DirectionalVertex(Direction.SouthEast, 11),
                }
            },
        };

        /// <summary>
        /// This is a bit spaghetti but it will save on a bit of memory and CPU time
        /// in the vertex blending process. <para/>
        /// Each outer vertex of a mesh tile is offset from its inner vertex by the inner
        /// vertex offset, so by adding the offset to the vertex id we get the id of the inner vertex.
        /// </summary>
        public const int INNER_VERTEX_OFFSET = 12;

        /// <summary>
        /// The amount of floats stored for each vertex in the main vertex data array.
        /// </summary>
        public const int VERTEX_OFFSET = 15; //position, texture coords, normal

        /// <summary>
        /// Assigned when the MeshChunk is created by the TileChunk
        /// </summary>
        public Tile TileHandle = null;

        public int TileIndex;

        /// <summary>
        /// The last weight will always be the height of the tile.
        /// </summary>
        public float[] Weights = new float[25];

        /// <summary>
        /// Points directly to the MeshChunk's vertex data array.
        /// </summary>
        public float[] VerticesHandle;

        public MeshChunk MeshChunk;

        public MeshTile(Vector3 centerPos, int tilePos, ref float[] vertices, Tile tile, MeshChunk chunk)
        {
            tile.MeshTileHandle = this;
            TileHandle = tile;

            MeshChunk = chunk;

            InitializeTile(centerPos, tilePos, ref vertices);
        }

        public void InitializeTile(Vector3 centerPos, int tilePos, ref float[] vertices)
        {
            TileIndex = tilePos;

            int offset = GetVertexOffset();

            VerticesHandle = vertices;

            //Vector2i tileCoords = new Vector2i(tilePos / MeshChunk.Height, tilePos % MeshChunk.Height);

            //float xBlendOffset = tileCoords.X * 0.75f / CHUNK_WIDTH;
            //float yBlendOffset = tileCoords.Y * TILE_HEIGHT / CHUNK_HEIGHT;
            //if (tileCoords.X % 2 == 0)
            //{
            //    yBlendOffset += TILE_HEIGHT * 0.5f / CHUNK_HEIGHT;
            //}

            for (int i = 0; i < VERTICES.Length; i += 3)
            {
                vertices[offset] = VERTICES[i] + centerPos.X; //X position
                vertices[offset + 1] = VERTICES[i + 1] + centerPos.Y; //Y position
                vertices[offset + 2] = VERTICES[i + 2] + centerPos.Z; //Z position

                vertices[offset + 3] = VERTICES[i]; //X texture coordinate
                vertices[offset + 4] = VERTICES[i + 1]; //Y texture coordinate

                vertices[offset + 5] = 0; //| Default normal value is just a vector pointing in the positive Z direction
                vertices[offset + 6] = 0; //|
                vertices[offset + 7] = 1; //|

                //vertices[offset + 8] = VERTICES[i] / CHUNK_WIDTH + xBlendOffset; //X blend map texture coordinate
                //vertices[offset + 9] = (((1 - HEIGHT_OFFSET_FULL) - VERTICES[i + 1]) / CHUNK_HEIGHT + yBlendOffset); //Y blend map texture coordinate

                vertices[offset + 10] = TileHandle.Color.X; //|
                vertices[offset + 11] = TileHandle.Color.Y; //|
                vertices[offset + 12] = TileHandle.Color.Z; //|
                vertices[offset + 13] = TileHandle.Color.W; //|
                vertices[offset + 14] = TileHandle.ColorMixPercent;

                offset += VERTEX_OFFSET;
            }

            if(TileHandle.Properties.Height != 0)
            {
                SetHeight(TileHandle.Properties.Height);
            }

            UpdateTextureInfo();
        }

        /// <summary>
        /// Sets the vertex's normal data to the passed normal vector. 
        /// </summary>
        public void SetNormal(int vertIndex, Vector3 normal)
        {
            int offset = GetVertexOffset();
            offset += vertIndex * VERTEX_OFFSET;

            VerticesHandle[offset + 5] = normal.X;
            VerticesHandle[offset + 6] = normal.Y;
            VerticesHandle[offset + 7] = normal.Z;
        }

        /// <summary>
        /// Applies all of a tile's weights to the vertex data.
        /// </summary>
        /// <param name="vertices"></param>
        public void ApplyWeights()
        {
            int offset = GetVertexOffset();

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                VerticesHandle[offset + 2] = Weights[i];

                offset += VERTEX_OFFSET;
            }
        }

        public void ApplyWeight(int index)
        {
            int offset = GetVertexOffset() + index * VERTEX_OFFSET;

            VerticesHandle[offset + 2] = Weights[index];
        }

        public void SetHeight(float height, bool resetWeights = true)
        {
            int offset = GetVertexOffset();

            if (resetWeights)
            {
                for (int i = 0; i < VERTEX_COUNT; i++)
                {
                    Weights[i] = height;

                    VerticesHandle[offset + 2] = Weights[i];

                    offset += VERTEX_OFFSET;
                }
            }
            else
            {
                Weights[24] = height;
                VerticesHandle[offset + VERTEX_OFFSET * (VERTEX_COUNT - 1) + 2] = Weights[24];
            }
        }

        public void SetColor(ref Vector4 color, float mixPercent)
        {
            int offset = GetVertexOffset();

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                VerticesHandle[offset + 10] = color.X;
                VerticesHandle[offset + 11] = color.Y;
                VerticesHandle[offset + 12] = color.Z;
                VerticesHandle[offset + 13] = color.W;
                VerticesHandle[offset + 14] = mixPercent;

                offset += VERTEX_OFFSET;
            }
        }

        public void UpdateTextureInfo()
        {
            //int offset = GetVertexOffset();
            ////take the tile and update the correct entry in the TextureInfo array
            //if (TileHandle == null)
            //{
            //    for(int i = 0; i < VERTEX_COUNT; i++)
            //    {
            //        //VerticesHandle[offset + 8] = 0;
            //        VerticesHandle[offset + 9] = 0;

            //        offset += VERTEX_OFFSET;
            //    }
            //}
            //else
            //{
            //    if (TileHandle.Properties.DisplayInfo.Texture.Texture == null)
            //    {
            //        return;
            //    }

            //    int texHandle = TileHandle.Properties.DisplayInfo.Texture.Texture.Handle;

            //    //int spritesheetPos = TileHandle.Properties.DisplayInfo.SpritesheetPos;
            //    int texIndex;

            //    //VerticesHandle[offset + 8] = TileHandle.Properties.DisplayInfo.SpritesheetPos;

            //    if (MeshChunk.TextureIndexMap.TryGetValue(texHandle, out var foundTexIndex))
            //    {
            //        texIndex = foundTexIndex;
            //    }
            //    else
            //    {
            //        texIndex = MeshChunk.UsedTextureHandles.Count;
            //        MeshChunk.TextureIndexMap.Add(texHandle, texIndex);
            //        MeshChunk.UsedTextureHandles.Add(texHandle);
            //    }

            //    for (int i = 0; i < VERTEX_COUNT; i++)
            //    {
            //        //VerticesHandle[offset + 8] = spritesheetPos;
            //        VerticesHandle[offset + 9] = texIndex;

            //        offset += VERTEX_OFFSET;
            //    }
            //}
        }

        private static ObjectPool<List<int>> _intListPool = new ObjectPool<List<int>>();
        private static ObjectPool<List<Vector3>> _vectorListPool = new ObjectPool<List<Vector3>>();
        public void CalculateNormal()
        {
            int baseOffset = GetVertexOffset();
            int offset = baseOffset;

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                Vector3 summedNormal = new Vector3(0, 0, 1);

                Vector3 p0 = new Vector3(VerticesHandle[offset],
                        VerticesHandle[offset + 1], VerticesHandle[offset + 2]);

                
                var mappedFaces = MeshChunk.FaceMap[i];

                if (mappedFaces != null)
                {
                    //reset the normal to 0 if we do have faces available
                    summedNormal.Z = 0;
                }

                foreach (var face in mappedFaces)
                {
                    List<int> connectedVertexOffsets = _intListPool.GetObject();
                    List<Vector3> vectors = _vectorListPool.GetObject();

                    for(int j = 0; j < face.VertexIds.Count; j++)
                    {
                        if (face.VertexIds[j] != i)
                        {
                            int connectedOffset = baseOffset + face.VertexIds[j] * VERTEX_OFFSET;
                            vectors.Add(new Vector3(
                                VerticesHandle[connectedOffset],
                                VerticesHandle[connectedOffset + 1],
                                VerticesHandle[connectedOffset + 2]));
                        }
                        else
                        {
                            vectors.Add(p0);
                        }
                    }


                    //vector from point A to point B is B - A
                    //Vector3 v1 = p1 - p0;
                    //Vector3 v2 = p2 - p0;

                    //the normal of 2 vectors is the cross product and they can be summed together before normalizing
                    //to easily weight them via magnitude (ie larger faces will contribute more to the resulting normal)
                    summedNormal += Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[0]);

                    vectors.Clear();
                    _vectorListPool.FreeObject(ref vectors);
                    _intListPool.FreeObject(ref connectedVertexOffsets);
                }

                //summedNormal.Z = Math.Abs(summedNormal.Z);

                //normalize the vector
                summedNormal.Normalize();

                SetNormal(i, summedNormal);

                offset += VERTEX_OFFSET;
            }
        }

        /// <summary>
        /// Returns the position of the 0th index of the current tile in the chunk vertices array.
        /// </summary>
        public int GetVertexOffset()
        {
            return TileIndex * VERTEX_OFFSET * VERTEX_COUNT;
        }

        public static uint[] FACES = new uint[]{
            0, 12, 1,
            1, 12, 13,
            1, 13, 14,
            1, 14, 2,
            2, 14, 3,
            3, 14, 15,
            3, 15, 16,
            3, 16, 4,
            4, 16, 5,
            5, 16, 17,
            5, 17, 18,
            5, 18, 6,
            6, 18, 7,
            7, 18, 19,
            7, 19, 20,
            8, 7, 20,
            8, 20, 9,
            9, 20, 21,
            9, 21, 22,
            9, 22, 10,
            10, 22, 11,
            11, 22, 23,
            11, 23, 12,
            11, 12, 0,
            12, 24, 13,
            13, 24, 14,
            14, 24, 15,
            15, 24, 16,
            16, 24, 17,
            17, 24, 18,
            18, 24, 19,
            19, 24, 20,
            20, 24, 21,
            21, 24, 22,
            22, 24, 23,
            23, 24, 12
        };
        public void GetFaces(out List<List<float>> faces)
        {
            faces = new List<List<float>>();

            string facesString = @"0 12 1
                1 12 13
                1 13 14
                1 14 2
                2 14 3
                3 14 15
                3 15 16
                3 16 4
                4 16 5
                5 16 17
                5 17 18
                5 18 6
                6 18 7
                7 18 19
                7 19 20
                8 7 20
                8 20 9
                9 20 21
                9 21 22
                9 22 10
                10 22 11
                11 22 23
                11 23 12
                11 12 0
                12 24 13
                13 24 14
                14 24 15
                15 24 16
                16 24 17
                17 24 18
                18 24 19
                19 24 20
                20 24 21
                21 24 22
                22 24 23
                23 24 12";

            var lines = facesString.Split("\r", StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                faces.Add(new List<float>());
                string trimmedLine = line.Trim();
                var verts = trimmedLine.Split(" ");
                foreach(var vert in verts)
                {
                    faces[^1].Add(int.Parse(vert));
                }
            }
        }
    }
}
