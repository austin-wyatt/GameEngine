using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public class TargetInformation
    {
        /// <summary>
        /// The indices of the targets in their selection lists. 
        /// Must be used in conjunction with the SelectedUnits AbilityUnitTarget.
        /// </summary>
        public List<int> TargetIndices = new List<int>();
        public AbilityUnitTarget Target;

        public TargetInformation(AbilityUnitTarget target)
        {
            Target = target;
        }

        public List<Unit> GetTargets(Ability ability)
        {
            List<Unit> returnList = new List<Unit>();
            switch (Target)
            {
                case AbilityUnitTarget.SelectedUnit:
                    returnList.Add(ability.SelectionInfo.SelectedUnits[0]);
                    break;
                case AbilityUnitTarget.CastingUnit:
                    returnList.Add(ability.CastingUnit);
                    break;
                case AbilityUnitTarget.SelectedUnits:
                    if(TargetIndices.Count > 0)
                    {
                        foreach (var index in TargetIndices)
                        {
                            returnList.Add(ability.SelectionInfo.SelectedUnits[index]);
                        }
                    }
                    else 
                    {
                        foreach (var unit in ability.SelectionInfo.SelectedUnits)
                        {
                            returnList.Add(unit);
                        }
                    }
                    break;
            }

            return returnList;
        }
    }
}
