using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public class DamageInstance
    {
        public Dictionary<DamageType, float> Damage = new Dictionary<DamageType, float>();

        public float PiercingPercent = 0.5f;

        public bool AmplifiedByNegativeShields = true;

        public AbilityClass AbilityClass = AbilityClass.Unknown;

        public string GetTooltipStrings(Unit sourceUnit) 
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

        public void ApplyDamageBonuses(Unit sourceUnit)
        {
            if(sourceUnit != null)
            {
                float genDamageAdd = sourceUnit.Info.BuffManager.GetValue(BuffEffect.GeneralDamageAdditive);
                float genDamageMult = sourceUnit.Info.BuffManager.GetValue(BuffEffect.GeneralDamageMultiplier);

                List<(DamageType type, float damage)> damageBonuses = new List<(DamageType,float)> ();

                foreach (var kvp in Damage)
                {
                    sourceUnit.Info.BuffManager.GetDamageBonuses(kvp.Key, out float add, out float mult);

                    float damage = (kvp.Value + add + genDamageAdd) * mult * genDamageMult;
                    damageBonuses.Add((kvp.Key, damage));
                }

                foreach(var bonus in damageBonuses)
                {
                    Damage[bonus.type] = bonus.damage;
                }
            }
        }
    }
}
