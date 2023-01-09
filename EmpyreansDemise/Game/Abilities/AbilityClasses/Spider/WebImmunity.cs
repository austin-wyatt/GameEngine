using Empyrean.Definitions.TileEffects;
using Empyrean.Engine_Classes;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityClasses.Spider
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
