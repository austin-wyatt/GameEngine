using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Player;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityDefinitions
{
    public class GroupMove : Ability
    {
        UnitGroup SelectedGroup;

        public GroupMove(UnitGroup unitGroup)
        {
            CastingUnit = unitGroup.Leader;

            SelectedGroup = unitGroup;

            SelectionInfo = new SelectionInfo(this);

            SelectionInfo.CanSelectTiles = true;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.False;

            MaxCharges = -1;
            
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)UIControls.ArrowIn },
                Spritesheet = (int)TextureName.UIControlsSpritesheet
            });

            Name = new Serializers.TextInfo(21, 3);
            Description = new Serializers.TextInfo(22, 3);
        }

        protected Tile _selectedTile = null;
        public override void OnTileClicked(TileMap map, Tile tile)
        {
            _selectedTile = tile;
            EnactEffect();
        }

        public override void EnactEffect()
        {
            if (SelectedGroup == null) return;
            
            BeginEffect();


            SelectedGroup.Leader.Info._movementAbility.EvaluateHoverPath(_selectedTile, _selectedTile.TileMap, ignoreRange: true, highlightTiles: false);

            List<(Unit unitToMove, bool useMove, Tile destination)> unitsToMove = new List<(Unit, bool, Tile)>();

            Unit unit;
            if (SelectedGroup.Leader.Info._movementAbility._path.Count > 0)
            {
                for (int i = 1; i < SelectedGroup.Units.Count; i++)
                {
                    unit = SelectedGroup.Units[i];

                    if (SelectedGroup.Leader.Info._movementAbility._path.Count >= i + 1)
                    {
                        Tile tile = SelectedGroup.Leader.Info._movementAbility._path[0];

                        //First path to leader's current destination
                        unit.Info._movementAbility.EvaluateHoverPath(tile, tile.TileMap, ignoreRange: true, highlightTiles: false, allowEndInUnit: true);

                        bool useMove = unit.Info._movementAbility._path.Count > 0;

                        //Then copy the leader's path to the correct ending position
                        if (useMove)
                        {
                            for (int j = 1; j < SelectedGroup.Leader.Info._movementAbility._path.Count - i; j++)
                            {
                                unit.Info._movementAbility._path.Add(SelectedGroup.Leader.Info._movementAbility._path[j]);
                            }
                        }

                        //If there is no path to the leader then we can teleport to the correct ending point
                        unitsToMove.Add((unit, useMove, SelectedGroup.Leader.Info._movementAbility._path[^(i + 1)]));
                    }
                }

                SelectedGroup.Leader.Info._movementAbility.EnactEffect();

                for (int i = 0; i < unitsToMove.Count; i++)
                {
                    if (unitsToMove[i].useMove)
                    {
                        unitsToMove[i].unitToMove.Info._movementAbility.EnactEffect();
                    }
                    else
                    {
                        int capturedIndex = i;

                        void teleportAction()
                        {
                            //add an animation here?
                            unitsToMove[capturedIndex].unitToMove.SetPositionOffset(unitsToMove[capturedIndex].destination._position);
                            unitsToMove[capturedIndex].unitToMove.SetTileMapPosition(unitsToMove[capturedIndex].destination);

                            SelectedGroup.Leader.Info._movementAbility.EffectEndedAction -= teleportAction;
                        };

                        SelectedGroup.Leader.Info._movementAbility.EffectEndedAction += teleportAction;
                    }
                }
            }
            else
            {
                EffectEnded();
            }

            Casted();
        }

        public override bool OnUnitClicked(Unit unit)
        {
            Scene.DeselectAbility();
            return false;
        }


        public override Tooltip GenerateTooltip()
        {
            string body = Description.ToString();

            Tooltip tooltip = UIHelpers.GenerateTooltipWithHeader(Name.ToString(), body);

            return tooltip;
        }
    }
}
