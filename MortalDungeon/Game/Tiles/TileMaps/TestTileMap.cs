using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Tiles.TileMaps
{
    class TestTileMap : TileMap
    {
        public TestTileMap(Vector3 position, TileMapPoint point, TileMapController controller) : base(position, point, controller, "TestTileMap")
        {

        }

        public override void PopulateTileMap(float zTilePlacement = 0)
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(Position);

            tilePosition.Z += zTilePlacement;

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    baseTile = new BaseTile(tilePosition, new TilePoint(i, o, this)) { Clickable = true };
                    baseTile.SetAnimation(BaseTileAnimationType.Grass);
                    baseTile.DefaultAnimation = BaseTileAnimationType.Grass;
                    baseTile.Properties.Type = TileType.Grass;
                    baseTile.TileMap = this;
                    baseTile.Outline = true;

                    Tiles.Add(baseTile);

                    if (_randomNumberGen.NextDouble() > 0.9) 
                    {
                        baseTile.Properties.Type = TileType.Grass_2;
                    }

                    //if (_randomNumberGen.NextDouble() < 0.2d && baseTile.TilePoint.X != 0 && baseTile.TilePoint.Y != 0) //add a bit of randomness to tile gen
                    //{
                    //    //baseTile.Properties.Classification = TileClassification.Terrain;
                    //    //baseTile.DefaultAnimation = BaseTileAnimationType.SolidWhite;
                    //    //baseTile.DefaultColor = new Vector4(0.25f, 0.25f, 0.25f, 1);

                    //    //baseTile.Properties.Type = TileType.Default;
                    //    CreateTree(baseTile);
                    //}

                    tilePosition.Y += baseTile.BaseObjects[0].Dimensions.Y;
                }
                tilePosition.X = (i + 1) * baseTile.BaseObjects[0].Dimensions.X / 1.34f; //1.29 before outlining changes
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : baseTile.BaseObjects[0].Dimensions.Y / -2f); //2 before outlining changes
            }

            LoadTextures(Tiles);

            tilePosition.Z += 0.03f;

            SetDefaultTileValues();
            //InitializeTexturedQuad();
        }


        public override void OnAddedToController()
        {
            base.OnAddedToController();
        }

        public override void PopulateFeatures()
        {
            base.PopulateFeatures();

            foreach (BaseTile baseTile in Tiles)
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
