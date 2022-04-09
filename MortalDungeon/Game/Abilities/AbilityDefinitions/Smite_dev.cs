using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Game.Map;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
{
    public class Smite_dev : TemplateRangedSingleTarget
    {
        public Smite_dev(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.RangedAttack;
            DamageType = DamageType.Piercing;
            Range = 15;
            MinRange = 0;
            CastingUnit = castingUnit;
            Damage = 10;

            ActionCost = 0;

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            RequiresLineToTarget = true;


            Name = new Serializers.TextInfo(11, 3);
            Description = new Serializers.TextInfo(12, 3);


            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)IconSheetIcons.Circle },
                Spritesheet = (int)TextureName.IconSpritesheet
            });
        }

        public override void EnactEffect()
        {
            BeginEffect();

            var dam = new Dictionary<DamageType, float>();
            dam.Add(DamageType.HealthRemoval, 1000);

            SelectedUnit.ApplyDamage(new DamageParams(new DamageInstance()
            {
                Damage = dam,
            }, ability: this));

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, Damage);

            return instance;
        }
    }
}
