using MortalDungeon.Definitions.TileEffects;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities.AbilityClasses.Spider
{
    public class WebImmunity : Ability
    {
        public WebImmunity(Unit unit) : base(unit)
        {
            Type = AbilityTypes.Passive;

            AnimationSet = new AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.W },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            Name = new TextInfo(15, 3);
            Description = new TextInfo(16, 3);

            AbilityClass = AbilityClass.Spider;
        }

        public override void ApplyPassives()
        {
            base.ApplyPassives();

            CastingUnit.Info.StatusManager.AddUnitCondition(UnitCondition.WebImmuneWeak);
            CastingUnit.Info.StatusManager.AddUnitCondition(UnitCondition.WebImmuneMed);
            CastingUnit.Info.StatusManager.AddUnitCondition(UnitCondition.WebImmuneStrong);
        }

        public override void RemovePassives()
        {
            base.RemovePassives();

            CastingUnit.Info.StatusManager.RemoveUnitCondition(UnitCondition.WebImmuneWeak);
            CastingUnit.Info.StatusManager.RemoveUnitCondition(UnitCondition.WebImmuneMed);
            CastingUnit.Info.StatusManager.RemoveUnitCondition(UnitCondition.WebImmuneStrong);
        }
    }
}
