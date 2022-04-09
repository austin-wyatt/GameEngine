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
            UnitTargetParams.Self = UnitCheckEnum.False;

            MaxCharges = -1;
            ActionCost = 0;
            EnergyCost = 0;

            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)UIControls.ArrowIn },
                Spritesheet = (int)TextureName.UIControlsSpritesheet
            });
        }

        protected Tile _selectedTile = null;
        public override void OnTileClicked(TileMap map, Tile tile)
        {
            _selectedTile = tile;
            EnactEffect();
        }

        public override void EnactEffect()
        {
            BeginEffect();

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(_selectedTile, 4)
            {
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
