using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public static class UnitPositionManager
    {
        public static Dictionary<TilePoint, HashSet<Unit>> UnitPositions = new Dictionary<TilePoint, HashSet<Unit>>();
        public static Dictionary<TileMapPoint, HashSet<Unit>> UnitMapPositions = new Dictionary<TileMapPoint, HashSet<Unit>>();

        public static void SetUnitPosition(Unit unit, TilePoint position)
        {
            if (UnitMapPositions.TryGetValue(position.ParentTileMap.TileMapCoords, out var unitMapSet))
            {
                unitMapSet.Add(unit);
            }
            else
            {
                HashSet<Unit> newUnitSet = new HashSet<Unit>();
                newUnitSet.Add(unit);

                UnitMapPositions.Add(position.ParentTileMap.TileMapCoords, newUnitSet);
            }

            if (UnitPositions.TryGetValue(position, out var units))
            {
                units.Add(unit);
            }
            else
            {
                HashSet<Unit> unitSet = new HashSet<Unit>();
                unitSet.Add(unit);

                UnitPositions.Add(position, unitSet);
            }
        }

        public static void RemoveUnitPosition(Unit unit, TilePoint position)
        {
            if(UnitPositions.TryGetValue(position, out var units))
            {
                units.Remove(unit);

                if(units.Count == 0)
                {
                    UnitPositions.Remove(position);
                }
            }

            if (UnitMapPositions.TryGetValue(position.ParentTileMap.TileMapCoords, out var unitsOnMap))
            {
                unitsOnMap.Remove(unit);

                if (unitsOnMap.Count == 0)
                {
                    UnitMapPositions.Remove(position.ParentTileMap.TileMapCoords);
                }
            }
        }

        private static readonly HashSet<Unit> _emptySet = new HashSet<Unit>();
        public static HashSet<Unit> GetUnitsOnTilePoint(TilePoint point)
        {
            if(UnitPositions.TryGetValue(point, out var units))
            {
                return units;
            }
            else
            {
                return _emptySet;
            }
        }
    }
}
