using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using MortalDungeon.Game.Particles;
using OpenTK.Mathematics;
using MortalDungeon.Game.Entities;

namespace MortalDungeon.Game.Abilities
{
    public class SpawnSkeleton : GenericSelectGround
    {
        public SpawnSkeleton(Unit castingUnit, int range = 3) : base(castingUnit, range)
        {
            Type = AbilityTypes.Summoning;
            DamageType = DamageType.NonDamaging;
            Range = range;
            CastingUnit = castingUnit;

            CastingMethod |= CastingMethod.Magic | CastingMethod.Intelligence | CastingMethod.Vocal;

            CanTargetGround = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "Spawn Skeleton";

            //Description = "Spawn a skeleton after channeling three times.";

            //Icon = new Icon(Icon.DefaultIconSize, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet, true);

            //Channel first = new Channel(castingUnit, "Spawn Skeleton (1)", Description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);
            //Channel second = new Channel(castingUnit, "Spawn Skeleton (2)", Description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);
            //Channel third = new Channel(castingUnit, "Spawn Skeleton (3)", Description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);

            //    first.AddCombo(second, null, false);
            //    second.AddCombo(third, first, false);
            //    third.AddCombo(this, second, false);

            //    first.CastingMethod = CastingMethod;
            //    second.CastingMethod = CastingMethod;
            //    third.CastingMethod = CastingMethod;
        }
       
        public override void EnactEffect()
        {
            base.EnactEffect();

            //create skeleton unit

            //Entity skele = new Entity(EntityParser.ApplyPrefabToUnit(EntityParser.FindPrefab(PrefabType.Unit, "Spawned Skeleton"), Scene));
            //skele.Handle.SetTeam(CastingUnit.AI.Team);

            //EntityManager.AddEntity(skele);
            //skele.DestroyOnUnload = true;

            //EntityManager.LoadEntity(skele, SelectedTile.ToFeaturePoint());


            //Explosion.ExplosionParams parameters = new Explosion.ExplosionParams(Explosion.ExplosionParams.Default)
            //{
            //    Acceleration = new Vector3(),
            //    MultiplicativeAcceleration = new Vector3(0.95f, 0.95f, 0.5f),
            //    ParticleCount = 50,
            //    BaseVelocity = new Vector3(30, 30, 0.03f),
            //    ColorDelta = new Vector4(0.02f, 0.02f, 0.02f, 0),
            //    ParticleSize = 0.1f
            //};

            //var castParticles = new Explosion(SelectedTile.Position + new Vector3(0, 0, 0.4f), new Vector4(0.5f, 0.5f, 0.5f, 1), parameters);
            //castParticles.OnFinish = () =>
            //{
            //    Scene._particleGenerators.Remove(castParticles);
            //};

            //Scene._particleGenerators.Add(castParticles);

            Casted();
            EffectEnded();
        }
    }
}
