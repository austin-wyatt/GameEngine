using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Units
{
    public static class VisionManager
    {
        public static Dictionary<UnitTeam, Dictionary<TilePoint, bool>> ConsolidatedVision = new Dictionary<UnitTeam, Dictionary<TilePoint, bool>>();
        public static CombatScene Scene;

        public static bool RevealAll = true;

        static VisionManager()
        {

        }

        public static object _consolidatedVisionLock = new object();
        public static void ConsolidateVision(UnitTeam team)
        {
            lock (_consolidatedVisionLock)
            {
                //clear the consolidated vision dictionary for the team
                if (ConsolidatedVision.TryGetValue(team, out Dictionary<TilePoint, bool> result))
                {
                    result.Clear();
                }
                else
                {
                    ConsolidatedVision.Add(team, new Dictionary<TilePoint, bool>());
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
                            dict.TryAdd(tile, true);
                        }
                    }
                }

                foreach (var tempVision in Scene.TemporaryVision)
                {
                    if (tempVision.Team == team)
                    {
                        for (int i = 0; i < tempVision.TilesToReveal.Count; i++)
                        {
                            dict.TryAdd(tempVision.TilesToReveal[i], true);
                        }
                    }
                }
            }


            Task.Run(() =>
            {
                Scene.CalculateRevealedUnits();
                Scene.HideNonVisibleObjects();
                Scene.EvaluateCombat();
            });
        }

        public static void CalculateVisionForUnits(IEnumerable<Unit> units)
        {
            HashSet<UnitTeam> unitTeams = new HashSet<UnitTeam>();

            foreach (var unit in units)
            {
                CalculateVision(unit.VisionGenerator);
                unitTeams.Add(unit.AI.Team);
            }

            foreach (var team in unitTeams)
            {
                ConsolidateVision(team);
            }
        }

        public static void CalculateVisionForUnit(Unit unit)
        {
            CalculateVision(unit.VisionGenerator);

            ConsolidateVision(unit.AI.Team);
        }

        private static Dictionary<FeaturePoint, LightObstruction> _sceneLightObstructions = new Dictionary<FeaturePoint, LightObstruction>();
        public static void PrepareLightObstructions(IEnumerable<LightObstruction> obstructions)
        {
            _sceneLightObstructions.Clear();

            foreach (var obs in obstructions)
            {
                _sceneLightObstructions.TryAdd(new FeaturePoint(obs.Position), obs);
            }
        }

        public static void CalculateVision(VisionGenerator generator, bool updateVisibleMaps = true)
        {
            var affectedMapsOld = generator.AffectedMaps;

            lock (generator._visibleTilesLock)
            {
                generator.VisibleTiles.Clear();
            }
            generator.AffectedMaps = new HashSet<TileMap>();

            BaseTile tile = TileMapHelpers.GetTile(new FeaturePoint(generator.Position));

            List<BaseTile> tileList = new List<BaseTile>();

            tile.TileMap.GetRingOfTiles(tile, tileList, (int)generator.Radius);

            #region precalculate this later
            List<List<Direction>> digitalLines = new List<List<Direction>>();

            for (int i = 0; i < tileList.Count / 6; i++)
            {
                var line = tile.TileMap.GetLineOfTiles(tile, tileList[i]);

                List<Direction> digitalLine = new List<Direction>();

                BaseTile prev = null;

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

            lock (generator._visibleTilesLock)
            {
                generator.VisibleTiles.Add(tile.TilePoint);
            }
            generator.AffectedMaps.Add(tile.TileMap);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < digitalLines.Count; j++)
                {
                    var line = digitalLines[j];

                    BaseTile currentTile = tile;

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

                        //if (generator.Team == Scene.VisibleTeam)
                        //{
                        //    currentTile.Update();
                        //}

                        if (_sceneLightObstructions.TryGetValue(currentTile.ToFeaturePoint(), out var obstruction) || 
                            ((tile.GetVisionHeight() - currentTile.GetVisionHeight()) <= -2))
                        {
                            break;
                        }
                    }
                }
            }

            if (generator.Team == Scene.VisibleTeam && updateVisibleMaps)
            {
                foreach (var map in generator.AffectedMaps)
                {
                    map.UpdateTile(map.Tiles[0]);
                }

                foreach (var map in affectedMapsOld)
                {
                    map.UpdateTile(map.Tiles[0]);
                }
            }
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
                        map.UpdateTile(map.Tiles[0]);
                    }
                }

                Scene.CalculateRevealedUnits();
                Scene.HideNonVisibleObjects();
                Scene.EvaluateCombat();
            }
        }
    }
}
