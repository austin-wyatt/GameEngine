using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using static MortalDungeon.Game.Tiles.TileMap;

namespace MortalDungeon.Game.Structures
{
    public enum WallType
    {
        Wall,
        Corner,
        Door
    }

    public class Wall : Structure
    {
        public WallType WallType;

        public bool Locked = false;
        public bool Openable = true;
        public bool Opened = false;

        public Wall(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position, WallType type) : base(scene, spritesheet, spritesheetPos, position)
        {
            WallType = type;
        }

        public void CreateDoor(BaseTile tile)
        {
            Vector3 rotation = tile.Structure.BaseObject.BaseFrame.GetRotationRadians();
            Vector3 tileDim = tile.GetDimensions();

            Vector3 topPos = tile.Structure.Position - new Vector3(tileDim.Y * (float)Math.Sin(rotation.Z) / 2, tileDim.Y * (float)Math.Cos(rotation.Z) / 2, 0);
            Vector3 botPos = tile.Structure.Position + new Vector3(tileDim.Y * (float)Math.Sin(rotation.Z) / 2, tileDim.Y * (float)Math.Cos(rotation.Z) / 2, 0);


            Name = "Door";
            Type = StructureEnum.Wall_Wood_Door;
            WallType = WallType.Door;

            HasContextMenu = true;

            BaseObject.BaseFrame.SpritesheetPosition = (int)StructureEnum.Wall_Wood_Door;
            SetColor(Colors.Transparent);

            BaseObject door_1 = tile.CreateBaseObjectFromSpritesheet(Spritesheets.StructureSheet, (int)StructureEnum.Wall_Wood_Door);
            door_1.SetPosition(topPos + 0.001f * Vector3.UnitZ);
            door_1.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(rotation.Z) + 180);
            BaseObjects.Add(door_1);

            BaseObject door_2 = tile.CreateBaseObjectFromSpritesheet(Spritesheets.StructureSheet, (int)StructureEnum.Wall_Wood_Door);
            door_2.BaseFrame.RotateZ((MathHelper.RadiansToDegrees(rotation.Z)));
            door_2.SetPosition(botPos + 0.001f * Vector3.UnitZ);
            BaseObjects.Add(door_2);

            Console.WriteLine(MathHelper.RadiansToDegrees(rotation.Z));
        }

        public void OpenDoor()
        {
            if (!Opened && Openable)
            {
                BaseObjects[1].BaseFrame.RotateZ(60);
                BaseObjects[2].BaseFrame.RotateZ(-60);
                Opened = true;
                Passable = true;
            }
        }

        public void CloseDoor()
        {
            if (Opened && Openable)
            {
                BaseObjects[1].BaseFrame.RotateZ(-60);
                BaseObjects[2].BaseFrame.RotateZ(60);
                Opened = false;
                Passable = false;
            }
        }

        public void ToggleDoor()
        {
            if (!Opened)
            {
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }


        public enum Walls
        {
            Wood,
            Stone,
            Iron
        }
        public static void CreateWalls(TileMap map, TilePoint startPoint, TilePoint endPoint, Walls walls)
        {
            PathToPointParameters param = new PathToPointParameters(startPoint, endPoint, 100)
            {
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground, TileClassification.Terrain, TileClassification.Water }
            };

            List<BaseTile> tiles = map.GetPathToPoint(param);
            Direction direction = Direction.None;
            Direction nextDirection = Direction.None;

            WallType type;

            for (int i = 0; i < tiles.Count; i++)
            {
                StructureEnum wallType;

                switch (walls)
                {
                    case Walls.Iron:
                        wallType = StructureEnum.Wall_Iron_1;
                        break;
                    case Walls.Stone:
                        wallType = StructureEnum.Wall_1;
                        break;
                    default:
                        wallType = StructureEnum.Wall_Wood_1;
                        break;
                }
                type = WallType.Wall;



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
                    switch (walls)
                    {
                        case Walls.Iron:
                            wallType = StructureEnum.Wall_Iron_1;
                            type = WallType.Corner;
                            break;
                        case Walls.Stone:
                            wallType = StructureEnum.Wall_Corner;
                            type = WallType.Corner;
                            break;
                        default:
                            wallType = StructureEnum.Wall_Wood_Corner;
                            type = WallType.Corner;
                            break;
                    }

                    Direction prevDirection = FeatureEquation.DirectionBetweenTiles(tiles[i].TilePoint, tiles[i - 1].TilePoint);

                    //Console.WriteLine(AngledWallDirectionsToRotation(prevDirection, nextDirection));
                    rotation = AngledWallDirectionsToRotation(prevDirection, nextDirection);
                }

                bool pathable = true;
                bool transparent = false;

                switch (walls)
                {
                    case Walls.Iron:
                        transparent = true;
                        break;
                }

                CreateWall(map, tiles[i], wallType, rotation, type, pathable, transparent, walls);

                direction = nextDirection;
            }
        }

        public static void CreateWall(TileMap map, BaseTile tile, StructureEnum wallType, int rotation, WallType type, bool pathable = true, bool transparent = false, Walls walls = Walls.Wood)
        {
            Wall wall = new Wall(map.Controller.Scene, Spritesheets.StructureSheet, (int)wallType, tile.Position + new Vector3(0, 0, 0.01f), type);


            //if (type == WallType.Corner && walls != Walls.Iron)
            //{
            //    wall.SetScale(2);
            //}

            if (type == WallType.Corner)
            {
                wall.BaseObjects.Clear();
                wall.BaseObjects.Add(_3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Wall_Iron_1, Spritesheets.StructureSheet), _3DObjects.WallCornerObj, tile.Position + new Vector3(0, 0, 0.3f)));

                wall.BaseObject.BaseFrame.RotateX(90);
            }
            else
            {
                wall.BaseObjects.Clear();
                wall.BaseObjects.Add(_3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Wall_Iron_1, Spritesheets.StructureSheet), _3DObjects.WallObj, tile.Position + new Vector3(0, 0, 0.3f)));

                wall.BaseObject.BaseFrame.RotateX(90);
            }

            wall.SetColor(new Vector4(0.329f, 0.333f, 0.349f, 1));

            wall.SetScale(0.5f);
            
            wall.BaseObject.BaseFrame.RotateZ(rotation - 90);

            wall.VisibleThroughFog = true;
            wall.SetTileMapPosition(tile);
            wall.Name = "Wall";
            wall.Pathable = true;
            wall.Info.Transparent = transparent;

            wall.SetTeam(UnitTeam.Neutral);
            wall.Info.Height = 4;

            tile.Chunk.Structures.Add(wall);
            tile.Structure = wall;


            //if (walls == Walls.Iron)
            //{
            //    Random rand = new Random();

            //    wall.BaseObject.BaseFrame.RotateZ(-rotation);
            //    wall.BaseObject.BaseFrame.RotateY(110);
            //    wall.SetPosition(wall.Position + new Vector3(0, -100, 0.30f));

            //    if (type == WallType.Corner)
            //    {
            //        wall.BaseObject.BaseFrame.RotateZ(60);
            //    }

            //    //Console.WriteLine($"Rotation: {rot}");
            //}
        }

        //theres definitely a formula for this but this works just as well and is readable so whatev
        public static int AngledWallDirectionsToRotation(Direction inlet, Direction outlet)
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


        public override Tooltip CreateContextMenu()
        {
            if (WallType != WallType.Door)
                return null;

            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(Type.Name());

            list.AddItem(Opened ? "Close" : "Open", (item) =>
            {
                ToggleDoor();
                item._textBox.SetText(Opened ? "Close" : "Open");
            });

            list.AddItem(Openable ? "Lock" : "Unlock", (item) =>
            {
                Openable = !Openable;
                item._textBox.SetText(Openable ? "Lock" : "Unlock");

                checkOpenOptionDisabled();
            });

            void checkOpenOptionDisabled()
            {
                if (!Openable)
                {
                    list.Items[0].SetDisabled(true);
                }
                else
                {
                    list.Items[0].SetDisabled(false);
                }
            }

            checkOpenOptionDisabled();

            return menu;
        }
    }
}
