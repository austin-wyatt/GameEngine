using Empyrean.Game.Combat;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AIImplementations
{
    public class AITargetSelection
    {
        //a method that chooses targets and creates AIActions for them

        //a method that can be overridden in the ability definition defining how the ability should be used from a given AIAction
        //(ie which units are targeted, what extra data needs to be passed to/from each selection info step, etc)

        public Func<InformationMorsel, float> EvaluateSimpleWeight = null;
        public Func<InformationMorsel, float> EvaluateFullWeight = null;


        public Func<InformationMorsel, Func<Task<bool>>> GenerateAction = null;
        public Func<AvailableMovePaths, Func<bool>> GenerateFeasibilityCheck = null;

        public virtual List<AIAction> GetPotentialActions(Func<InformationMorsel, AvailableMovePaths> getPathsForMorsel)
        {
            List<AIAction> actions = new List<AIAction>();

            if (EvaluateSimpleWeight == null || GenerateFeasibilityCheck == null || GenerateAction == null)
                return actions;

            foreach (var morselKVP in TileMapManager.Scene.CombatState.UnitInformation)
            {
                foreach (var morsel in morselKVP.Value)
                {
                    float weight = EvaluateSimpleWeight.Invoke(morsel.ActionMorsel);

                    if(weight > 1)
                    {
                        AIAction unitAction = new AIAction()
                        {
                            Weight = weight,
                            DoAction = GenerateAction.Invoke(morsel.ActionMorsel),
                            FeasibilityCheck = GenerateFeasibilityCheck.Invoke(getPathsForMorsel.Invoke(morsel.ActionMorsel))
                        };

                        if (EvaluateFullWeight != null)
                        {

                        }

                        actions.Add(unitAction);
                    }
                }
            }

            return actions;
        }
    }
}
