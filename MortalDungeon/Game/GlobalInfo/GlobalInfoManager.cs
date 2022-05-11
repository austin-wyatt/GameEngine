using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Save
{
    /// <summary>
    /// Houses a handful of external helper functions for interacting with GlobalInfo
    /// </summary>
    public static class GlobalInfoManager
    {
        public static int GetPOIParameter(int poiId, POIParameterType parameter)
        {
            var entry = GlobalInfo.GetPOI(poiId);
            if (entry == null) return 0;

            return entry.GetParameterValue(parameter);
        }

        public static void SetPOIParameter(int poiId, POIParameterType parameter, int value)
        {
            var entry = GlobalInfo.GetPOI(poiId);
            if (entry == null) return;
            GlobalInfo.WillModify(ref entry);

            entry.SetParameterValue(parameter, value);
        }

        public static void RemovePOIParameter(int poiId, POIParameterType parameter)
        {
            var entry = GlobalInfo.GetPOI(poiId);
            if (entry == null) return;
            GlobalInfo.WillModify(ref entry);

            entry.SetParameterValue(parameter, 0);
        }

        public static void IncrementPOIParameter(int poiId, POIParameterType parameter)
        {
            var entry = GlobalInfo.GetPOI(poiId);
            if (entry == null) return;
            GlobalInfo.WillModify(ref entry);

            entry.IncrementParameterValue(parameter);
        }

        public static void DecrementPOIParameter(int poiId, POIParameterType parameter)
        {
            var entry = GlobalInfo.GetPOI(poiId);
            if (entry == null) return;
            GlobalInfo.WillModify(ref entry);

            entry.DecrementParameterValue(parameter);
        }
    }
}
