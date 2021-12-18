using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    internal class Graveyard_1 : FeatureEquation
    {
        private enum GraveyardFeatures 
        {
            DeadGrass = 1,
            Fence = 2,
            Gate = 4,
            DeadTree = 8,
            MustExplore = 16,
            Grave = 32,
            Enemy = 64
        }

        private GraveyardParams GraveyardParams;
        private Random NumberGen;

        private List<FeaturePoint> WallPoints = new List<FeaturePoint>();

        private List<BaseTile> WallTiles = new List<BaseTile>();
        private List<BaseTile> GateTiles = new List<BaseTile>();

        internal Graveyard_1(GraveyardParams @params)
        {
            GraveyardParams = @params;
            NumberGen = new Random(HashCoordinates(@params.Origin.X, @params.Origin.Y));
        }

        internal override void ApplyToTile(BaseTile tile, bool freshGeneration = true)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if (!freshGeneration)
                return;

            if (AffectedPoints.TryGetValue(affectedPoint, out int value))
            {
                if (GetBit(value, 0)) 
                {
                    tile.Properties.Type = TileType.Dead_Grass;
                    tile.Update();
                }

                if (GetBit(value, 1))
                {
                    WallTiles.Add(tile);
                }
                if (GetBit(value, 2))
                {
                    GateTiles.Add(tile);
                }
                if (GetBit(value, 3))
                {
                    if (tile.Properties.Type.SimplifiedType() == SimplifiedTileType.Grass) 
                    {
                        Tree tree =new Tree(tile.TileMap, tile, 0);

                        tree.BaseObject.BaseFrame.AddAppliedColor(new Color(0.3f, 0.3f, 0.3f, 1));
                    }
                }
                if(GetBit(value, 4))
                {
                    tile.Properties.MustExplore = true;
                    tile.Explored[Units.UnitTeam.PlayerUnits] = false;
                }
                if (GetBit(value, 5)) 
                {
                    if (tile.Structure == null) 
                    {
                        Grave grave = new Grave(tile.TileMap, tile, (int)StructureEnum.Grave_1 + NumberGen.Next() % 3);
                    }
                }
                if (GetBit(value, 6))
                {
                    if (tile.Structure == null)
                    {
                        Entity skele = new Entity(EntityParser.ApplyPrefabToUnit(EntityParser.FindPrefab(PrefabType.Unit, "Grave Skele"), tile.GetScene()));
                        EntityManager.AddEntity(skele);

                        skele.Handle.pack_name = "graveyard" + FeatureID;

                        skele.DestroyOnUnload = true;

                        skele.Load(affectedPoint);

                        //Console.WriteLine("Skeleton spawned on point " + affectedPoint.ToString());
                    }
                }
            }
        }

        internal override void GenerateFeature()
        {
            ClearAffectedPoints();

            FeaturePoint startPoint = GraveyardParams.Origin;

            List<FeaturePoint> path = new List<FeaturePoint>();

            for (int i = 0; i < GraveyardParams.Radius; i++)
            {
                path.Clear();
                GetRingOfTiles(startPoint, path, i);

                float grassChance = 1;

                int falloffAmount = GraveyardParams.Radius - i;
                if (falloffAmount <= GraveyardParams.GrassFalloffRadius) 
                {
                    grassChance = (float)falloffAmount / GraveyardParams.GrassFalloffRadius;
                }

                if (i == 0) 
                {
                    path.Add(startPoint);
                }

                for (int j = 0; j < path.Count; j++)
                {
                    AffectedPoints.TryGetValue(path[j], out int val);

                    if (NumberGen.NextDouble() < grassChance) 
                    {
                        val |= (int)GraveyardFeatures.DeadGrass;
                    }

                    if (NumberGen.NextDouble() < GraveyardParams.TreeDensity) 
                    {
                        if (NumberGen.NextDouble() < 0.1) 
                        {
                            val |= (int)GraveyardFeatures.Enemy;
                        }
                        else 
                        {
                            val |= (int)GraveyardFeatures.DeadTree;
                        }
                    }

                    if (i == GraveyardParams.GraveyardRadius) 
                    {
                        val |= (int)GraveyardFeatures.Fence;

                        WallPoints.Add(path[j]);
                    }

                    if (i <= GraveyardParams.GraveyardRadius + 2) 
                    {
                        val |= (int)GraveyardFeatures.MustExplore;
                    }

                    if (i < GraveyardParams.GraveyardRadius - 2)
                    {
                        if (path[j].X % 3 == 0 && path[j].Y % 2 == 0) 
                        {
                            val |= (int)GraveyardFeatures.Grave;
                        }
                    }

                    AffectedPoints[path[j]] = val;
                    AffectedMaps.Add(FeaturePointToTileMapCoords(path[j]));
                }
            }

            
            for (int i = 0; i < GraveyardParams.Doors; i++)
            {
                int door = NumberGen.Next() % WallPoints.Count;

                AffectedPoints.TryGetValue(WallPoints[door], out int val);

                val |= (int)GraveyardFeatures.Gate;

                AffectedPoints[WallPoints[door]] = val;
            }

            WallPoints.Clear();
        }

        internal override void OnAppliedToMaps()
        {
            base.OnAppliedToMaps();

            if (WallTiles.Count > 0) 
            {
                Wall.CreateWalls(WallTiles[0].TileMap, WallTiles, Wall.WallMaterial.Iron);

                if (WallTiles[0].Structure is Wall wall) 
                {
                    (List<Wall> walls, bool circular) = Wall.FindAdjacentWalls(wall);
                    Wall.UnifyWalls(walls, circular);
                }
            }

            if (GateTiles.Count > 0) 
            {
                GateTiles.ForEach(tile =>
                {
                    //Wall wall = tile.Structure as Wall;
                    //if (wall != null && wall.WallType == WallType.Wall)
                    //{
                    //    wall.CreateDoor(tile);
                    //}
                    tile.RemoveStructure(tile.Structure);
                });
                
            }

            WallTiles.Clear();
            GateTiles.Clear();
        }
    }


    internal struct GraveyardParams
    {
        internal FeaturePoint Origin;
        internal int Radius;
        internal int GraveyardRadius;
        internal float TreeDensity;
        internal int GrassFalloffRadius;
        internal int Doors;

        internal GraveyardParams(FeaturePoint origin, int radius = 10, int graveyardRadius = 5, float density = 0.3f)
        {
            Origin = origin;
            Radius = radius;
            TreeDensity = density;

            GraveyardRadius = graveyardRadius;

            GrassFalloffRadius = 5;
            Doors = 4;
        }
    }
}
