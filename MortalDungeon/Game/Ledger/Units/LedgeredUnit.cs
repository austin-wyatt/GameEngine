using MortalDungeon.Game.Save;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Ledger.Units
{

    [XmlType(TypeName = "LU")]
    [Serializable]
    public class LedgeredUnit
    {
        [XmlElement("Lfe")]
        public FeatureUnitEntry FeatureEntry;
        [XmlElement("Lui")]
        public UnitSaveInfo UnitInfo;

        public LedgeredUnit() { }

        public LedgeredUnit(Unit unit)
        {
            FeatureEntry = new FeatureUnitEntry(unit.ObjectHash, unit.FeatureID);
            UnitInfo = new UnitSaveInfo(unit);
        }

        public override bool Equals(object obj)
        {
            return obj is LedgeredUnit unit &&
                   EqualityComparer<FeatureUnitEntry>.Default.Equals(FeatureEntry, unit.FeatureEntry);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FeatureEntry);
        }
    }
}
