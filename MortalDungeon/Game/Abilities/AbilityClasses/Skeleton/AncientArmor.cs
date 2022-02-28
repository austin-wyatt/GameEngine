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
    public class AncientArmor : Ability
    {
        private int ShieldsGained = 1;
        public AncientArmor(Unit castingUnit)
        {
            Type = AbilityTypes.BuffDefensive;
            CastingUnit = castingUnit;

            Grade = 2;

            ActionCost = 1;
            ChargeRechargeCost = 30;

            MaxCharges = 2;
            Charges = 2;

            CanTargetSelf = true;
            CanTargetGround = false;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            AbilityClass = AbilityClass.Skeleton;

            Name = new Serializers.TextInfo(1, 3);
            Description = new Serializers.TextInfo(2, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.A },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            List<BaseTile> validTiles = new List<BaseTile> { CastingUnit.Info.TileMapPosition };

            AffectedUnits.Add(CastingUnit);

            TargetAffectedUnits();

            return validTiles;
        }


        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedUnits.Contains(unit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            Sound sound = new Sound(Sounds.Select) { Gain = 0.5f, Pitch = GlobalRandom.NextFloat(0.75f, 0.8f) };
            sound.Play();

            Casted();

            if (CastingUnit.Info.CurrentShields > 0)
            {
                CastingUnit.SetShields(CastingUnit.Info.CurrentShields + ShieldsGained);
            }
            else 
            {
                CastingUnit.SetShields(ShieldsGained);
            }

            EffectEnded();
        }
    }
}
