using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public class TileChunk
    {
        public const int DefaultChunkWidth = 10;
        public const int DefaultChunkHeight = 10;

        public Vector3 Center;
        public float Radius = 0; //radius in global coords 

        public float LocalRadius = 0; //radius in local coords

        public List<BaseTile> Tiles = new List<BaseTile>();
        public int Width = DefaultChunkWidth;
        public int Height = DefaultChunkHeight;

        public bool Cull = false;

        public TileChunk() { }

        public TileChunk(int width, int height) 
        {
            Width = width;
            Height = height;
        }

        public void AddTile(BaseTile tile) 
        {
            Tiles.Add(tile);
        }

        public void CalculateValues() 
        {
            Center = (Tiles[0].Position + Tiles[Tiles.Count - 1].Position) / 2;
            Radius = new Vector3(Tiles[0].Position - Center).Length;

            LocalRadius = WindowConstants.ConvertGlobalToLocalCoordinates(new Vector3(Tiles[0].Position - Center)).Length;
        }
    }
}
