using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Units
{
    public struct AppliedDamageReturnValues
    {
        public float DamageBlockedByShields;
        public float ActualDamageDealt;
        public bool KilledEnemy;
        public bool AttackBrokeShield;
        public float DamageResisted;
        public float PotentialDamageBeforeModifications;
    }

    public interface IUnit
    {
        public UnitInfo Info { get; set; }
        public UnitAI AI { get; set; }

        public AppliedDamageReturnValues ApplyDamage(DamageParams damageParams, bool spoofDamage = false);
    }
}
