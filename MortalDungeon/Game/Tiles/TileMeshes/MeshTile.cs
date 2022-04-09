using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Tiles.Meshes
{
    public class Vertex
    {
        public float[] Position = new float[3];
        public float Weight = 0;

        public Vector3 Normal = new Vector3();

        /// <summary>
        /// The Id of the vertex that will be used to connect it to other vertices <para/>
        /// 1 indexed
        /// </summary>
        public int Id = 1;

        /// <summary>
        /// Which vertex of the prototype mesh tile that this vertex represents 
        /// </summary>
        public int VertexOrder = 1;
        public Vertex(Span<float> position, int id)
        {
            for (int i = 0; i < 3; i++)
            {
                Position[i] = position[i];
            }

            Id = id;
        }
    }

    public class VertexPair
    {
        public VertexPair(int source, int neighbor)
        {
            Source = source;
            Neighbor = neighbor;
        }

        public int Source;
        public int Neighbor;
    }

    public class MeshTile
    {
        //how far offset all of the external vertices should be to ensure the center of the hexagon is at 0.5, 0.5
        //the magic numbers are "sqrt(3) / 2 * height" which is the formula for the height of a hexagon
        private const float HEIGHT_OFFSET_FULL = (1 - 0.8660254f) / 2;
        private const float HEIGHT_OFFSET_HALF = (1 - 0.4330127f) / 2;

        /// <summary>
        /// Outer vertices only
        /// </summary>
        public static float[] SIMPLE_VERTICES =
        {
            1/4f, 1 - HEIGHT_OFFSET_FULL, 0, //1
            3/4f, 1 - HEIGHT_OFFSET_FULL, 0, //2
            1f, 0.5f, 0, //3
            3/4f, HEIGHT_OFFSET_FULL, 0, //4
            1/4f, HEIGHT_OFFSET_FULL, 0, //5
            0, 0.5f, 0, //6
        };

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

        public static Dictionary<Direction, List<VertexPair>> NeighboringVertexPairs = new Dictionary<Direction, List<VertexPair>>
        {
            {
                Direction.South,
                new List<VertexPair>()
                {
                    new VertexPair(8, 0),
                    new VertexPair(7, 1),
                    new VertexPair(6, 2),
                }
            },
            {
                Direction.North,
                new List<VertexPair>()
                {
                    new VertexPair(0, 8),
                    new VertexPair(1, 7),
                    new VertexPair(2, 6),
                }
            },
            {
                Direction.SouthWest,
                new List<VertexPair>()
                {
                    new VertexPair(10, 2),
                    new VertexPair(9, 3),
                    new VertexPair(8, 4),
                }
            },
            {
                Direction.NorthWest,
                new List<VertexPair>()
                {
                    new VertexPair(0, 4),
                    new VertexPair(11, 5),
                    new VertexPair(10, 6),
                }
            },
            {
                Direction.SouthEast,
                new List<VertexPair>()
                {
                    new VertexPair(4, 0),
                    new VertexPair(5, 11),
                    new VertexPair(6, 10),
                }
            },
            {
                Direction.NorthEast,
                new List<VertexPair>()
                {
                    new VertexPair(2, 10),
                    new VertexPair(3, 9),
                    new VertexPair(4, 8),
                }
            },
        };

        public List<Vertex> Vertices = new List<Vertex>();

        /// <summary>
        /// Assigned when the MeshChunk is created by the TileChunk
        /// </summary>
        public Tile TileHandle = null;

        public int TileIndex;

        public MeshTile(Vector3 centerPos, int tilePos)
        {
            int idIndex = 0;
            int vertLength = VERTICES.Length / 3;

            TileIndex = tilePos;

            for (int i = 0; i < VERTICES.Length; i += 3)
            {
                var vert = new Vertex(VERTICES.AsSpan(i, 3), tilePos * vertLength + idIndex + 1);
                vert.Position[0] += centerPos.X;
                vert.Position[1] += centerPos.Y;
                vert.Position[2] += centerPos.Z;

                vert.VertexOrder = i / 3 + 1;

                Vertices.Add(vert);

                idIndex++;
            }
        }


        public void CreateOBJ()
        {
            using (StreamWriter obj = new StreamWriter("MeshTileTest.obj"))
            {
                obj.WriteLine("o MeshTile");

                for (int i = 0; i < VERTICES.Length; i += 3)
                {
                    obj.WriteLine($"v {VERTICES[i]} {VERTICES[i + 1]} {VERTICES[i + 2] + Vertices[i / 3].Weight}");
                }

                obj.WriteLine("s off");

                GetFaces(out var faces);

                foreach (var face in faces)
                {
                    obj.WriteLine($"f {face[0]}/{face[0]}/{face[0]} {face[1]}/{face[1]}/{face[1]} {face[2]}/{face[2]}/{face[2]}");
                }
            }
        }
        public void GetFaces(out List<List<float>> faces)
        {
            faces = new List<List<float>>();

            string facesString = @"1 13 2
            2 13 14
            2 14 15
            2 15 3
            3 15 4
            4 15 16
            4 16 17
            4 17 5
            5 17 6
            6 17 18
            6 18 19
            6 19 7
            7 19 8
            8 19 20
            8 20 21
            9 8 21
            9 21 10
            10 21 22
            10 22 23
            10 23 11
            11 23 12
            12 23 24
            12 24 13
            12 13 1
            13 25 14
            14 25 15
            15 25 16
            16 25 17
            17 25 18
            18 25 19
            19 25 20
            20 25 21
            21 25 22
            22 25 23
            23 25 24
            24 25 13";

            var lines = facesString.Split("\r", StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                faces.Add(new List<float>());
                string trimmedLine = line.Trim();
                var verts = trimmedLine.Split(" ");
                foreach(var vert in verts)
                {
                    faces[^1].Add(Vertices[int.Parse(vert) - 1].Id);
                }
            }
        }
    }
}
