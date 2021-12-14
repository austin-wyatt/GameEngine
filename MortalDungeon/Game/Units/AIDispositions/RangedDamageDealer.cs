using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class RangedDamageDealer : Disposition
    {
        public float Bloodthirsty;


        private float _unitSeekRange = 50;
        public RangedDamageDealer(Unit unit) : base(unit)
        {

        }

        public override UnitAIAction GetAction(AIAction action)
        {
            float weight = 0;

            Ability rangedAbility = GetBestAbility(_unit.GetAbilitiesOfType(AbilityTypes.RangedAttack), _unit);
            Unit target;

            if (rangedAbility == null || _unit.Info.Dead)
                return null;

            UnitAIAction returnAction = null;

            switch (action)
            {
                case AIAction.MoveCloser:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = CheckEnum.True, Dead = CheckEnum.False });

                    if (target != null)
                    {
                        if (!rangedAbility.UnitInRange(target) && _unit.Info.Energy >= _unit.Info._movementAbility.EnergyCost && rangedAbility.CanCast()) 
                        {
                            weight += 1.9f * Weight;

                            returnAction = new MoveInRangeOfAbility(_unit, rangedAbility, null, target)
                            {
                                Weight = weight
                            };
                        }

                        return returnAction;
                    }
                    break;
                case AIAction.AttackEnemyRanged:
                    target = GetClosestUnit(_unit, _unitSeekRange, new UnitSearchParams() { IsHostile = CheckEnum.True, Dead = CheckEnum.False});

                    if (target != null)
                    {
                        if (rangedAbility.UnitInRange(target) && rangedAbility.CanCast())
                        {
                            weight += 2 * Weight;

                            weight += (1 - target.Info.Health / target.Info.MaxHealth) * Bloodthirsty;

                            returnAction = new AttackEnemy(_unit, rangedAbility, null, target) { Weight = weight };
                            return returnAction;
                        }
                    }
                    break;
                //case AIAction.MoveFarther:
                //    target = GetClosestUnit(_unit.AI.EnemyTeam, _unit, _unitSeekRange);
                    
                //    if (target != null)
                //    {
                //        if (rangedAbility.UnitUnderRange(target))
                //        {
                //            //get tile to run away to
                //            List<BaseTile> validTiles = _unit.Info._movementAbility.GetValidTileTargets(Map, Scene._units);
                //            for (int i = 0; i < validTiles.Count; i++) 
                //            {
                //                if (rangedAbility.MinRange > Map.GetDistanceBetweenPoints(target.Info.TileMapPosition.TilePoint, validTiles[i].TilePoint)) 
                //                {
                //                    validTiles.RemoveAt(i);
                //                    i--;
                //                }
                //            }

                //            weight += 1.5f * Weight;

                //            Random rand = new Random();

                //            returnAction = new MoveToTile(_unit, _unit.Info._movementAbility, validTiles[rand.Next() % validTiles.Count], null)
                //            {
                //                Weight = weight
                //            };

                //            return returnAction;
                //        }
                //    }
                //    break;
            }

            return null;
        }
    }
}
