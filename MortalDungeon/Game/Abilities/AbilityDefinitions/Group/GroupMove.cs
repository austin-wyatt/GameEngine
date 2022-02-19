using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities.AbilityDefinitions
{
    public class GroupMove : Ability
    {
        public GroupMove(Unit castingUnit)
        {
            CastingUnit = castingUnit;

            CanTargetGround = true;
            CanTargetSelf = false;

            MaxCharges = -1;
            ActionCost = 0;
            EnergyCost = 0;

            SetIcon(UIControls.ArrowIn, Spritesheets.UIControlsSpritesheet);
        }

        protected BaseTile _selectedTile = null;
        public override void OnTileClicked(TileMap map, BaseTile tile)
        {
            _selectedTile = tile;
            EnactEffect();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(_selectedTile, 4)
            {
                TraversableTypes = CastingUnit.Info._movementAbility.TraversableTypes,
                Units = Scene._units,
                CastingUnit = CastingUnit,
                AbilityType = Type,
                CheckTileLower = true
            };

            var validTiles = _selectedTile.TileMap.FindValidTilesInRadius(param);
            int currTile = 0;

            List<Unit> unitsToMove = new List<Unit>();

            foreach (var unit in Scene._selectedUnits)
            {
                if (unit.AI.ControlType == ControlType.Controlled)
                {
                    if (currTile >= validTiles.Count)
                    {
                        return;
                    }

                    if (unit.Info._movementAbility.CheckPathToTile(validTiles[currTile]))
                    {
                        unitsToMove.Add(unit);
                    }

                    currTile++;
                }
            }

            foreach(var unit in unitsToMove)
            {
                unit.Info._movementAbility.EnactEffect();
            }

            Casted();
        }

        public override bool OnUnitClicked(Unit unit)
        {
            Scene.DeselectAbility();
            return false;
        }
    }
}
