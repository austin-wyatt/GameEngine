using MortalDungeon.Game.Abilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class Utility : Disposition
    {
        public float Virtuous = 3;


        private float _unitSeekRange = 50;
        public Utility(Unit unit) : base(unit)
        {

        }

        public override UnitAIAction GetAction(AIAction action)
        {
            float weight = 0;

            List<Ability> buffAbilities = new List<Ability>();

            Ability buffDefensive = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.BuffDefensive), _unit);
            Ability buffOffensive = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.BuffOffensive), _unit);

            if(buffOffensive != null) 
            {
                buffAbilities.Add(buffOffensive);
            }
            if (buffDefensive != null)
            {
                buffAbilities.Add(buffDefensive);
            }

            Ability buff = null;

            if(buffAbilities.Count > 0) 
            {
                buff = buffAbilities[GlobalRandom.Next(buffAbilities.Count)];
            }


            Unit target = null;

            if (buff == null || _unit.Info.Dead)
                return null;

            List<Unit> potentialTargets;

            switch (action)
            {
                //case AIAction.MoveCloser:
                //    potentialTargets = GetReasonablyCloseUnits(_unit, _unit.Info.Speed, new UnitSearchParams() { IsFriendly = UnitCheckEnum.True, Dead = UnitCheckEnum.False });

                //    foreach (var u in potentialTargets)
                //    {
                //        if (target == null || u.Info.Health / u.Info.MaxHealth < target.Info.Health / target.Info.MaxHealth)
                //        {
                //            target = u;
                //        }
                //    }

                //    if (target != null)
                //    {
                //        if (!buffDefensive.UnitInRange(target) && _unit.Info.Energy >= _unit.Info._movementAbility.EnergyCost && buffDefensive.CanCast())
                //        {
                //            weight += 1.05f * Weight;

                //            //weight += (1 - target.Info.Health / target.Info.MaxHealth) * Virtuous;

                //            MoveInRangeOfAbility returnAction = new MoveInRangeOfAbility(_unit, buffDefensive, null, target) { Weight = weight };
                //            return returnAction;
                //        }
                //    }
                //    break;
                case AIAction.BuffAlly:
                    buff.GetValidTileTargets(_unit.GetTileMap(), Scene._units);
                    potentialTargets = buff.AffectedUnits;

                    target = potentialTargets[GlobalRandom.Next(potentialTargets.Count)];

                    if (target != null)
                    {
                        if (buff.UnitInRange(target) && buff.CanCast())
                        {
                            weight += 2.5f * Weight;

                            //weight += (1 - target.Info.Health / target.Info.MaxHealth) * Virtuous;

                            UseAbilityOnUnit returnAction = new UseAbilityOnUnit(_unit, AIAction.BuffAlly, buff, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
            }

            return null;
        }

        public override void OnActionSelected(AIAction action)
        {
            base.OnActionSelected(action);

            if (action == AIAction.BuffAlly) 
            {
                TurnFatigue += 1;
            }
        }
    }
}
