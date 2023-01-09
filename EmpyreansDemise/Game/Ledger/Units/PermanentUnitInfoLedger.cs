using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Ledger.Units
{
    /// <summary>
    /// Stores data specific to permanent units (units that are manually created for features)
    /// </summary>
    public static class PermanentUnitInfoLedger
    {
        public static HashSet<PermanentUnitInfo> UnitInfo = new HashSet<PermanentUnitInfo>();


        public static void SetParameterValue(int permanentId, PermanentUnitInfoParameter param, int value)
        {
            lock (_tempUnitLock)
            {
                _tempUnitInfo.PermanentId = permanentId;
                if (UnitInfo.TryGetValue(_tempUnitInfo, out var unitInfo))
                {
                    unitInfo.SetValue(param, value);
                }
            }
        }

        /// <summary>
        /// Defaults to 0 if the parameter has not been set
        /// </summary>
        public static int GetParameterValue(int permanentId, PermanentUnitInfoParameter param)
        {
            lock (_tempUnitLock)
            {
                _tempUnitInfo.PermanentId = permanentId;
                if (UnitInfo.TryGetValue(_tempUnitInfo, out var unitInfo))
                {
                    return unitInfo.GetValue(param);
                }

                return 0;
            }
        }

        public static PermanentUnitInfo GetUnitPermanentInfo(int permanentId)
        {
            lock (_tempUnitLock)
            {
                _tempUnitInfo.PermanentId = permanentId;
                if (UnitInfo.TryGetValue(_tempUnitInfo, out var unitInfo))
                {
                    return unitInfo;
                }

                return null;
            }
        }

        private static object _tempUnitLock = new object();
        private static PermanentUnitInfo _tempUnitInfo = new PermanentUnitInfo();
    }
}
