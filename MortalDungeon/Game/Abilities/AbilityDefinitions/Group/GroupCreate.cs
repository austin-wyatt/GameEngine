using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities.AbilityDefinitions
{
    public class GroupCreate : Ability
    {
        public GroupCreate(Unit castingUnit)
        {
            CastingUnit = castingUnit;

            CanTargetGround = false;
            CanTargetSelf = true;
            UnitTargetParams.IsControlled = UnitCheckEnum.True;

            MaxCharges = -1;
            ActionCost = 0;
            EnergyCost = 0;

            if (PlayerParty.Grouped)
            {
                SetIcon(UIControls.StatusOnline, Spritesheets.UIControlsSpritesheet);
            }
            else
            {
                if (PlayerParty.CanGroupUnits())
                {
                    SetIcon(UIControls.StatusAway, Spritesheets.UIControlsSpritesheet);
                }
                else
                {
                    SetIcon(UIControls.StatusOffline, Spritesheets.UIControlsSpritesheet);
                }
            }
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            if (PlayerParty.Grouped)
            {
                PlayerParty.UngroupUnits(PlayerParty.PrimaryUnit.Info.TileMapPosition);
            }
            else
            {
                PlayerParty.GroupUnits(SelectedUnit);
            }

            _selectingUnit = false;
            Casted();
            EffectEnded();
        }

        private bool _selectingUnit = false;
        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            base.OnSelect(scene, currentMap);

            if (PlayerParty.CanGroupUnits())
            {
                _selectingUnit = true;
            }
            else if (PlayerParty.Grouped)
            {
                EnactEffect();
            }
            else
            {
                Scene.DeselectAbility();
            }
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (_selectingUnit && PlayerParty.UnitsInParty.Contains(unit))
            {
                CastingUnit = unit;
                SelectedUnit = unit;
                EnactEffect();
            }

            return false;
        }

    }
}
