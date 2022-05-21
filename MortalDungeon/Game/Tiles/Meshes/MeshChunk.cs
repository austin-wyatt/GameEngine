using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Tiles.Meshes
{
    public class Face
    {
        public List<int> VertexIds = new List<int>();

        public Face(float v1, float v2, float v3)
        {
            VertexIds.Add((int)v1); 
            VertexIds.Add((int)v2); 
            VertexIds.Add((int)v3);
        }
    }

    public class TileFaceGroup
    {
        public List<Face> Faces;
        public Tile Tile;
    }

    public class MeshChunk
    {
        public int Width = 10;
        public int Height = 10;

        public List<List<MeshTile>> MeshTiles = new List<List<MeshTile>>();

        //public SortedDictionary<int, Vertex> VertexMap = new SortedDictionary<int, Vertex>();
        public static ConcurrentDictionary<int, List<Face>> FaceMap = new ConcurrentDictionary<int, List<Face>>();
        public static List<TileFaceGroup> FaceList = new List<TileFaceGroup>();
        private static bool _staticDataInitialized = false;

        public TransformableMesh Mesh = new TransformableMesh();



        /// <summary>
        /// The bottom left corner of the mesh in local coordinates
        /// </summary>
        public Vector3 Origin = new Vector3();

        /// <summary>
        /// The face draw order for visible tiles
        /// </summary>
        public uint[] VisionDrawOrder;
        public int VisionDrawOrderLength = 0;

        /// <summary>
        /// The face draw order for out of vision tiles
        /// </summary>
        public uint[] FogDrawOrder;
        public int FogDrawOrderLength = 0;

        /// <summary>
        /// The list of texture handles that are being used by the MeshChunk in order <para/>
        /// This is used to set the correct texture uniform locations for the mesh
        /// </summary>
        public List<int> UsedTextureHandles = new List<int>();
        public Dictionary<int, int> TextureIndexMap = new Dictionary<int, int>();

        public MeshChunk()
        {

        }

        public MeshChunk(List<Tile> tiles, int width = 10, int height = 10)
        {
            Width = width;
            Height = height;

            Mesh.Vertices = new float[MeshTile.VERTEX_OFFSET * MeshTile.VERTEX_COUNT * Width * Height];
            VisionDrawOrder = new uint[width * height * 3 /*vertices per face*/ * 36 /*faces per tile*/];
            FogDrawOrder = new uint[width * height * 3 /*vertices per face*/ * 36 /*faces per tile*/];

            FillChunk(tiles);
        }

        public void EmptyData()
        {
            Mesh.Vertices = new float[0];
            Mesh.VertexDrawOrder = new uint[0];

            UsedTextureHandles.Clear();
            TextureIndexMap.Clear();

            VisionDrawOrder = new uint[0];
            FogDrawOrder = new uint[0];

            MeshTiles.Clear();
        }

        public void FillChunk(List<Tile> tiles)
        {
            const float xIncrement = 0.75f;
            const float yIncrement = -0.8660254f; //-sqrt(3) / 2

            Vector3 tilePos = new Vector3();

            int rowCount = 0;
            for (int i = 0; i < Width; i++)
            {
                List<MeshTile> meshTiles = new List<MeshTile>();
                MeshTiles.Add(meshTiles);

                for (int j = 0; j < Height; j++)
                {
                    MeshTile meshTile;

                    if (tiles[i * Height + j].MeshTileHandle == null)
                    {
                        meshTile = new MeshTile(tilePos, i * Height + j, ref Mesh.Vertices, tiles[i * Height + j], this);
                    }
                    else
                    {
                        meshTile = tiles[i * Height + j].MeshTileHandle;
                        meshTile.InitializeTile(tilePos, i * Height + j, ref Mesh.Vertices);
                    }

                    meshTiles.Add(meshTile);

                    tilePos.Y += yIncrement;
                }

                rowCount++;

                tilePos.Y = rowCount % 2 == 0 ? 0 : -yIncrement * 0.5f;
                tilePos.X += xIncrement;
            }

            lock (_staticDataLock)
            {
                if (!_staticDataInitialized)
                {
                    FillFaceList();
                    _staticDataInitialized = true;
                }
            }
        }

        private static object _staticDataLock = new object();

        public void FillFaceList()
        {
            bool faceMapFilled = false;

            foreach (var column in MeshTiles)
            {
                foreach (var tile in column)
                {
                    tile.GetFaces(out var faces);

                    TileFaceGroup group = new TileFaceGroup();
                    group.Faces = new List<Face>();
                    group.Tile = tile.TileHandle;

                    foreach (var faceList in faces)
                    {
                        Face face = new Face(faceList[0], faceList[1], faceList[2]);

                        group.Faces.Add(face);


                        //add each connection to the FaceMap
                        if (!faceMapFilled)
                        {
                            foreach (var fId in faceList)
                            {
                                int id = (int)fId;

                                if (FaceMap.TryGetValue(id, out var mappedFaces))
                                {
                                    mappedFaces.Add(face);
                                }
                                else
                                {
                                    FaceMap.TryAdd(id, new List<Face>() { face });
                                }
                            }
                        }
                    }

                    if (group.Faces.Count > 0)
                    {
                        FaceList.Add(group);
                    }

                    faceMapFilled = true;
                }
            }
        }

        public void CalculateVertexNormals()
        {
            foreach (var column in MeshTiles)
            {
                foreach (var tile in column)
                {
                    CalculateNormalsForTile(tile);
                }
            }
        }

        public void CalculateNormalsForTile(MeshTile tile)
        {
            int baseOffset = tile.GetVertexOffset();
            int offset = baseOffset;

            for (int i = 0; i < MeshTile.VERTEX_COUNT; i++)
            {
                Vector3 summedNormal = new Vector3(0, 0, 1);

                Vector3 p0 = new Vector3(Mesh.Vertices[offset],
                        Mesh.Vertices[offset + 1], Mesh.Vertices[offset + 2]);

                var mappedFaces = FaceMap[i];

                if (mappedFaces != null)
                {
                    //reset the normal to 0 if we do have faces available
                    summedNormal.Z = 0;
                }

                foreach (var face in mappedFaces)
                {
                    List<int> connectedVertexOffsets = new List<int>(2);

                    foreach (var vId in face.VertexIds)
                    {
                        //if (vId != vertex.Id)
                        //    connectedVertices.Add(VertexMap[vId]);
                        if (vId != i)
                            connectedVertexOffsets.Add(baseOffset + vId * MeshTile.VERTEX_OFFSET);
                    }

                    Vector3 p1 = new Vector3(
                        Mesh.Vertices[connectedVertexOffsets[0]],
                        Mesh.Vertices[connectedVertexOffsets[0] + 1],
                        Mesh.Vertices[connectedVertexOffsets[0] + 2]);

                    Vector3 p2 = new Vector3(
                        Mesh.Vertices[connectedVertexOffsets[1]],
                        Mesh.Vertices[connectedVertexOffsets[1] + 1],
                        Mesh.Vertices[connectedVertexOffsets[1] + 2]);

                    //vector from point A to point B is simply B - A
                    Vector3 v1 = p1 - p0;
                    Vector3 v2 = p2 - p0;

                    //the normal of 2 vectors is the cross product and they can be summed together before normalizing
                    //to easily weight them via magnitude (ie larger faces will contribute more to the resulting normal)
                    summedNormal += Vector3.Cross(v1, v2);
                }

                //normalize the vector
                summedNormal.Normalize();

                tile.SetNormal(i, summedNormal);

                offset += MeshTile.VERTEX_OFFSET;
            }
        }

        public List<(Direction Dir, MeshTile Tile)> GetNeighboringTiles(Vector2i tileCoord)
        {
            List<(Direction Dir, MeshTile Tile)> tileList = new List<(Direction Dir, MeshTile Tile)>();

            bool isEvenTile = tileCoord.X % 2 == 0;
            //int yOffset = isEvenTile ? 1 : 0;
            int yOffset = !isEvenTile ? 1 : 0;


            foreach (var direction in Enum.GetValues(typeof(Direction)))
            {
                switch (direction)
                {
                    case Direction.South:
                        if (tileCoord.Y < Height - 1)
                        {
                            tileList.Add((Direction.South, MeshTiles[tileCoord.X][tileCoord.Y + 1]));
                        }
                        break;
                    case Direction.North:
                        if (tileCoord.Y > 0)
                        {
                            tileList.Add((Direction.North, MeshTiles[tileCoord.X][tileCoord.Y - 1]));
                        }
                        break;
                    case Direction.NorthEast:
                        if (tileCoord.Y - yOffset >= 0 && tileCoord.X < Width - 1)
                        {
                            tileList.Add((Direction.NorthEast, MeshTiles[tileCoord.X + 1][tileCoord.Y - yOffset]));
                        }
                        break;
                    case Direction.NorthWest:
                        if (tileCoord.Y - yOffset >= 0 && tileCoord.X > 0)
                        {
                            tileList.Add((Direction.NorthWest, MeshTiles[tileCoord.X - 1][tileCoord.Y - yOffset]));
                        }
                        break;
                    case Direction.SouthEast:
                        if (tileCoord.Y - yOffset + 1 < Width && tileCoord.X < Width - 1)
                        {
                            tileList.Add((Direction.SouthEast, MeshTiles[tileCoord.X + 1][tileCoord.Y - yOffset + 1]));
                        }
                        break;
                    case Direction.SouthWest:
                        if (tileCoord.Y - yOffset + 1 < Width && tileCoord.X > 0)
                        {
                            tileList.Add((Direction.SouthWest, MeshTiles[tileCoord.X - 1][tileCoord.Y - yOffset + 1]));
                        }
                        break;
                }
            }

            return tileList;
        }

        public void InitializeMesh()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Maybe only calculate normals when something is done that requires calculating normals? 
            //Ie: blending involving a tile, height changes, etc
            //CalculateVertexNormals();

            //Console.WriteLine($"Normals {stopwatch.ElapsedMilliseconds}ms");

            UpdateDrawOrder();

            //Console.WriteLine($"Mesh chunk initialized in {stopwatch.ElapsedMilliseconds}ms");
        }


        /// <summary>
        /// Updates the fog and visible draw order arrays.
        /// </summary>
        public void UpdateDrawOrder()
        {
            //36 faces per tile

            int tileOffset;
            TileFaceGroup group;
            MeshTile meshTile;

            FogDrawOrderLength = 0;
            VisionDrawOrderLength = 0;

            int j;
            int i;

            for (j = 0; j < FaceList.Count; j++)
            {
                group = FaceList[j];

                tileOffset = group.Tile.MeshTileHandle.TileIndex * MeshTile.VERTEX_COUNT;

                meshTile = MeshTiles[group.Tile.MeshTileHandle.TileIndex / Height][group.Tile.MeshTileHandle.TileIndex % Height];

                if (meshTile.TileHandle.InFog(VisionManager.Scene.VisibleTeam))
                {
                    for (i = 0; i < group.Faces.Count; i++)
                    {
                        FogDrawOrder[FogDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[0] + tileOffset);
                        FogDrawOrder[FogDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[1] + tileOffset);
                        FogDrawOrder[FogDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[2] + tileOffset);
                    }
                }
                else
                {
                    for (i = 0; i < group.Faces.Count; i++)
                    {
                        VisionDrawOrder[VisionDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[0] + tileOffset);
                        VisionDrawOrder[VisionDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[1] + tileOffset);
                        VisionDrawOrder[VisionDrawOrderLength++] = (uint)(group.Faces[i].VertexIds[2] + tileOffset);
                    }
                }
            }
        }


        /// <summary>
        /// Make a pass through all of the tiles and blend any vertices between them if necessary. <para/>
        /// If a tile that requires blending lies on the edge of the chunk then the adjacent chunk(s) should
        /// be determined and blended as well. <para/>
        /// Normals need to be recalculated at the end of this process as well.
        /// </summary>
        public void BlendVertices()
        {

        }
    }
}
