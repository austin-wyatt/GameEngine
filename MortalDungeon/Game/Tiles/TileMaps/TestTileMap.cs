using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Game.Map;
using Empyrean.Game.Objects;
using Empyrean.Game.Structures;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Tiles.TileMaps
{
    class TestTileMap : TileMap
    {
        private int[] _grassTiles = new int[] {166, 167, 168, 169, 186, 187, 188, 189, 206, 207, 208, 209, 226, 227, 228, 229 };

        public TestTileMap(Vector3 position, TileMapPoint point, TileMapController controller) : base(position, point, controller, "TestTileMap")
        {

        }

        public override void PopulateTileMap(float zTilePlacement = 0)
        {
            Tile tile = new Tile();
            Vector3 tilePosition = new Vector3(Position);

            List<AsyncSignal> tileTextureSignals = new List<AsyncSignal>();

            Vector3 tileDim = new Vector3(TileBounds.TileDimensions);

            tilePosition.Z += zTilePlacement;

            

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    //Vector3 zFuzz = new Vector3(0, 0, (float)_randomNumberGen.NextDouble() / 50);
                    //baseTile = new BaseTile(tilePosition + zFuzz, new TilePoint(i, o, this)) { Clickable = true };
                    tile = new Tile(tilePosition, new TilePoint(i, o, this));
                    tile.Properties.SetType(TileType.Grass, false, updateChunk: false);
                    //tile.Properties.SetType(TileType.Stone_1, false, updateChunk: false);
                    
                    tile.TileMap = this;

                    Tiles.Add(tile);

                    //checkerboard pattern
                    //if((i % 2 == 0) && (o % 2 == 0))
                    //{
                    //    tile.Properties.SetType(TileType.Stone_1, false, updateChunk: false);
                    //}

                    //tile.Properties.SetType(TileType.Stone_1, false, updateChunk: false);

                    //if (_randomNumberGen.NextDouble() > 0.9)
                    //{
                    //    baseTile.Properties.Type = TileType.Grass_2;
                    //}

                    //baseTile.Properties.Type = (TileType)_grassTiles.GetRandom();

                    //baseTile.BaseObject.BaseFrame.SpritesheetPosition = (int)TileType.Fill;
                    //float val = GlobalRandom.NextFloat() / 30f;
                    //baseTile.SetColor(_Colors.GrassGreen - new Vector4(val, val, 0, 0));

                    float val = GlobalRandom.NextFloat() / 15f;
                    tile.SetColor(_Colors.White - new Vector4(val, val, val, 0));

                    tilePosition.Y += tileDim.Y;

                    //if (WindowConstants.InMainThread(Thread.CurrentThread))
                    //{
                    //    Renderer.LoadTextureFromSimple(tile.Properties.DisplayInfo.Texture);
                    //}
                    //else
                    //{
                    //    AsyncSignal textureSignal = new AsyncSignal();
                    //    tileTextureSignals.Add(textureSignal);

                    //    TextureLoadBatcher.LoadTexture(tile.Properties.DisplayInfo.Texture, textureSignal);
                    //}
                }
                tilePosition.X = (i + 1) * tileDim.X * 0.75f;
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : tileDim.Y * -0.5f); 
            }

            tilePosition.Z += 0.03f;

            //for(int i = 0; i < tileTextureSignals.Count; i++)
            //{
            //    tileTextureSignals[i].Wait();
            //    tileTextureSignals[i].Dispose();
            //}
        }


        public override void PopulateFeatures()
        {
            base.PopulateFeatures();

            foreach (Tile baseTile in Tiles)
            {
                if (_randomNumberGen.NextDouble() < 0.2d && baseTile.TilePoint.X != 0 && baseTile.TilePoint.Y != 0 && baseTile.Properties.Classification != TileClassification.Water) //add a bit of randomness to tile gen
                {
                    if (_randomNumberGen.NextDouble() > 0.3)
                    {
                        if (baseTile.Structure == null) 
                        {
                            switch (baseTile.Properties.Type)
                            {
                                case TileType.Stone_1:
                                case TileType.Stone_2:
                                case TileType.Stone_3:
                                case TileType.Gravel:
                                    continue;
                            }
                            Tree tree = new Tree(this, baseTile);
                            baseTile.AddStructure(tree);
                        }
                    }
                    else 
                    {
                        if (baseTile.Structure == null)
                        {
                            Rock rock = new Rock(this, baseTile);
                            baseTile.AddStructure(rock);
                        }
                    }
                }
            }
        }
    }
}
