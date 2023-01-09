using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using OpenTK.Mathematics;
using Empyrean.Game.Map;
using System.Diagnostics;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Game.Items;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Movement;

namespace Empyrean.Game.Abilities
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

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity;

            SelectionInfo.UnitTargetParams.Dead = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            SelectionInfo.Context.SetFlag(SelectionInfoContext.LineRequiredToTarget, true);

            CastRequirements.EquipmentRequirement.RequiredTag |= ItemTag.Weapon_Melee;


            Name = new Serializers.TextInfo(11, 3);
            Description = new Serializers.TextInfo(12, 3);


            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)IconSheetIcons.Circle },
                Spritesheet = (int)TextureName.IconSpritesheet
            });

            HasHoverEffect = true;
            MultiSelectionType multi = new MultiSelectionType(this);
            SelectionInfo = multi;

            SingleTarget singleTarget = new SingleTarget(this);
            singleTarget.UnitTargetParams = new UnitSearchParams()
            {
                Dead = UnitCheckEnum.False,
                IsFriendly = UnitCheckEnum.SoftTrue,
                IsHostile = UnitCheckEnum.SoftTrue,
                IsNeutral = UnitCheckEnum.SoftTrue,
                Self = UnitCheckEnum.False,
                InVision = UnitCheckEnum.True,
            };

            multi.AddChainedSelectionInfo(singleTarget);

            RadialSelection radial = new RadialSelection(this);

            radial.Selected += () =>
            {
                radial.SourceTile = singleTarget.SelectedUnits[0].Info.TileMapPosition;
                radial.SelectedUnits.Add(singleTarget.SelectedUnits[0]);

                radial.Direction = GMath.AngleOfPoints(radial.SourceTile._position, CastingUnit.Info.TileMapPosition._position);
            };

            multi.AddChainedSelectionInfo(radial);


            TargetInformation targetInfo = new TargetInformation(AbilityUnitTarget.SelectedUnit);
            ApplyDamage applyDamageEffect = new ApplyDamage(targetInfo);
            applyDamageEffect.CreateDamageInstance = () =>
            {
                DamageInstance damageInstance = new DamageInstance();
                //damageInstance.Damage.Add(DamageType.HealthRemoval, 1000);
                damageInstance.Damage.Add(DamageType.HealthRemoval, 1);

                return damageInstance;
            };

            EffectManager.Effects.Add(applyDamageEffect);

            MoveEffect forcedMoveEffect = new MoveEffect(targetInfo);

            forcedMoveEffect.EffectEnacted += () =>
            {
                radial.RemoveVisualIndicators();
            };

            forcedMoveEffect.GetMoveContract = () =>
            {
                return MovementHelper.CalculateForcedMovement(radial.SourceTile, radial.CurrAngle, radial.CurrMagnitude);
            };

            EffectManager.Effects.Add(forcedMoveEffect);
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            //BeginEffect();

            //Task.Run(() =>
            //{
            //    var dam = new Dictionary<DamageType, float>();
            //    dam.Add(DamageType.HealthRemoval, 1000);

            //    SelectionInfo.SelectedUnit.ApplyDamage(new DamageParams(new DamageInstance()
            //    {
            //        Damage = dam,
            //    }, ability: this));

            //    Casted();
            //    EffectEnded();
            //});
        }

    }
}
