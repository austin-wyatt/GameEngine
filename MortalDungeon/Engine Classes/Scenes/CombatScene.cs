using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public class CombatScene : Scene
    {
        public int Round = 0;
        public List<Unit> InitiativeOrder = new List<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;

        public Ability _selectedAbility = null;

        public Unit CurrentUnit;

        public bool InCombat = true;

        protected const AbilityTypes DefaultAbilityType = AbilityTypes.Move;

        /// <summary>
        /// Start the next round
        /// </summary>
        public virtual void AdvanceRound()
        {
            Round++;

            StartRound();
        }

        /// <summary>
        /// End the current round and calculate anything that needs to be calculated at that point
        /// </summary>
        public virtual void CompleteRound()
        {
            //do stuff that needs to be done when a round is completed

            AdvanceRound();
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound()
        {
            UnitTakingTurn = 0;

            //do calculations here (advance an event, show a cutscene, etc)

            StartTurn();
        }

        /// <summary>
        /// Start the turn for the unit that is currently up in the initiative order
        /// </summary>
        public virtual void StartTurn()
        {
            //change the UI, move the camera, show which unit is selected, etc
        }

        /// <summary>
        /// Complete the current unit's turn and start the next unit's turn
        /// </summary>
        public virtual void CompleteTurn()
        {
            UnitTakingTurn++;

            if (UnitTakingTurn == InitiativeOrder.Count)
            {
                CompleteRound();
                return;
            }

            StartTurn(); //Advance to the next unit's turn
        }

        public virtual void SelectAbility(Ability ability)
        {
            _selectedAbility = ability;
            ability.OnSelect(this, _tileMaps[0]);
        }

        public virtual void DeselectAbility()
        {
            _selectedAbility.TileMap.DeselectTiles();

            _selectedAbility?.OnAbilityDeselect();
            _selectedAbility = null;
        }

        public override void EvaluateTileMapHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            _tileMaps.ForEach(map =>
            {
                map.EndHover();

                ObjectCursorBoundsCheck(map.Tiles, mouseRayNear, mouseRayFar, (tile) =>
                {
                    if (tile.Hoverable)
                    {
                        map.HoverTile(tile);
                        if (_selectedAbility != null && _selectedAbility.HasHoverEffect)
                        {
                            _selectedAbility.OnHover(tile, map);
                        }
                    }
                });
            });
        }

        public override void HandleRightClick()
        {
            base.HandleRightClick();

            if (_selectedAbility != null)
            {
                _selectedAbility.OnRightClick();
            }
        }


    }
}
