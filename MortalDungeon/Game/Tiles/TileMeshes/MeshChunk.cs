using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Tiles.Meshes
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

        public SortedDictionary<int, Vertex> VertexMap = new SortedDictionary<int, Vertex>();
        public Dictionary<int, List<Face>> FaceMap = new Dictionary<int, List<Face>>();
        public List<TileFaceGroup> FaceList = new List<TileFaceGroup>();

        public TransformableMesh Mesh = new TransformableMesh();

        /// <summary>
        /// The face draw order for visible tiles
        /// </summary>
        public uint[] VisionDrawOrder;
        /// <summary>
        /// The face draw order for out of vision tiles
        /// </summary>
        public uint[] FogDrawOrder;

        /// <summary>
        /// The list of texture handles that are being used by the MeshChunk in order <para/>
        /// This is used to set the correct texture uniform locations for the mesh
        /// </summary>
        public List<int> UsedTextureHandles = new List<int>();

        /// <summary>
        /// Contains the spritesheet position and uniform position of the texture for each mesh tile <para/>
        /// This will be applied once for every MeshTile.VERTICES.Length vertices in the instanced render data
        /// </summary>
        public float[] TextureInfo;

        public MeshChunk()
        {

        }

        public MeshChunk(List<Tile> tiles, int width = 10, int height = 10)
        {
            Width = width;
            Height = height;

            FillChunk(tiles);
        }

        public void FillChunk(List<Tile> tiles)
        {
            for (int i = 0; i < Width; i++)
            {
                List<MeshTile> meshTiles = new List<MeshTile>();
                MeshTiles.Add(meshTiles);

                for (int j = 0; j < Height; j++)
                {
                    MeshTile meshTile = new MeshTile(new Vector3(0, 0, 0), i * Height + j);

                    meshTile.TileHandle = tiles[i * Height + j];
                    tiles[i * Height + j].MeshTileHandle = meshTile;

                    meshTiles.Add(meshTile);
                }
            }

            PositionTiles();
            MergeTileVertices();
            FillFaceMap();
        }

        public void PositionTiles()
        {
            float xIncrement = 0.75f;
            float yIncrement = -(float)Math.Sqrt(3) / 2;

            float x = 0;
            float y = 0;

            int rowCount = 0;
            foreach (var row in MeshTiles)
            {
                foreach (var tile in row)
                {
                    for (int j = 0; j < tile.Vertices.Count; j++)
                    {
                        tile.Vertices[j].Position[0] = MeshTile.VERTICES[j * 3] + x;
                        tile.Vertices[j].Position[1] = MeshTile.VERTICES[j * 3 + 1] + y;
                        tile.Vertices[j].Position[2] = MeshTile.VERTICES[j * 3 + 2] + tile.Vertices[j].Weight;
                    }

                    y += yIncrement;
                }

                rowCount++;

                y = rowCount % 2 == 0 ? 0 : -yIncrement * 0.5f;
                x += xIncrement;
            }
        }

        public void MergeTileVertices()
        {
            int x = 0;
            int y = 0;
            foreach (var column in MeshTiles)
            {
                foreach (var tile in column)
                {
                    var neighboringTiles = GetNeighboringTiles(new Vector2i(x, y));

                    //Combine all shared vertices into a single vertex (orphaned vertices should just be garbage collected)
                    foreach (var neighboringTile in neighboringTiles)
                    {
                        var vertexPairs = MeshTile.NeighboringVertexPairs[neighboringTile.Dir];

                        foreach (var vertexPair in vertexPairs)
                        {
                            neighboringTile.Tile.Vertices[vertexPair.Neighbor] = tile.Vertices[vertexPair.Source];
                        }
                    }

                    //Add the current tile's vertices to the vertex map
                    foreach (var vertex in tile.Vertices)
                    {
                        if (!VertexMap.ContainsKey(vertex.Id))
                        {
                            VertexMap.Add(vertex.Id, vertex);
                        }
                    }

                    y++;
                }

                y = 0;
                x++;
            }
        }

        public void FillVertices()
        {
            Mesh.Vertices = new float[Width * Height * MeshTile.VERTICES.Length];

            int i = 0;

            foreach (var row in MeshTiles)
            {
                foreach (var tile in row)
                {
                    for (int j = 0; j < tile.Vertices.Count; j++)
                    {
                        Mesh.Vertices[i++] = tile.Vertices[j].Position[0];
                        Mesh.Vertices[i++] = tile.Vertices[j].Position[1];
                        Mesh.Vertices[i++] = tile.Vertices[j].Position[2] + tile.Vertices[j].Weight;
                    }
                }
            }
        }

        public void FillFaceMap()
        {
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
                        foreach (var fId in faceList)
                        {
                            int id = (int)fId;

                            if (FaceMap.TryGetValue(id, out var mappedFaces))
                            {
                                mappedFaces.Add(face);
                            }
                            else
                            {
                                FaceMap.Add(id, new List<Face>() { face });
                            }
                        }
                    }

                    if(group.Faces.Count > 0)
                    {
                        FaceList.Add(group);
                    }
                }
            }
        }

        public void CalculateVertexNormals()
        {
            foreach (var column in MeshTiles)
            {
                foreach (var tile in column)
                {
                    foreach (var vertex in tile.Vertices)
                    {
                        Vector3 summedNormal = new Vector3(0, 0, 1);

                        Vector3 p0 = new Vector3(vertex.Position[0],
                                vertex.Position[1], vertex.Position[2] + vertex.Weight);

                        var mappedFaces = FaceMap[vertex.Id];

                        if (mappedFaces != null)
                        {
                            //reset the normal to 0 if we do have faces available
                            summedNormal.Z = 0;
                        }

                        foreach (var face in mappedFaces)
                        {
                            List<Vertex> connectedVertices = new List<Vertex>(2);

                            foreach (var vId in face.VertexIds)
                            {
                                if (vId != vertex.Id)
                                    connectedVertices.Add(VertexMap[vId]);
                            }

                            Vector3 p1 = new Vector3(connectedVertices[0].Position[0],
                                connectedVertices[0].Position[1], connectedVertices[0].Position[2] + connectedVertices[0].Weight);

                            Vector3 p2 = new Vector3(connectedVertices[1].Position[0],
                                connectedVertices[1].Position[1], connectedVertices[1].Position[2] + connectedVertices[1].Weight);

                            //vector from point A to point B is simply B - A
                            Vector3 v1 = p1 - p0;
                            Vector3 v2 = p2 - p0;

                            //the normal of 2 vectors is the cross product and they can be summed together before normalizing
                            //to easily weight them via magnitude (ie larger faces will contribute more to the resulting normal)
                            summedNormal += Vector3.Cross(v1, v2);
                        }

                        //normalize the vector
                        summedNormal.Normalize();

                        vertex.Normal = summedNormal;
                    }
                }
            }
        }

        //public void tempRaiseTile()
        //{
        //    Random rand = new Random();

        //    int tileToRaise = rand.Next(Width * Height);
        //    //int tileToRaise = 70;

        //    float height = (float)rand.NextDouble();

        //    MeshTile tile = MeshTiles[tileToRaise / Height][tileToRaise % Height];

        //    for(int i = 0; i < tile.Vertices.Count; i++)
        //    {
        //        tile.Vertices[i].Weight = height;
        //    }

        //    FillVertices();
        //}

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

        //public void CreateOBJ()
        //{
        //    CalculateVertexNormals();

        //    using (StreamWriter obj = new StreamWriter("MeshChunkTest.obj"))
        //    {
        //        obj.WriteLine("o MeshChunk");

        //        //insert vertices
        //        for (int i = 0; i < Vertices.Length; i += 3)
        //        {
        //            obj.WriteLine($"v {Vertices[i]} {Vertices[i + 1]} {Vertices[i + 2]}");
        //        }

        //        obj.WriteLine("s on");
        //        //obj.WriteLine("s off");

        //        //insert vertex normals
        //        foreach (var row in MeshTiles)
        //        {
        //            foreach (var tile in row)
        //            {
        //                for (int j = 0; j < tile.Vertices.Count; j++)
        //                {
        //                    obj.WriteLine($"vn {tile.Vertices[j].Normal.X} {tile.Vertices[j].Normal.Y} {tile.Vertices[j].Normal.Z}");
        //                }
        //            }
        //        }

        //        //insert vertex textures
        //        for (int i = 0; i < MeshTile.VERTICES.Length; i += 3)
        //        {
        //            obj.WriteLine($"vt {MeshTile.VERTICES[i]} {MeshTile.VERTICES[i + 1]}");
        //        }

        //        //insert faces
        //        foreach (var mappedFace in FaceMap)
        //        {
        //            foreach(var face in mappedFace.Value)
        //            {
        //                var v0 = VertexMap[face.VertexIds[0]];
        //                var v1 = VertexMap[face.VertexIds[1]];
        //                var v2 = VertexMap[face.VertexIds[2]];

        //                obj.WriteLine($"f {face.VertexIds[0]}/{v0.VertexOrder}/{face.VertexIds[0]} " +
        //                        $"{face.VertexIds[1]}/{v1.VertexOrder}/{face.VertexIds[1]} " +
        //                        $"{face.VertexIds[2]}/{v2.VertexOrder}/{face.VertexIds[2]}");
        //            }
        //        }


        //    }
        //}

        /// <summary>
        /// Fill transformable mesh with data
        /// </summary>
        public void FillMesh()
        {
            CalculateVertexNormals();

            Mesh.Stride = 8 * sizeof(float); //position (3), texture coords (2), normals (3)

            Mesh.Vertices = new float[8 * MeshTile.VERTICES.Length * Width * Height];

            Dictionary<int, int> textureIndexMap = new Dictionary<int, int>();

            UsedTextureHandles.Clear();

            TextureInfo = new float[2 * Width * Height]; //spritesheet position (1), texture uniform position (1)
            int texInfoIndex = 0;

            int index = 0;
            foreach (var column in MeshTiles)
            {
                foreach (var tile in column)
                {
                    foreach (var vertex in tile.Vertices)
                    {
                        Mesh.Vertices[index++] = vertex.Position[0];
                        Mesh.Vertices[index++] = vertex.Position[1];
                        Mesh.Vertices[index++] = vertex.Position[2] + vertex.Weight;
                        Mesh.Vertices[index++] = MeshTile.VERTICES[(vertex.VertexOrder - 1) * 3]; //X coordinate of base vertex
                        Mesh.Vertices[index++] = MeshTile.VERTICES[(vertex.VertexOrder - 1) * 3 + 1]; //Y coordinate of base vertex
                        Mesh.Vertices[index++] = vertex.Normal[0];
                        Mesh.Vertices[index++] = vertex.Normal[1];
                        Mesh.Vertices[index++] = vertex.Normal[2];
                    }

                    if (tile.TileHandle == null)
                    {
                        TextureInfo[texInfoIndex++] = 0;
                        TextureInfo[texInfoIndex++] = 0;
                    }
                    else
                    {
                        int texHandle = tile.TileHandle.Properties.DisplayInfo.Texture.Texture.Handle;

                        TextureInfo[texInfoIndex++] = tile.TileHandle.Properties.DisplayInfo.SpritesheetPos;

                        if (textureIndexMap.TryGetValue(texHandle, out var texIndex))
                        {
                            TextureInfo[texInfoIndex++] = texIndex;
                        }
                        else
                        {
                            TextureInfo[texInfoIndex++] = UsedTextureHandles.Count;
                            textureIndexMap.Add(texHandle, UsedTextureHandles.Count);
                            UsedTextureHandles.Add(texHandle);
                        }
                    }

                }
            }

            UpdateDrawOrder();
        }

        

        /// <summary>
        /// If only fog/vision changes then only the draw order needs to be updated
        /// </summary>
        public void UpdateDrawOrder()
        {
            //36 faces per tile
            //Mesh.VertexDrawOrder = new uint[FaceList.Count * 3];

            List<uint> fogFaces = new List<uint>();
            List<uint> visibleFaces = new List<uint>();

            //A separate draw order for in vision and fog tiles should be created

            for (int j = 0; j < FaceList.Count; j++)
            {
                var group = FaceList[j];

                if (group.Tile.InFog(VisionManager.Scene.VisibleTeam))
                {
                    for (int i = 0; i < group.Faces.Count; i++)
                    {
                        fogFaces.Add((uint)group.Faces[i].VertexIds[0]);
                        fogFaces.Add((uint)group.Faces[i].VertexIds[1]);
                        fogFaces.Add((uint)group.Faces[i].VertexIds[2]);
                    }
                }
                else
                {
                    for (int i = 0; i < group.Faces.Count; i++)
                    {
                        visibleFaces.Add((uint)group.Faces[i].VertexIds[0]);
                        visibleFaces.Add((uint)group.Faces[i].VertexIds[1]);
                        visibleFaces.Add((uint)group.Faces[i].VertexIds[2]);
                    }
                }
            }

            VisionDrawOrder = visibleFaces.ToArray();
            FogDrawOrder = fogFaces.ToArray();
        }
    }
}
