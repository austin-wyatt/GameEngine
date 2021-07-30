using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class Strike : Ability
    {
        public Strike(Unit castingUnit, int range = 1, float damage = 10)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            EnergyCost = 5;

            Name = "Strike";
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, new List<TileClassification> { TileClassification.AttackableTerrain }, units, CastingUnit);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            base.OnSelect(scene, currentMap);

            if (Scene.EnergyDisplayBar.CurrentEnergy >= GetEnergyCost())
            {
                AffectedTiles = GetValidTileTargets(currentMap, scene._units);

                TrimTiles(AffectedTiles, Units);

                AffectedTiles.ForEach(tile =>
                {
                    currentMap.SelectTile(tile);
                });

                Scene.EnergyDisplayBar.HoverAmount(GetEnergyCost());
            }
            else 
            {
                Scene.DeselectAbility();
            }
        }

        public override void OnUnitClicked(Unit unit)
        {
            if (CastingUnit.ObjectID == unit.ObjectID)
            {
                Scene.DeselectAbility();
            }
            else if (unit.Team != CastingUnit.Team) 
            {
                SelectedUnit = unit;
                EnactEffect();
            }
        }

        public override void OnCast()
        {
            base.OnCast();

            Scene.EnergyDisplayBar.HoverAmount(0);

            float energyCost = GetEnergyCost();

             //special cases for energy reduction go here

            Scene.EnergyDisplayBar.AddEnergy(-energyCost);

            TileMap.DeselectTiles();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(GetDamage(), DamageType);

            OnCast();
        }
    }
}
