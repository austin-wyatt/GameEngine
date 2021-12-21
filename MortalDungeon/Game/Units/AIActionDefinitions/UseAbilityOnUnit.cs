using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class UseAbilityOnUnit : UnitAIAction
    {
        public UseAbilityOnUnit(Unit castingUnit, AIAction actionType, Ability ability, BaseTile tile = null, Unit unit = null) : base(castingUnit, actionType, ability, tile, unit) { }

        public override void EnactEffect()
        {
            Ability.EffectEndedAction = () =>
            {
                CastingUnit.AI.BeginNextAction();
                Ability.EffectEndedAction = null;
            };

            if (TargetedTile != null)
            {
                Ability.OnTileClicked(Tile.TileMap, Tile);
            }
            else if (TargetedUnit != null) 
            {
                Ability.SelectedUnit = TargetedUnit;

                if (!Ability.UnitInRange(TargetedUnit)) 
                {
                    Console.WriteLine("How did this happen?");
                }

                Vector2i clusterPos = Scene._tileMapController.PointToClusterPosition(Ability.SelectedUnit.Info.TileMapPosition);

                if (VisionMap.InVision(clusterPos.X, clusterPos.Y, UnitTeam.PlayerUnits)) 
                {
                    Scene.SmoothPanCameraToUnit(Ability.SelectedUnit, 1);
                }

                Ability.EnactEffect();
            }
        }
    }
}
