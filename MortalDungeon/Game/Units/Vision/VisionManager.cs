using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Units
{
    public static class VisionManager
    {
        /// <summary>
        /// The integer portion of the dictionary stores how many sources have vision of the tile.
        /// </summary>
        public static Dictionary<UnitTeam, Dictionary<TilePoint, int>> ConsolidatedVision = new Dictionary<UnitTeam, Dictionary<TilePoint, int>>();
        public static CombatScene Scene;

        public static bool RevealAll = true;

        static VisionManager()
        {

        }

        public static object _consolidatedVisionLock = new object();
        public static void ConsolidateVision(UnitTeam team)
        {
            if (Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading))
                return;

            lock (_consolidatedVisionLock)
            {
                //clear the consolidated vision dictionary for the team
                if (ConsolidatedVision.TryGetValue(team, out Dictionary<TilePoint, int> result))
                {
                    result.Clear();
                }
                else
                {
                    ConsolidatedVision.Add(team, new Dictionary<TilePoint, int>());
                }
            }

            List<VisionGenerator> teamVisionGenerators = new List<VisionGenerator>();

            foreach (var visionGen in Scene.UnitVisionGenerators)
            {
                if (visionGen.Team == team)
                {
                    teamVisionGenerators.Add(visionGen);
                }
            }

            lock (_consolidatedVisionLock)
            {
                var dict = ConsolidatedVision[team];

                foreach (var visionGen in teamVisionGenerators)
                {
                    lock (visionGen._visibleTilesLock)
                    {
                        foreach (var tile in visionGen.VisibleTiles)
                        {
                            if(dict.ContainsKey(tile))
                            {
                                dict[tile]++;
                            }
                            else
                            {
                                dict.TryAdd(tile, 1);
                            }
                        }
                    }
                }

                foreach (var tempVision in Scene.TemporaryVision)
                {
                    if (tempVision.Team == team)
                    {
                        for (int i = 0; i < tempVision.TilesToReveal.Count; i++)
                        {
                            if (dict.ContainsKey(tempVision.TilesToReveal[i]))
                            {
                                dict[tempVision.TilesToReveal[i]]++;
                            }
                            else
                            {
                                dict.TryAdd(tempVision.TilesToReveal[i], 1);
                            }
                        }
                    }
                }
            }

            Scene.CalculateRevealedUnits();
            Scene.EvaluateCombat(team);

            Task.Run(() =>
            {
                Scene.HideNonVisibleObjects();
                if(team == Scene.VisibleTeam)
                {
                    Scene.RenderDispatcher.DispatchAction(Scene._structureDispatchObject, Scene.CreateStructureInstancedRenderData);
                }

                lock (_consolidatedActionsLock)
                {
                    for (int i = 0; i < VisionConsolidatedActions.Count; i++)
                    {
                        VisionConsolidatedActions.Pop().Invoke();
                    }
                }
            });
        }

        private static Stack<Action> VisionConsolidatedActions = new Stack<Action>();
        private static object _consolidatedActionsLock = new object();

        public static void CalculateVisionForUnits(IEnumerable<Unit> units)
        {
            HashSet<UnitTeam> unitTeams = new HashSet<UnitTeam>();

            foreach (var unit in units)
            {
                CalculateVision(unit.VisionGenerator);
                unitTeams.Add(unit.AI.GetTeam());
            }

            foreach (var team in unitTeams)
            {
                ConsolidateVision(team);
            }
        }

        public static void CalculateVisionForUnit(Unit unit)
        {
            CalculateVision(unit.VisionGenerator);

            ConsolidateVision(unit.AI.GetTeam());
        }


        private static ObjectPool<List<List<Direction>>> _digitalLineListPool = new ObjectPool<List<List<Direction>>>();
        private static ObjectPool<List<Direction>> _directionListPool = new ObjectPool<List<Direction>>();
        public static void CalculateVision(VisionGenerator generator, bool updateVisibleMaps = true)
        {
            var affectedMapsOld = generator.AffectedMaps;

            lock (generator._visibleTilesLock)
            {
                generator.VisibleTiles.Clear();
            }
            generator.AffectedMaps = TileMap.TileMapSetPool.GetObject();


            Tile tile = TileMapHelpers.GetTile(new FeaturePoint(generator.Position));

            List<Tile> tileList = Tile.TileListPool.GetObject();

            tile.TileMap.GetRingOfTiles(tile, tileList, (int)generator.Radius);

            #region precalculate this later
            List<List<Direction>> digitalLines = _digitalLineListPool.GetObject();

            for (int i = 0; i < tileList.Count / 6; i++)
            {
                if (tileList[i] == null)
                    continue;

                var line = tile.TileMap.GetLineOfTiles(tile, tileList[i]);

                List<Direction> digitalLine = _directionListPool.GetObject();

                Tile prev = null;

                foreach (var t in line)
                {
                    if (prev != null)
                    {
                        digitalLine.Add(FeatureEquation.DirectionBetweenTiles(prev, t));
                    }

                    prev = t;
                }
                digitalLines.Add(digitalLine);
            }
            #endregion

            tileList.Clear();
            Tile.TileListPool.FreeObject(ref tileList);

            lock (generator._visibleTilesLock)
            {
                generator.VisibleTiles.Add(tile.TilePoint);
            }
            generator.AffectedMaps.Add(tile.TileMap);

            int j;
            Tile currentTile;

            for (int i = 0; i < 6; i++)
            {
                for (j = 0; j < digitalLines.Count; j++)
                {
                    var line = digitalLines[j];

                    currentTile = tile;

                    for(int k = 0; k < line.Count; k++)
                    {
                        var dir = line[k];

                        currentTile = tile.TileMap.GetNeighboringTile(currentTile, (Direction)((int)(dir + i) % 6));

                        if (currentTile == null)
                            break;

                        lock (generator._visibleTilesLock)
                        {
                            generator.VisibleTiles.Add(currentTile.TilePoint);
                        }
                        generator.AffectedMaps.Add(currentTile.TileMap);

                        bool currTileHigher = (tile.GetVisionHeight() - currentTile.GetVisionHeight()) <= -2;
                        bool currTileLower = (tile.GetVisionHeight() - currentTile.GetVisionHeight()) >= 1;
                        if ((currentTile.BlocksType(BlockingType.Vision) && !currTileLower) || currTileHigher)
                        {
                            break;
                        }
                    }
                }
            }


            if (generator.Team == Scene.VisibleTeam && updateVisibleMaps)
            {
                void updateVision()
                {
                    foreach (var map in generator.AffectedMaps)
                    {
                        map.UpdateChunks(TileUpdateType.Vision);
                    }

                    foreach (var map in affectedMapsOld)
                    {
                        map.UpdateChunks(TileUpdateType.Vision);
                    }

                    affectedMapsOld.Clear();
                    TileMap.TileMapSetPool.FreeObject(ref affectedMapsOld);
                }

                lock (_consolidatedActionsLock)
                {
                    VisionConsolidatedActions.Push(updateVision);
                }
            }
            else
            {
                affectedMapsOld.Clear();
                TileMap.TileMapSetPool.FreeObject(ref affectedMapsOld);
            }

            List<Direction> list;
            for(int i = 0; i < digitalLines.Count; i++)
            {
                list = digitalLines[i];
                list.Clear();
                _directionListPool.FreeObject(ref list);
            }

            digitalLines.Clear();
            _digitalLineListPool.FreeObject(ref digitalLines);
        }

        /// <summary>
        /// Calculates what can see the passed VisionGenerator in a given radius.
        /// (ie the height calculation is reversed as the edges of the radius -> center is what we care about)
        /// </summary>
        public static List<List<Tile>> CalculateVisionLinesToGenerator(VisionGenerator generator)
        {
            List<List<Tile>> visionLines = new List<List<Tile>>();

            Tile tile = TileMapHelpers.GetTile(new FeaturePoint(generator.Position));

            List<Tile> tileList = new List<Tile>();

            tile.TileMap.GetRingOfTiles(tile, tileList, (int)generator.Radius);

            #region precalculate this later
            List<List<Direction>> digitalLines = new List<List<Direction>>();

            for (int i = 0; i < tileList.Count / 6; i++)
            {
                if (tileList[i] == null)
                    continue;

                var line = tile.TileMap.GetLineOfTiles(tile, tileList[i]);

                List<Direction> digitalLine = new List<Direction>();

                Tile prev = null;

                foreach (var t in line)
                {
                    if (prev != null)
                    {
                        digitalLine.Add(FeatureEquation.DirectionBetweenTiles(prev, t));
                    }

                    prev = t;
                }
                digitalLines.Add(digitalLine);
            }
            #endregion

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < digitalLines.Count; j++)
                {
                    var visionLine = new List<Tile>();
                    visionLines.Add(visionLine);

                    var line = digitalLines[j];

                    Tile currentTile = tile;

                    for (int k = 0; k < line.Count; k++)
                    {
                        var dir = line[k];

                        currentTile = tile.TileMap.GetNeighboringTile(currentTile, (Direction)((int)(dir + i) % 6));

                        if (currentTile == null)
                            break;

                        visionLine.Add(currentTile);

                        bool currTileHigher = (tile.GetVisionHeight() - currentTile.GetVisionHeight()) <= -2;
                        bool currTileLower = (tile.GetVisionHeight() - currentTile.GetVisionHeight()) >= 2;
                        if ((currentTile.BlocksType(BlockingType.Vision) && !currTileHigher) || currTileLower)
                        {
                            break;
                        }
                    }
                }
            }

            return visionLines;
        } 

        public static void SetRevealAll(bool revealAll)
        {
            bool update = revealAll != RevealAll;

            RevealAll = revealAll;

            if (update)
            {
                lock (TileMapManager._visibleMapLock)
                {
                    foreach (var map in TileMapManager.VisibleMaps)
                    {
                        map.UpdateChunks(TileUpdateType.Vision);
                    }
                }

                Scene.CalculateRevealedUnits();
                Scene.HideNonVisibleObjects();
                Scene.EvaluateCombat(Scene.VisibleTeam);
                Scene.RenderDispatcher.DispatchAction(Scene._structureDispatchObject, Scene.CreateStructureInstancedRenderData);
            }
        }
    }
}
