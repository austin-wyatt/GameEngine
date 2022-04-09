using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Game.Tiles
{
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
        public MeshChunkInstancedRenderData ChunkInstancedRenderData;

        public TileChunk() { }

        public TileChunk(int width, int height) 
        {
            Width = width;
            Height = height;
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
            ChunkInstancedRenderData.CleanUp();
        }

        public void CalculateValues() 
        {
            Center = (Tiles[0].Position + Tiles[^1].Position) / 2;
            Radius = new Vector3(Tiles[0].Position - Center).Length * 1.5f;

            SideLengths = WindowConstants.ConvertGlobalToLocalCoordinates(new Vector3(Tiles[0].Position - Center));
            LocalRadius = WindowConstants.ConvertGlobalToLocalCoordinates(new Vector3(Tiles[0].Position - Center)).Length;
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

        public void OnFilled()
        {
            //TODO, move the mesh chunk so that it is in the correct position
            MeshChunk = new MeshChunk(Tiles, Width, Height);
        }

        public object _meshUpdateLock = new object();
        public void UpdateTile()
        {
            //Just refill the entire mesh for now. Once the base implementation is working we can look into 
            //more targeted updates for the mesh
            lock (_meshUpdateLock)
            {
                MeshChunk.FillMesh();
            }
            
            Window.QueueToRenderCycle(() =>
            {
                lock (_meshUpdateLock)
                {
                    ChunkInstancedRenderData.GenerateInstancedRenderData(this);
                }
            });
        }

    }
}
