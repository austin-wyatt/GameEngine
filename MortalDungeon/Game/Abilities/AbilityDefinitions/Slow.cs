using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Abilities
{
    public class Slow : TemplateRangedSingleTarget
    {
        float _slowMultiplier;
        int _slowDuration;

        public Slow(Unit castingUnit, int range = 1, float slowAmount = 0.25f, int duration = 3) : base(castingUnit)
        {
            Type = AbilityTypes.Debuff;
            Range = range;
            CastingUnit = castingUnit;

            _slowDuration = duration;
            _slowMultiplier = 1 + slowAmount;

            CastingMethod |= CastingMethod.Magic;

            //Name = "Slow";

            CanTargetGround = false;

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.SpiderWeb, Spritesheets.IconSheet, true, Icon.BackgroundType.DebuffBackground);
        }

        public override void EnactEffect()
        {
            BeginEffect();

            //SlowDebuff slowDebuff = new SlowDebuff(SelectedUnit, _slowDuration, _slowMultiplier);

            //SelectedUnit.Info.AddBuff(slowDebuff);

            Casted();
            EffectEnded();
        }
    }
}
