using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public static class VisionHelpers
    {
        public static List<Unit> GetUnitsInRadius(Unit castingUnit, List<Unit> availableUnits, int radius, Scene scene)
        {
            List<Unit> units = new List<Unit>();

            for (int i = 0; i < availableUnits.Count; i++)
            {
                if (TileMap.GetDistanceBetweenPoints(castingUnit.Info.TileMapPosition, availableUnits[i].Info.TileMapPosition) <= radius)
                {
                    units.Add(availableUnits[i]);
                }
            }

            return units;
        }

        public static bool PointInVision(TilePoint pointToCheck, UnitTeam team, List<TemporaryVisionParams> temporaryVisionList)
        {
            HashSet<VisionGenerator> temporarilyRemovedGenerators = new HashSet<VisionGenerator>();

            foreach (var tempVision in temporaryVisionList)
            {
                temporarilyRemovedGenerators.Add(tempVision.Unit.VisionGenerator);
            }


            foreach (var gen in VisionManager.Scene.UnitVisionGenerators)
            {
                if (gen.Team == team && !temporarilyRemovedGenerators.Contains(gen))
                {
                    if (gen.VisibleTiles.Contains(pointToCheck))
                        return true;
                }
            }

            foreach(var tempVision in temporaryVisionList)
            {
                VisionGenerator temp = new VisionGenerator(tempVision.Unit.VisionGenerator);
                temp.SetPosition(tempVision.TemporaryPosition);

                VisionManager.CalculateVision(temp);

                if (temp.VisibleTiles.Contains(pointToCheck))
                {
                    return true;
                }
            }


            return false;
        }
    }

    public struct TemporaryVisionParams
    {
        public Unit Unit;
        public TilePoint TemporaryPosition;
    }
}
