using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
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

                    LoadTexture(baseTile);

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

            tilePosition.Z += 0.03f;
            InitializeHelperTiles(tilePosition);

            SetDefaultTileValues();
            InitializeTexturedQuad();
        }

        public void CreateTree(BaseTile tile) 
        {
            if (tile.Structure != null)
                return;

            switch (tile.Properties.Type) 
            {
                case TileType.Stone_1:
                case TileType.Stone_2:
                case TileType.Stone_3:
                case TileType.Gravel:
                    return;
            }

            Structure tree = new Structure(Controller.Scene, Spritesheets.StructureSheet, _randomNumberGen.Next() % 2 + 2, tile.Position + new Vector3(0, -200, 0.22f));
            tree.BaseObject.BaseFrame.RotateX(25);
            tree.BaseObject.BaseFrame.SetScaleAll(1 + (float)_randomNumberGen.NextDouble() / 2);

            tree.VisibleThroughFog = true;
            tree.TileMapPosition = tile;
            tree.Name = "Tree";
            tree.Pathable = false;

            tree.SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

            tree.SetTeam(UnitTeam.Neutral);
            tree.Height = 2;

            tile.Chunk.Structures.Add(tree);
            tile.Structure = tree;
            //tile.Chunk.GenericObjects.Add(tree.SelectionTile);
        }

        public void CreateRock(BaseTile tile)
        {
            if (tile.Structure != null)
                return;

            Structure rock = new Structure(Controller.Scene, Spritesheets.StructureSheet, _randomNumberGen.Next() % 3 + (int)StructureEnum.Rock_1, tile.Position + new Vector3(0, -200, 0.12f));
            rock.BaseObject.BaseFrame.RotateX(15);
            rock.BaseObject.BaseFrame.SetScaleAll(1 + (float)_randomNumberGen.NextDouble() / 2);
            
            rock.VisibleThroughFog = true;
            rock.TileMapPosition = tile;
            rock.Name = "Rock";
            tile.Properties.Classification = TileClassification.Terrain;

            rock.SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

            rock.SetTeam(UnitTeam.Neutral);
            if (rock.Type == StructureEnum.Rock_2)
            {
                rock.Height = 1;
            }
            else 
            {
                rock.Height = 2;
            }

            tile.Chunk.Structures.Add(rock);
            tile.Structure = rock;
            //tile.Chunk.GenericObjects.Add(tree.SelectionTile);
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
                        CreateTree(baseTile);
                    }
                    else 
                    {
                        CreateRock(baseTile);
                    }
                }
            }
        }
    }
}
