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

        private float ZRotation = 0;
        private const float WALL_HEIGHT = 0.6f;

        public Wall(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position, WallType type) : base(scene, spritesheet, spritesheetPos, position)
        {
            WallType = type;
        }

        public void CreateDoor(BaseTile tile)
        {
            Vector3 tileDim = tile.GetDimensions();

            float rot = MathHelper.DegreesToRadians(ZRotation);

            Vector3 topPos = tile.Structure.Position - new Vector3(tileDim.Y * (float)Math.Sin(rot) / 2, tileDim.Y * (float)Math.Cos(rot) / 2, 0);
            Vector3 botPos = tile.Structure.Position + new Vector3(tileDim.Y * (float)Math.Sin(rot) / 2, tileDim.Y * (float)Math.Cos(rot) / 2, 0);

            Name = "Door";
            //Type = StructureEnum.Wall_Wood_Door;
            Type = StructureEnum.Wall_Iron_1;
            WallType = WallType.Door;

            HasContextMenu = true;

            BaseObject.BaseFrame.SpritesheetPosition = (int)Type;
            SetColor(Colors.Transparent);

            RemoveBaseObject(BaseObject);

            BaseObject door_1 = tile.CreateBaseObjectFromSpritesheet(Spritesheets.StructureSheet, (int)StructureEnum.Wall_Iron_Door);
            door_1.SetPosition(topPos);
            door_1.BaseFrame.RotateX(90);
            door_1.BaseFrame.RotateZ(ZRotation - 90);
            door_1.BaseFrame.VerticeType = 0;

            AddBaseObject(door_1);

            BaseObject door_2 = tile.CreateBaseObjectFromSpritesheet(Spritesheets.StructureSheet, (int)StructureEnum.Wall_Iron_Door);
            door_2.SetPosition(botPos);
            door_2.BaseFrame.RotateX(90);
            door_2.BaseFrame.RotateZ(ZRotation + 90);
            door_2.BaseFrame.VerticeType = 0;
            AddBaseObject(door_2);

            door_2.BaseFrame.SetBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1));
            door_1.BaseFrame.SetBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1));

            door_1.BaseFrame.ScaleAll(0.5f);
            door_2.BaseFrame.ScaleAll(0.5f);

            LoadTexture(this);
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


        public enum WallMaterial
        {
            Wood,
            Stone,
            Iron
        }
        public static void CreateWalls(TileMap map, List<BaseTile> tiles, WallMaterial walls)
        {
            Direction direction = Direction.None;
            Direction nextDirection = Direction.None;

            WallType type;

            for (int i = 0; i < tiles.Count; i++)
            {
                StructureEnum wallType;

                switch (walls)
                {
                    case WallMaterial.Iron:
                        wallType = StructureEnum.Wall_Iron_1;
                        break;
                    case WallMaterial.Stone:
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
                        case WallMaterial.Iron:
                            wallType = StructureEnum.Wall_Iron_1;
                            type = WallType.Corner;
                            break;
                        case WallMaterial.Stone:
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
                    case WallMaterial.Iron:
                        transparent = true;
                        break;
                }

                CreateWall(map, tiles[i], wallType, rotation, type, pathable, transparent, walls);

                direction = nextDirection;
            }
        }

        public static void CreateWall(TileMap map, BaseTile tile, StructureEnum wallType, int rotation, WallType type, bool pathable = true, bool transparent = false, WallMaterial wallMaterial = WallMaterial.Wood)
        {
            Wall wall = new Wall(map.Controller.Scene, Spritesheets.StructureSheet, (int)wallType, tile.Position + new Vector3(0, 0, WALL_HEIGHT), type);


            //if (type == WallType.Corner && walls != Walls.Iron)
            //{
            //    wall.SetScale(2);
            //}

            if (type == WallType.Corner)
            {
                wall.BaseObjects.Clear();
                //wall.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Wall_Iron_1, Spritesheets.StructureSheet), _3DObjects.WallCornerObj, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));
                wall.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.TestSheet), _3DObjects.WallCorner3D, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));

                wall.BaseObject.BaseFrame.RotateX(90);
            }
            else
            {
                wall.BaseObjects.Clear();
                //wall.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Wall_Iron_1, Spritesheets.StructureSheet), _3DObjects.WallObj, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));
                wall.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.TestSheet), _3DObjects.Wall3D, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));

                wall.BaseObject.BaseFrame.RotateX(90);
            }

            wall.SetColor(new Vector4(0.5f, 0.5f, 0.5f, 1));

            wall.SetScale(0.5f);
            
            wall.BaseObject.BaseFrame.RotateZ(rotation - 90);

            wall.VisibleThroughFog = true;
            wall.SetTileMapPosition(tile);
            wall.Name = "Wall";
            wall.Pathable = true;
            wall.Info.Transparent = transparent;

            wall.SetTeam(UnitTeam.Unknown);
            wall.Info.Height = 4;

            LoadTexture(wall);

            tile.AddStructure(wall);

            wall.ZRotation = rotation;

            if (wallMaterial == WallMaterial.Iron) 
            {
                wall.LightObstruction.ObstructionType = Lighting.LightObstructionType.None;
            }
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
        /// <summary>
        /// Given a wall, find all adjacent walls of the same material
        /// </summary>
        /// <param name="wall">The wall to start with</param>
        public static (List<Wall> walls, bool circular) FindAdjacentWalls(Wall wall) 
        {
            TileMap map = wall.Info.Map;

            List<Wall> wallList = new List<Wall>();
            bool circular = false;

            Wall prevWall = null;
            Wall currWall = wall;

            Wall secondaryDirection = null;
            bool visitedSecondaryDirection = false;
            while (true) 
            {
                List<BaseTile> tiles = new List<BaseTile>();
                map.GetNeighboringTiles(currWall.Info.TileMapPosition, tiles, false);

                tiles.ForEach(t => t.TilePoint._visited = false);

                if (visitedSecondaryDirection) 
                {
                    wallList.Insert(0, currWall);
                }
                else 
                {
                    wallList.Add(currWall);
                }

                bool wallFound = false;
                for (int i = 0; i < tiles.Count; i++) 
                {
                    if (tiles[i].Structure is Wall tempWall)
                    {
                        if (!wallList.Contains(tempWall)) 
                        {
                            if (!wallFound)
                            {
                                currWall = tempWall;

                                wallFound = true;
                            }
                            else if(secondaryDirection == null)
                            {
                                secondaryDirection = tempWall;
                            }
                        }
                    }
                }

                if (wallFound == false)
                {
                    if (!visitedSecondaryDirection && secondaryDirection != null)
                    {
                        prevWall = currWall;
                        currWall = secondaryDirection;
                        visitedSecondaryDirection = true;
                    }
                    else 
                    {
                        break;
                    }
                }
                else if (currWall == secondaryDirection) 
                {
                    wallList.Add(currWall);
                    circular = true;
                    break;
                }

                prevWall = currWall;
            }

            return (wallList, circular);
        }

        public static void UnifyWalls(List<Wall> wallList, bool circular) 
        {
            Direction direction = Direction.None;
            Direction nextDirection = Direction.None;

            for (int i = 0; i < wallList.Count; i++)
            {
                WallType wallType;

                wallType = WallType.Wall;


                if (direction == Direction.None)
                {
                    direction = Direction.North;
                }

                int rotation = 0;

                if (i < wallList.Count - 1)
                {
                    nextDirection = FeatureEquation.DirectionBetweenTiles(wallList[i].Info.Point, wallList[i + 1].Info.Point);
                }
                else if (circular) 
                {
                    nextDirection = FeatureEquation.DirectionBetweenTiles(wallList[i].Info.Point, wallList[0].Info.Point);
                }

                rotation = FeatureEquation.AngleOfDirection(nextDirection);

                if (i != 0 && nextDirection != direction)
                {
                    wallType = WallType.Corner;

                    Direction prevDirection = FeatureEquation.DirectionBetweenTiles(wallList[i].Info.Point, wallList[i - 1].Info.Point);

                    rotation = AngledWallDirectionsToRotation(prevDirection, nextDirection);
                }

                direction = nextDirection;

                if (wallList[i].WallType == WallType.Door)
                    continue;

                wallList[i].WallType = wallType;

                BaseTile tile = wallList[i].Info.TileMapPosition;
                Vector4 wallColor = wallList[i].BaseObject.BaseFrame.BaseColor;

                if (wallList[i].WallType == WallType.Corner)
                {
                    wallList[i].BaseObjects.Clear();
                    //wallList[i].AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject((int)wallList[i].Type, Spritesheets.StructureSheet), _3DObjects.WallCornerObj, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));
                    wallList[i].AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.TestSheet), _3DObjects.WallCorner3D, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));

                    wallList[i].BaseObject.BaseFrame.RotateX(90);
                }
                else
                {
                    wallList[i].BaseObjects.Clear();
                    //wallList[i].AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Wall_Iron_1, Spritesheets.StructureSheet), _3DObjects.WallObj, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));
                    wallList[i].AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.TestSheet), _3DObjects.Wall3D, tile.Position + new Vector3(0, 0, WALL_HEIGHT)));

                    wallList[i].BaseObject.BaseFrame.RotateX(90);
                }

                wallList[i].SetColor(wallColor);

                wallList[i].SetScale(0.5f);

                wallList[i].BaseObject.BaseFrame.RotateZ(rotation - 90);

                wallList[i].ZRotation = rotation;
            }
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
