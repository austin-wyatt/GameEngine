using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    //public class AbilityAttributes
    //{
    //    public List<DamageInstance> DamageInstances = new List<DamageInstance>();
    //}

    public class DamageInstance
    {
        public Dictionary<DamageType, float> Damage = new Dictionary<DamageType, float>();

        public float PiercingPercent = 0.5f;

        public string GetTooltipStrings(Unit castingUnit) 
        {
            string returnString = "";

            foreach (DamageType type in Damage.Keys) 
            {
                if (type == DamageType.NonDamaging) continue;

                if (type == DamageType.Piercing)
                {
                    returnString += $"{Damage[type]} {type} damage ({PiercingPercent.ToString("P0")})\n";
                }
                else 
                {
                    returnString += $"{Damage[type]} {type} damage\n";
                }
            }

            return returnString;
        }
    }
}
