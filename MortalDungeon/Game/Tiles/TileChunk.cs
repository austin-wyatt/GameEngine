using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Structures;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Game.Tiles
{
    internal class TileChunk : ITickable
    {
        internal const int DefaultChunkWidth = 10;
        internal const int DefaultChunkHeight = 10;

        internal Vector3 Center;
        internal Vector3 SideLengths;
        internal float Radius = 0; //radius in global coords 

        internal float LocalRadius = 0; //radius in local coords

        internal List<BaseTile> Tiles = new List<BaseTile>();
        internal List<Structure> Structures = new List<Structure>();
        internal List<GameObject> GenericObjects = new List<GameObject>();
        internal int Width = DefaultChunkWidth;
        internal int Height = DefaultChunkHeight;

        internal bool Cull = true;

        internal TileChunk() { }

        internal TileChunk(int width, int height) 
        {
            Width = width;
            Height = height;
        }

        internal void AddTile(BaseTile tile) 
        {
            Tiles.Add(tile);
            tile.Chunk = this;
        }

        internal void ClearChunk() 
        {
            Tiles.ForEach(tile =>
            {
                tile.CleanUp();
            });

            Tiles.Clear();
            Structures.Clear();
            GenericObjects.Clear();
        }

        internal void CalculateValues() 
        {
            Center = (Tiles[0].Position + Tiles[^1].Position) / 2;
            Radius = new Vector3(Tiles[0].Position - Center).Length;

            SideLengths = WindowConstants.ConvertGlobalToLocalCoordinates(new Vector3(Tiles[0].Position - Center));
            LocalRadius = WindowConstants.ConvertGlobalToLocalCoordinates(new Vector3(Tiles[0].Position - Center)).Length;
        }

        internal void Tick() 
        {
            //Tiles.ForEach(tile => tile.Tick());
            Structures.ForEach(structure => structure.Tick());
        }

        internal void OnCull()
        {
            if (Tiles.Count == 0)
                return;

            CombatScene scene = Tiles[0].GetScene();

            Structures.ForEach(structure =>
            {
                if (!Cull)
                {
                    scene.CollateUnit(structure);
                }
                else 
                {
                    scene.DecollateUnit(structure);
                }
            });
        }
    }
}
