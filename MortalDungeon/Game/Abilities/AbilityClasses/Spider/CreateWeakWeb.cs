using MortalDungeon.Definitions.TileEffects;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities.AbilityClasses.Spider
{
    public class CreateWeakWeb : TemplateRangedAOE
    {
        public CreateWeakWeb(Unit unit) : base(unit)
        {
            TilePattern = new List<Vector3i> { new Vector3i(0, 0, 0), new Vector3i(-1, 1, 0), new Vector3i(1, 0, -1), new Vector3i(1, -1, 0), new Vector3i(-1, 0, 1) };

            AbilityClass = AbilityClass.Spider;

            CastingMethod = unit?.Info.Species == Species.Bug ? CastingMethod.Innate : CastingMethod.Magic;

            ActionCost = 2;
            OneUsePerTurn = false;

            Name = new TextInfo(13, 3);
            Description = new TextInfo(14, 3);

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
            base.EnactEffect();

            foreach (var tile in _hoveredTiles)
            {
                TileEffectManager.AddTileEffectToPoint(new WeakSpiderWeb(), tile);
            }

            Casted();
            EffectEnded();
        }
    }
}
