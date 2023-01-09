using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Ledger.Units
{
    public enum PermanentUnitInfoParameter
    {
        /// <summary>
        /// 0 if the unit is currently alive. <para/>
        /// 1 if the unit is currently dead. <para/>
        /// Resets to 0 when a POI is cleared.
        /// </summary>
        Dead,
        /// <summary>
        /// How many times this particular permanent unit has been killed. 
        /// </summary>
        KillCount,
        /// <summary>
        /// 1 if the unit is currently loaded. 0 if not.
        /// </summary>
        Loaded,
    }

    public class PermanentUnitInfoValue
    {
        public PermanentUnitInfoParameter Parameter;
        public int Value;
    }

    public class PermanentUnitInfo
    {
        public List<PermanentUnitInfoValue> InfoValues = new List<PermanentUnitInfoValue>();
        public int PermanentId = 0;


        public int GetValue(PermanentUnitInfoParameter param)
        {
            for (int i = 0; i < InfoValues.Count; i++)
            {
                if (InfoValues[i].Parameter == param)
                {
                    return InfoValues[i].Value;
                }
            }

            return 0;
        }

        public void SetValue(PermanentUnitInfoParameter param, int value)
        {
            //Remove the parameter if we are setting it to 0
            if (value == 0)
            {
                for (int i = 0; i < InfoValues.Count; i++)
                {
                    if (InfoValues[i].Parameter == param)
                    {
                        InfoValues.RemoveAt(i);

                        return;
                    }
                }

                return;
            }

            //if the value is not zero then modify the parameter (or add the parameter if it does not exist)
            for (int i = 0; i < InfoValues.Count; i++)
            {
                if (InfoValues[i].Parameter == param)
                {
                    InfoValues[i].Value = value;

                    return;
                }
            }

            InfoValues.Add(new PermanentUnitInfoValue() { Parameter = param, Value = value });
        }

        public override bool Equals(object obj)
        {
            return obj is PermanentUnitInfo info &&
                   PermanentId == info.PermanentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PermanentId);
        }
    }
}
