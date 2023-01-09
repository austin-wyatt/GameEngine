using Empyrean.Game.Combat;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    public partial class TemplateRangedSingleTarget
    {
        protected struct WeightParamsStruct 
        {
            public float BaseWeight;
            public float AllyWeight;
            public float EnemyWeight;
            public float NeutralWeight;

            /// <summary>
            /// Accepts the weight, ability, and unit morsel as parameters<para/>
            /// Returns the new weight
            /// </summary>
            public List<Func<float, Ability, InformationMorsel, float>> WeightModifications;

            public WeightParamsStruct(WeightParamsStruct temp)
            {
                BaseWeight = temp.BaseWeight;
                AllyWeight = temp.AllyWeight;
                EnemyWeight = temp.EnemyWeight;
                NeutralWeight = temp.NeutralWeight;

                WeightModifications = new List<Func<float, Ability, InformationMorsel, float>>
                {
                    OpportunismModification,
                    BloodthirstModification
                };
            }

            public static WeightParamsStruct Base = new WeightParamsStruct()
            {
                BaseWeight = 1.5f,
                AllyWeight = 0,
                EnemyWeight = 0,
                NeutralWeight = 0,
                WeightModifications = new List<Func<float, Ability, InformationMorsel, float>>()
            };

            public static Func<float, Ability, InformationMorsel, float> OpportunismModification = (weight, ability, morsel) =>
            {
                //opportunism
                weight *= 1 + ability.CastingUnit.AI.Feelings.GetFeelingValue(FeelingType.Opportunism, morsel);

                return weight;
            };
            public static Func<float, Ability, InformationMorsel, float> BloodthirstModification = (weight, ability, morsel) =>
            {
                //bloodthirst
                weight *= 1 + ability.CastingUnit.AI.Feelings.GetFeelingValue(FeelingType.Bloodthirst, morsel);

                return weight;
            };
        }

        protected WeightParamsStruct WeightParams = new WeightParamsStruct(WeightParamsStruct.Base);

        protected class TemplateAIAction : IAIAction
        {
            public float Weight { get; set; }

            private InformationMorsel Morsel;
            private WeightParamsStruct WeightParams;
            private Ability Ability;

            public TemplateAIAction(Ability ability, InformationMorsel morsel, WeightParamsStruct weightParams)
            {
                Morsel = morsel;
                WeightParams = weightParams;
                Ability = ability;

                CalculateWeight();
            }

            public async Task<bool> ActionChosen()
            {
                TilePoint morselPos = Morsel.Position.ToTilePoint();

                if (Ability.GetPositionValid(Ability.CastingUnit.Info.TileMapPosition.TilePoint, morselPos))
                {
                    Ability.SelectionInfo.SelectedUnits.Add(Morsel.Unit);
                    Ability.EnactEffect();
                    return true;
                }
                else if(TileMapManager.NavMesh.GetPathToPoint(Ability.CastingUnit.Info.TileMapPosition.ToFeaturePoint(), 
                    Morsel.Position, Ability.CastingUnit.Info._movementAbility.NavType, 
                    out var feelerList, Ability.Range + Ability.CastingUnit.Info._movementAbility.GetRange(),
                    Ability.CastingUnit, considerCaution: true, allowEndInUnit: true))
                {
                    //Here we know that a path to the unit exists.

                    //check all the points in the feeler path 
                    foreach(var tile in feelerList)
                    {
                        if(Ability.GetPositionValid(tile.TilePoint, morselPos))
                        {
                            if (await AIBrain.MovementCheck(Ability.CastingUnit, tile.ToFeaturePoint(), null))
                            {
                                if(Ability.GetPositionValid(Ability.CastingUnit.Info.TileMapPosition, morselPos))
                                {
                                    Ability.SelectionInfo.SelectedUnits.Add(Morsel.Unit);
                                    Ability.EnactEffect();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                //if we fail the movement check then we don't have enough movement to continue down the feeler path
                                break;
                            }
                        }
                    }

                    //If the points in the feeler path are not enough then some paths near the feeler path should be explored
                    //(this would include the case where the target unit is below the minimum range)
                    //
                    //Perhaps if the feeler path fails we can radially check around the range of the ability for a spot that 
                    //can be moved to that the ability can be used from.

                    //if nothing can be found then the action fails.
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public void CalculateWeight()
            {
                if(!Ability.SelectionInfo.UnitTargetParams.CheckUnit(Morsel.Unit, Ability.CastingUnit))
                {
                    Weight = 0;
                    return;
                }

                var relation = Morsel.Team.GetRelation(Ability.CastingUnit.AI.GetTeam());

                Weight = WeightParams.BaseWeight;

                switch (relation)
                {
                    case Relation.Friendly:
                        Weight *= WeightParams.AllyWeight;
                        break;
                    case Relation.Hostile:
                        Weight *= WeightParams.EnemyWeight;
                        break;
                    case Relation.Neutral:
                        Weight *= WeightParams.NeutralWeight;
                        break;
                }

                foreach(var modification in WeightParams.WeightModifications)
                {
                    Weight = modification.Invoke(Weight, Ability, Morsel);
                }
            }
        }

        public override List<IAIAction> GetDesiredTargets()
        {
            List<IAIAction> targets = new List<IAIAction>();

            foreach (var morselKVP in TileMapManager.Scene.CombatState.UnitInformation)
            {
                foreach(var morsel in morselKVP.Value)
                {
                    TemplateAIAction target = new TemplateAIAction(this, morsel.ActionMorsel, WeightParams);

                    if(target.Weight > 1)
                    {
                        targets.Add(target);
                    }
                }
            }

            return targets;
        }
    }
}
