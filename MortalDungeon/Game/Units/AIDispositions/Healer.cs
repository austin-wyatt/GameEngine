using MortalDungeon.Game.Abilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class Healer : Disposition
    {
        internal float Virtuous = 3;
        internal float HPThreshold = 0.5f;


        private float _unitSeekRange = 50;
        internal Healer(Unit unit) : base(unit)
        {

        }

        internal override UnitAIAction GetAction(AIAction action)
        {
            float weight = 0;

            Ability healAbility = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.Heal), _unit);
            Unit target = null;

            if (healAbility == null || _unit.Info.Dead)
                return null;

            List<Unit> potentialTargets;

            switch (action)
            {
                case AIAction.MoveCloser:
                    potentialTargets = GetReasonablyCloseUnits(_unit, _unit.Info.Speed, new UnitSearchParams() { IsFriendly = UnitCheckEnum.True, Dead = UnitCheckEnum.False });

                    foreach (var u in potentialTargets) 
                    {
                        if(target == null || u.Info.Health / u.Info.MaxHealth < target.Info.Health / target.Info.MaxHealth) 
                        {
                            target = u;
                        }
                    }

                    if (target != null)
                    {
                        if (!healAbility.UnitInRange(target) && _unit.Info.Energy >= _unit.Info._movementAbility.EnergyCost && healAbility.CanCast())
                        {
                            weight += 0.5f * Weight;

                            weight += (1 - target.Info.Health / target.Info.MaxHealth) * Virtuous;

                            MoveInRangeOfAbility returnAction = new MoveInRangeOfAbility(_unit, healAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
                case AIAction.HealAlly:
                    potentialTargets = GetReasonablyCloseUnits(_unit, _unit.Info.Speed, new UnitSearchParams() { Self = UnitCheckEnum.SoftTrue, IsFriendly = UnitCheckEnum.True, Dead = UnitCheckEnum.False });

                    foreach (var u in potentialTargets)
                    {
                        if (target == null || u.Info.Health / u.Info.MaxHealth < target.Info.Health / target.Info.MaxHealth)
                        {
                            target = u;
                        }
                    }

                    if (target != null)
                    {
                        if (healAbility.UnitInRange(target) && healAbility.CanCast())
                        {
                            if (healAbility.GetDamageInstance().Damage[DamageType.Healing] >= (target.Info.MaxHealth - target.Info.Health)
                                || target.Info.Health / target.Info.MaxHealth < HPThreshold)
                            {
                                weight += 0.5f * Weight;

                                weight += (1 - target.Info.Health / target.Info.MaxHealth) * Virtuous;

                                UseAbilityOnUnit returnAction = new UseAbilityOnUnit(_unit, AIAction.HealAlly, healAbility, null, target) { Weight = weight };
                                return returnAction;
                            }
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
