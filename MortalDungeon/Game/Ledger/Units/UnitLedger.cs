using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Ledger.Units
{
    [Serializable]
    [XmlType(TypeName = "FEE")]
    public struct FeatureUnitEntry
    {
        [XmlElement("Fh")]
        public long Hash;
        [XmlElement("Fi")]
        public long FeatureId;

        public FeatureUnitEntry(long hash, long id)
        {
            Hash = hash;
            FeatureId = id;
        }

        public override bool Equals(object obj)
        {
            return obj is FeatureUnitEntry entry &&
                   Hash == entry.Hash &&
                   FeatureId == entry.FeatureId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, FeatureId);
        }
    }

    public static class UnitLedger
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
        public static bool IsUnitLedgered(long objectHash, long featureId)
        {
            lock (_ledgerLock)
            {
                _tempUnit.FeatureEntry.FeatureId = featureId;
                _tempUnit.FeatureEntry.Hash = objectHash;

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
            var mapPos = unit.UnitInfo.Position.ToTileMapPoint();

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
        }

        private static void RemoveUnitFromTileMapPositionDictionary(LedgeredUnit unit)
        {
            var mapPos = unit.UnitInfo.Position.ToTileMapPoint();

            if(LedgeredTileMapPositions.TryGetValue(mapPos, out var ledgeredUnits))
            {
                ledgeredUnits.Remove(unit);
                if(ledgeredUnits.Count == 0)
                {
                    LedgeredTileMapPositions.Remove(mapPos);
                }
            }
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

            Unit newUnit = info.CreateUnit(TileMapManager.Scene);

            unit.UnitInfo.ApplyUnitInfoToUnit(newUnit);

            return new Entity(newUnit);
        }
    }

    public class LedgeredUnitsSaveHelper
    {
        //maybe do this to simplify saving and loading the ledgered units?
    }
}
