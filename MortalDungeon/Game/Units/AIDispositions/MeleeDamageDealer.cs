using MortalDungeon.Game.Abilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class MeleeDamageDealer : Disposition
    {
        public float Bloodthirsty;


        private float _unitSeekRange = 50;
        public MeleeDamageDealer(Unit unit) : base(unit) 
        {
            
        }

        public override UnitAIAction GetAction(AIAction action)
        {
            float weight = 0;

            Ability meleeAbility = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.MeleeAttack), _unit);
            Unit target;

            if (meleeAbility == null)
                return null;

            switch (action) 
            {
                case AIAction.MoveCloser:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = CheckEnum.True, Dead = CheckEnum.False });

                    if (target != null)
                    {
                        if (!meleeAbility.UnitInRange(target))
                        {
                            weight += 2 * Weight;

                            MoveToUnit returnAction = new MoveToUnit(_unit, _unit.Info._movementAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
                case AIAction.AttackEnemyMelee:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = CheckEnum.True, Dead = CheckEnum.False });

                    if (target != null)
                    {
                        if (meleeAbility.UnitInRange(target))
                        {
                            weight += 2 * Weight;

                            weight += (1 - target.Info.Health / UnitInfo.MaxHealth) * Bloodthirsty;

                            AttackEnemy returnAction = new AttackEnemy(_unit, meleeAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
