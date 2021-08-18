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

        public void CreateWalls(TilePoint startPoint, TilePoint endPoint)
        {
            PathToPointParameters param = new PathToPointParameters(startPoint, endPoint, 100) 
            {
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground, TileClassification.Terrain, TileClassification.Water }
            };

            List<BaseTile> tiles = GetPathToPoint(param);
            Direction direction = Direction.None;
            Direction nextDirection = Direction.None;

            for (int i = 0; i < tiles.Count; i++)
            {
                StructureEnum wallType = StructureEnum.Wall_1;

                if (direction == Direction.None) 
                {
                    direction = Direction.North;
                }

                int rotation = 0;

                if (i < tiles.Count - 1)
                {
                    nextDirection = FeatureEquation.DirectionBetweenTiles(tiles[i].TilePoint, tiles[i + 1].TilePoint);
                }

                rotation = FeatureEquation.AngleOfDirection(nextDirection);

                if (i != 0 && nextDirection != direction) 
                {
                    wallType = StructureEnum.Wall_Corner;

                    Direction prevDirection = FeatureEquation.DirectionBetweenTiles(tiles[i].TilePoint, tiles[i - 1].TilePoint);

                    Console.WriteLine(AngledWallDirectionsToRotation(prevDirection, nextDirection));
                    rotation = AngledWallDirectionsToRotation(prevDirection, nextDirection);
                }


                CreateWall(tiles[i], wallType, rotation);

                direction = nextDirection;
            }
        }

        public void CreateWall(BaseTile tile, StructureEnum wallType, int rotation) 
        {
            Structure wall = new Structure(Controller.Scene, Spritesheets.StructureSheet, (int)wallType, tile.Position + new Vector3(0, 0, 0.01f));

            if (wallType == StructureEnum.Wall_Corner)
            {
                wall.SetScale(2);
            }

            wall.BaseObject.BaseFrame.RotateZ(rotation);

            wall.VisibleThroughFog = true;
            wall.TileMapPosition = tile;
            wall.Name = "Wall";
            wall.Pathable = true;

            //if (color)
            //    wall.SetColor(Colors.Red);

            wall.SetTeam(UnitTeam.Neutral);
            wall.Height = 4;

            tile.Chunk.Structures.Add(wall);
            tile.Structure = wall;
        }

        //theres definitely a formula for this but this works just as well and is readable so whatev
        public int AngledWallDirectionsToRotation(Direction inlet, Direction outlet) 
        {
            return inlet switch
            {
                Direction.NorthWest => outlet == Direction.NorthEast ? 60 : 180,
                Direction.SouthWest => outlet == Direction.North ? 120 : 240,
                Direction.South => outlet == Direction.NorthWest ? 180 : 300,
                Direction.SouthEast => outlet == Direction.SouthWest ? 240 : 0,
                Direction.NorthEast => outlet == Direction.South ? 300 : 60,
                _ => outlet == Direction.SouthEast ? 0 : 120,
            };
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
