using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    //internal class AbilityAttributes
    //{
    //    internal List<DamageInstance> DamageInstances = new List<DamageInstance>();
    //}

    internal class DamageInstance
    {
        internal Dictionary<DamageType, float> Damage = new Dictionary<DamageType, float>();

        internal float PiercingPercent = 0.5f;

        internal string GetTooltipStrings(Unit castingUnit) 
        {
            string returnString = "";

            foreach (DamageType type in Damage.Keys) 
            {
                if (type == DamageType.NonDamaging) continue;

                if (type == DamageType.Piercing)
                {
                    returnString += $"{Damage[type]} {type} damage ({PiercingPercent.ToString("P0")})\n";
                }
                else if (type == DamageType.Healing) 
                {
                    returnString += $"{Damage[type]} healing\n";
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
