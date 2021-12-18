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
    internal class SpawnSkeleton : Ability
    {
        internal SpawnSkeleton(Unit castingUnit, int range = 3)
        {
            Type = AbilityTypes.Summoning;
            DamageType = DamageType.NonDamaging;
            Range = range;
            CastingUnit = castingUnit;

            CanTargetGround = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            Name = "Spawn Skeleton";

            _description = "Spawn a skeleton after channeling three times.";

            Icon = new Icon(Icon.DefaultIconSize, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet, true);

            Channel first = new Channel(castingUnit, "Spawn Skeleton (1)", _description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);
            Channel second = new Channel(castingUnit, "Spawn Skeleton (2)", _description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);
            Channel third = new Channel(castingUnit, "Spawn Skeleton (3)", _description, Objects.TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet);

            first.AddCombo(second, null, false);
            second.AddCombo(third, first, false);
            third.AddCombo(this, second, false);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            if (CastingUnit.AI.ControlType == ControlType.Controlled && CanTargetGround) 
            {
                validTiles.ForEach(tile =>
                {
                    tile.TilePoint.ParentTileMap.SelectTile(tile);
                });
            }

            return validTiles;
        }

        internal override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (AffectedTiles.Exists(t => t == tile))
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.DeselectTiles();
            }
        }


        internal override void OnCast()
        {
            ClearSelectedTiles();

            base.OnCast();
        }

        internal override void OnAICast()
        {
            base.OnAICast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            //create skeleton unit

            Entity skele = new Entity(EntityParser.ApplyPrefabToUnit(EntityParser.FindPrefab(PrefabType.Unit, "Spawned Skeleton"), Scene));
            skele.Handle.SetTeam(CastingUnit.AI.Team);

            EntityManager.AddEntity(skele);
            skele.DestroyOnUnload = true;

            skele.Load(SelectedTile.ToFeaturePoint());


            Explosion.ExplosionParams parameters = new Explosion.ExplosionParams(Explosion.ExplosionParams.Default)
            {
                Acceleration = new Vector3(),
                MultiplicativeAcceleration = new Vector3(0.95f, 0.95f, 0.5f),
                ParticleCount = 50,
                BaseVelocity = new Vector3(30, 30, 0.03f),
                ColorDelta = new Vector4(0.02f, 0.02f, 0.02f, 0),
                ParticleSize = 0.1f
            };

            var castParticles = new Explosion(SelectedTile.Position + new Vector3(0, 0, 0.4f), new Vector4(0.5f, 0.5f, 0.5f, 1), parameters);
            castParticles.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(castParticles);
            };

            Scene._particleGenerators.Add(castParticles);

            Casted();
            EffectEnded();
        }

        internal override void OnAbilityDeselect()
        {
            ClearSelectedTiles();

            base.OnAbilityDeselect();

            SelectedTile = null;
        }

        internal void ClearSelectedTiles() 
        {
            lock(AffectedTiles)
            AffectedTiles.ForEach(tile =>
            {
                tile.TilePoint.ParentTileMap.DeselectTiles();
            });
        }
    }
}
