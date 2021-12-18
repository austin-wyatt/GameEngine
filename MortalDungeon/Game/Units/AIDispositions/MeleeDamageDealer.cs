using MortalDungeon.Game.Abilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class MeleeDamageDealer : Disposition
    {
        internal float Bloodthirsty;


        private float _unitSeekRange = 50;
        internal MeleeDamageDealer(Unit unit) : base(unit) 
        {
            
        }

        internal override UnitAIAction GetAction(AIAction action)
        {
            float weight = 0;

            Ability meleeAbility = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.MeleeAttack), _unit);
            Unit target;

            if (meleeAbility == null || _unit.Info.Dead)
                return null;

            switch (action) 
            {
                case AIAction.MoveCloser:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = UnitCheckEnum.True, Dead = UnitCheckEnum.False });

                    if (target != null)
                    {
                        if (!meleeAbility.UnitInRange(target) && _unit.Info.Energy >= _unit.Info._movementAbility.EnergyCost && meleeAbility.CanCast())
                        {
                            weight += 1.9f * Weight;

                            MoveInRangeOfAbility returnAction = new MoveInRangeOfAbility(_unit, meleeAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
                case AIAction.AttackEnemyMelee:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = UnitCheckEnum.True, Dead = UnitCheckEnum.False });

                    if (target != null)
                    {
                        if (meleeAbility.UnitInRange(target) && meleeAbility.CanCast())
                        {
                            weight += 2.5f * Weight;

                            weight += (1 - target.Info.Health / target.Info.MaxHealth) * Bloodthirsty;

                            UseAbilityOnUnit returnAction = new UseAbilityOnUnit(_unit, AIAction.AttackEnemyMelee, meleeAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
