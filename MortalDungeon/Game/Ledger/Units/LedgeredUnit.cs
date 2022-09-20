using Empyrean.Game.Save;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Ledger.Units
{

    [XmlType(TypeName = "LU")]
    [Serializable]
    public class LedgeredUnit
    {
        [XmlElement("Lui")]
        public UnitSaveInfo UnitInfo;

        public LedgeredUnit() 
        {
            UnitInfo = new UnitSaveInfo();
        }

        public LedgeredUnit(Unit unit)
        {
            UnitInfo = new UnitSaveInfo(unit, unloadingUnit: true);
        }

        public override bool Equals(object obj)
        {
            return obj is LedgeredUnit unit && unit.UnitInfo.PermanentId == UnitInfo.PermanentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnitInfo.PermanentId);
        }
    }
}
