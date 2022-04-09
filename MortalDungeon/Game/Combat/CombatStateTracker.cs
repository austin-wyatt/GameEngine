using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Combat
{
    public enum MorselType
    {
        All,
        Action,
        Turn
    }

    public class CombatStateTracker
    {
        public Dictionary<UnitTeam, HashSet<UnitMorsels>> UnitInformation = new Dictionary<UnitTeam, HashSet<UnitMorsels>>();

        public static CombatScene Scene;

        public Dictionary<Unit, List<LineOfTiles>> UnimpededUnitSightlines = new Dictionary<Unit, List<LineOfTiles>>();


        public void StartCombat()
        {
            UnimpededUnitSightlines.Clear();

            AddMorselForAll();

            foreach(var unit in Scene.UnitsInCombat)
            {
                CalculateUnimpededSightlinesForUnit(unit);
            }

            Scene.UnitMoved += CalculateUnimpededSightlinesForUnit;
            Scene.UnitAddedToCombat += CalculateUnimpededSightlinesForUnit;
        }

        public void EndCombat()
        {
            UnitInformation.Clear();
            UnimpededUnitSightlines.Clear();

            Scene.UnitMoved -= CalculateUnimpededSightlinesForUnit;
            Scene.UnitAddedToCombat -= CalculateUnimpededSightlinesForUnit;
        }

        public void AddMorselForAll()
        {
            foreach (var unit in Scene.UnitsInCombat)
            {
                CreateMorsel(unit, MorselType.All);
            }
        }

        public void CreateMorsel(Unit unit, MorselType morselType)
        {
            foreach(var team in Scene.ActiveTeams)
            {
                if(VisionManager.ConsolidatedVision.TryGetValue(team, out var vision))
                {
                    if(vision.TryGetValue(unit.Info.TileMapPosition, out int visionCount))
                    {
                        if(visionCount > 0)
                        {
                            HashSet<UnitMorsels> morsels;
                            UnitMorsels newMorsel = new UnitMorsels(unit);

                            if (UnitInformation.TryGetValue(team, out morsels)) 
                            {
                                if (morsels.TryGetValue(newMorsel, out var foundMorsel))
                                {
                                    newMorsel = foundMorsel;
                                }
                                else
                                {
                                    morsels.Add(newMorsel);
                                }
                            }
                            else
                            {
                                morsels = new HashSet<UnitMorsels>();
                                morsels.Add(newMorsel);

                                UnitInformation.Add(team, morsels);
                            }


                            switch (morselType)
                            {
                                case MorselType.Action:
                                    newMorsel.CreateActionMorsel();
                                    break;
                                case MorselType.Turn:
                                    newMorsel.CreateTurnMorsel();
                                    break;
                                case MorselType.All:
                                    newMorsel.CreateTurnMorsel();
                                    newMorsel.CreateActionMorsel();
                                    break;

                            }
                        }
                    }
                }
            }
        }

        public void CalculateUnimpededSightlinesForUnit(Unit unit)
        {
            UnimpededUnitSightlines.Remove(unit);

            var lineOfTilesList = new List<LineOfTiles>();

            UnimpededUnitSightlines.Add(unit, lineOfTilesList);

            VisionGenerator gen = new VisionGenerator()
            {
                Position = Map.FeatureEquation.PointToMapCoords(unit.Info.TileMapPosition),
                Radius = 10
            };

            var visionLines = VisionManager.CalculateVisionLinesToGenerator(gen);

            for(int i = 0; i < visionLines.Count; i++)
            {
                lineOfTilesList.Add(new LineOfTiles(visionLines[i]));
            }
        }
    }

    public class LineOfTiles
    {
        public List<Tile> Tiles = new List<Tile>();

        public LineOfTiles(List<Tile> tiles)
        {
            Tiles = tiles;
        }
    }
}
