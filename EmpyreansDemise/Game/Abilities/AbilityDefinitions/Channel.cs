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
using Empyrean.Game.Particles;

namespace Empyrean.Game.Abilities
{
    public class Channel : TemplateRangedSingleTarget
    {
        public Channel(Unit castingUnit, string name, string description, Enum icon = null, Spritesheet spritesheet = null) : base(castingUnit)
        {
            CastingUnit = castingUnit;

            SelectionInfo.CanSelectTiles = false;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.True;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = name;
            //Description = description;

            var iconPos = icon == null ? IconSheetIcons.Channel : icon;
            Spritesheet iconSpritesheet = spritesheet == null ? Spritesheets.IconSheet : spritesheet;

            //Icon = new Icon(Icon.DefaultIconSize, iconPos, iconSpritesheet, true);
        }

        public override void OnCast()
        {
            TileMap.Controller.DeselectTiles();

            base.OnCast();
        }

        public override void EnactEffect()
        {
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
