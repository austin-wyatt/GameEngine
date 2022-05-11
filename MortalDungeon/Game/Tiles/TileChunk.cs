using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Empyrean.Game.Tiles
{
    [Flags]
    public enum TileUpdateType
    {
        Initialize = 0,
        /// <summary>
        /// Recalculates the mesh chunk's draw order based on visible tiles 
        /// and applies that to the render data.
        /// </summary>
        Vision = 1,
        /// <summary>
        /// Applies any changes made to the tile spritesheet data.
        /// </summary>
        Textures = 2,
        /// <summary>
        /// Applies any changes made to the transformation matrix of the chunk mesh.
        /// </summary>
        Transformation = 4,
        /// <summary>
        /// Applies any changes made to vertex position, texture coordinates,
        /// or normal vectors.
        /// </summary>
        Vertex = 8,
    }

    public class TileChunk : ITickable
    {
        public const int DefaultChunkWidth = 10;
        public const int DefaultChunkHeight = 10;

        public Vector3 Center;
        public Vector3 SideLengths;
        public float Radius = 0; //radius in global coords 

        public float LocalRadius = 0; //radius in local coords

        public List<Tile> Tiles = new List<Tile>();
        public HashSet<Structure> Structures = new HashSet<Structure>();
        public List<GameObject> GenericObjects = new List<GameObject>();
        public int Width = DefaultChunkWidth;
        public int Height = DefaultChunkHeight;

        public bool Cull = true;

        public MeshChunk MeshChunk;
        public MeshChunkInstancedRenderData ChunkRenderData;
        public BlendMap BlendMap;

        public TileMap TileMap;
        /// <summary>
        /// The coordinates of the chunk inside of the tile map
        /// </summary>
        public Vector2i ChunkPosition = new Vector2i();

        public TileChunk() { }

        public TileChunk(int width, int height, TileMap map) 
        {
            Width = width;
            Height = height;

            TileMap = map;
        }

        public void AddTile(Tile tile) 
        {
            Tiles.Add(tile);
            tile.Chunk = this;
        }

        public void ClearChunk() 
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                Tiles[i].CleanUp();
            }

            foreach (var structure in Structures)
            {
                structure.CleanUp();
                TileMapManager.Scene.RemoveStructure(structure);
            }

            Tiles.Clear();
            Structures.Clear();
            GenericObjects.Clear();

            BlendMap.CleanUp(BlendMap);
            BlendMap = null;

            ClearRenderData();
        }

        public void ClearRenderData()
        {
            Window.QueueToRenderCycle(() =>
            {
                lock (_meshUpdateLock)
                {
                    ChunkRenderData?.CleanUp();
                    ChunkRenderData = null;
                }
            });
        }

        public void CalculateValues() 
        {
            Vector3 localTilePos = Tiles[0]._position / new Vector3(WindowConstants.ScreenUnits.X, WindowConstants.ScreenUnits.Y, 1);

            Center = (Tiles[0]._position + Tiles[^1]._position) / 2;
            //Center = MeshChunk.Mesh.Position + new Vector3(0.5f + 0.75f * (Width / 2 - 1), TileBounds.MeshTileDimensions.Y * Height / 2, 0);
            Radius = (Tiles[0]._position - Center).Length * 1.5f;

            LocalRadius = (localTilePos - Center).Length;
        }

        public void Tick() 
        {
            //Tiles.ForEach(tile => tile.Tick());
        }

        public void OnCull()
        {
            if (Tiles.Count == 0)
                return;

            //CombatScene scene = Tiles[0].GetScene();

            //Structures.ForEach(structure =>
            //{
            //    if (!Cull)
            //    {
            //        scene.CollateUnit(structure);
            //    }
            //    else 
            //    {
            //        scene.DecollateUnit(structure);
            //    }
            //});
        }

        public void InitializeMesh()
        {
            BlendMap = BlendMap.GetBlendMap(this);

            MeshChunk = new MeshChunk(Tiles, Width, Height);

            PositionMesh();

            lock (_meshUpdateLock)
            {
                MeshChunk.InitializeMesh();
            }
        }

        /// <summary>
        /// Sqrt(3) / 2 * CHUNK_HEIGHT + (0.5 * Sqrt(3) / 2) <para/>
        /// Represents the height of a tile chunk in local opengl coordinates.
        /// </summary>
        private const float MESH_HEIGHT = 9.093267f;
        public void PositionMesh()
        {
            Vector3 pos = WindowConstants.ConvertGlobalToLocalCoordinates(Tiles[0].Position);
            pos.X -= 0.5f;
            pos.Y -= 0.5f;
            pos.Z = 0;

            MeshChunk.Mesh.SetTranslation(pos);

            //pos.Y += 0.5f + MeshTile.TILE_HEIGHT - 0.0075f;
            pos.Y += 0.5f + MeshTile.TILE_HEIGHT;

            MeshChunk.Origin = pos;
            CalculateValues();
        }

        private object _meshUpdateObj = new object();
        public object _meshUpdateLock = new object();
        public void Update(TileUpdateType tileUpdateType)
        {
            if (!TileMap.Visible)
            {
                return;
            }

            if(tileUpdateType == TileUpdateType.Initialize)
            {
                lock (_meshUpdateLock)
                {
                    MeshChunk.UpdateDrawOrder();
                    ChunkRenderData = new MeshChunkInstancedRenderData();
                }

                TileMapManager.Scene.RenderDispatcher.DispatchAction(_meshUpdateObj, () =>
                {
                    lock (_meshUpdateLock)
                    {
                        ChunkRenderData?.GenerateInstancedRenderData(this);
                    }
                });
                return;
            }

            if (MeshChunk == null)
                return;

            if((tileUpdateType & (TileUpdateType.Vision | TileUpdateType.Transformation | TileUpdateType.Textures | TileUpdateType.Vertex)) > 0)
            {
                TileMapManager.Scene.RenderDispatcher.DispatchAction(_meshUpdateObj, () =>
                {
                    lock (_meshUpdateLock)
                    {
                        if (ChunkRenderData == null) return;

                        if((tileUpdateType & TileUpdateType.Vision) > 0)
                        {
                            MeshChunk.UpdateDrawOrder();
                            ChunkRenderData.FillVisionBuffers(this);
                        }

                        if ((tileUpdateType & TileUpdateType.Transformation) > 0)
                        {
                            ChunkRenderData.FillTransformationData(this);
                        }

                        if (((tileUpdateType & TileUpdateType.Vertex) > 0) || ((tileUpdateType & TileUpdateType.Textures) > 0))
                        {
                            ChunkRenderData.FillVertexBuffers(this);
                        }
                    }
                });
            }
        }
    }
}
