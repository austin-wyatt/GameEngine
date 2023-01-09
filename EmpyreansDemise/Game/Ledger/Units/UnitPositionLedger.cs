using Empyrean.Game.Entities;
using Empyrean.Game.Map;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Ledger.Units
{
    public static class UnitPositionLedger
    {
        public static HashSet<LedgeredUnit> LedgeredUnits = new HashSet<LedgeredUnit>();

        public static Dictionary<TileMapPoint, HashSet<LedgeredUnit>> LedgeredTileMapPositions = new Dictionary<TileMapPoint, HashSet<LedgeredUnit>>();
        

        /// <summary>
        /// Units should be ledgered when their entity is unloaded.
        /// When a save state is triggered all active entities should be ledgered.
        /// </summary>
        /// <param name="unit"></param>
        public static void LedgerUnit(Unit unit)
        {
            LedgeredUnit ledgeredUnit = new LedgeredUnit(unit);

            if (!LedgeredUnits.Add(ledgeredUnit))
            {
                RemoveUnitFromLedger(ledgeredUnit);
                LedgeredUnits.Add(ledgeredUnit);
            }

            AddUnitToTileMapPositionDictionary(ledgeredUnit);
        }

        /// <summary>
        /// Units should be removed from the ledger when they are loaded as entities
        /// </summary>
        public static void RemoveUnitFromLedger(Unit unit)
        {
            LedgeredUnit ledgeredUnit = new LedgeredUnit(unit);
            RemoveUnitFromLedger(ledgeredUnit);
        }

        public static void RemoveUnitFromLedger(LedgeredUnit unit)
        {
            LedgeredUnits.Remove(unit);
            RemoveUnitFromTileMapPositionDictionary(unit);
        }

        private static LedgeredUnit _tempUnit = new LedgeredUnit();
        private static object _ledgerLock = new object();
        public static bool IsUnitLedgered(int permanentId)
        {
            lock (_ledgerLock)
            {
                _tempUnit.UnitInfo.PermanentId = permanentId;

                return LedgeredUnits.Contains(_tempUnit);
            }
        }

        /// <summary>
        /// This should be called after a save state is loaded
        /// </summary>
        public static void BuildTileMapPositionDictionary()
        {
            LedgeredTileMapPositions.Clear();
            foreach(var unit in LedgeredUnits)
            {
                AddUnitToTileMapPositionDictionary(unit);
            }
        }

        private static void AddUnitToTileMapPositionDictionary(LedgeredUnit unit)
        {
            TileMapPoint mapPos = TileMapPoint.Pool.GetObject();

             unit.UnitInfo.Position.ToTileMapPoint(ref mapPos);

            if (LedgeredTileMapPositions.TryGetValue(mapPos, out var units))
            {
                units.Add(unit);
            }
            else
            {
                HashSet<LedgeredUnit> newSet = new HashSet<LedgeredUnit>();
                newSet.Add(unit);

                LedgeredTileMapPositions.Add(mapPos, newSet);
            }

            TileMapPoint.Pool.FreeObject(ref mapPos);
        }

        private static void RemoveUnitFromTileMapPositionDictionary(LedgeredUnit unit)
        {
            TileMapPoint mapPos = TileMapPoint.Pool.GetObject();
            unit.UnitInfo.Position.ToTileMapPoint(ref mapPos);

            if(LedgeredTileMapPositions.TryGetValue(mapPos, out var ledgeredUnits))
            {
                ledgeredUnits.Remove(unit);
                if(ledgeredUnits.Count == 0)
                {
                    LedgeredTileMapPositions.Remove(mapPos);
                }
            }

            TileMapPoint.Pool.FreeObject(ref mapPos);
        }

        private static HashSet<LedgeredUnit> _emptySet = new HashSet<LedgeredUnit>();
        public static HashSet<LedgeredUnit> GetLedgeredUnitsOnTileMap(TileMapPoint point)
        {
            if(LedgeredTileMapPositions.TryGetValue(point, out var units))
            {
                return units;
            }
            else
            {
                return _emptySet;
            }
        }

        public static Entity CreateEntityFromLedgeredUnit(LedgeredUnit unit)
        {
            UnitCreationInfo info = UnitInfoBlockManager.GetUnit(unit.UnitInfo.UnitCreationInfoId);

            if (info == null)
                return null;

            

            Unit newUnit = info.CreateUnit(TileMapManager.Scene, firstLoad: false);

            unit.UnitInfo.ApplyUnitInfoToUnit(newUnit);

            return new Entity(newUnit);
        }
    }
}
