using Empyrean.Definitions.TileEffects;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityClasses.Spider
{
    public class CreateWeakWeb : TemplateRangedAOE
    {
        public CreateWeakWeb(Unit unit) : base(unit)
        {
            ((AOETarget)SelectionInfo).TilePattern = new List<Vector3i> { new Vector3i(0, 0, 0), new Vector3i(-1, 1, 0), new Vector3i(1, 0, -1), new Vector3i(1, -1, 0), new Vector3i(-1, 0, 1) };

            AbilityClass = AbilityClass.Spider;

            CastingMethod = unit?.Info.Species == Species.Bug ? CastingMethod.Innate : CastingMethod.Magic;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 2, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
            OneUsePerTurn = false;

            SelectionInfo.CanSelectUnits = false;

            Name = TextEntry.GetTextEntry(13); //13  3
            Description = TextEntry.GetTextEntry(14); //14  3

            AnimationSet = AnimationSetManager.GetAnimationSet(70);
        }

        public override void ApplyPassives()
        {
            base.ApplyPassives();

            CastingUnit.Info.StatusManager.AddUnitCondition(UnitCondition.WebImmuneWeak);
        }

        public override void RemovePassives()
        {
            base.RemovePassives();

            CastingUnit.Info.StatusManager.RemoveUnitCondition(UnitCondition.WebImmuneWeak);
        }

        public override void EnactEffect()
        {
            BeginEffect();

            foreach (var tile in SelectionInfo.SelectedTiles)
            {
                TileEffectManager.AddTileEffectToPoint(new WeakSpiderWeb() { OwnerId = CastingUnit.PermanentId }, tile);
            }

            Casted();
            EffectEnded();
        }
    }
}
