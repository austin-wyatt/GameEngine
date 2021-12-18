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
using MortalDungeon.Game.Particles;

namespace MortalDungeon.Game.Abilities
{
    internal class Channel : Ability
    {
        internal Channel(Unit castingUnit, string name, string description, Enum icon = null, Spritesheet spritesheet = null)
        {
            CastingUnit = castingUnit;

            CanTargetGround = false;
            CanTargetSelf = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            Name = name;
            _description = description;

            var iconPos = icon == null ? IconSheetIcons.Channel : icon;
            Spritesheet iconSpritesheet = spritesheet == null ? Spritesheets.IconSheet : spritesheet;

            Icon = new Icon(Icon.DefaultIconSize, iconPos, iconSpritesheet, true);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            List<BaseTile> validTiles = new List<BaseTile> { CastingUnit.Info.TileMapPosition };

            AffectedUnits.Add(CastingUnit);

            TargetAffectedUnits();

            return validTiles;
        }

        internal override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }


        internal override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            Explosion.ExplosionParams parameters = new Explosion.ExplosionParams(Explosion.ExplosionParams.Default)
            {
                Acceleration = new Vector3(),
                MultiplicativeAcceleration = new Vector3(0.95f, 0.95f, 0.5f),
                ParticleCount = 200,
                BaseVelocity = new Vector3(30, 30, 0.03f),
                ColorDelta = new Vector4(0.02f, 0.02f, 0.02f, 0),
                ParticleSize = 0.1f
            };

            var castParticles = new Explosion(CastingUnit.Position + new Vector3(0, 0, 0.2f), new Vector4(0.3f, 0.87f, 0.81f, 1), parameters);
            castParticles.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(castParticles);
            };

            Scene._particleGenerators.Add(castParticles);

            Casted();
            EffectEnded();
        }
    }
}
