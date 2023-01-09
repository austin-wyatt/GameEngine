using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public enum SelectionInfoContext
    {
        LineRequiredToTarget
    }

    public class SelectionInfo
    {
        public Ability Ability;

        public CombatScene Scene { get => TileMapManager.Scene; }

        public virtual List<Unit> SelectedUnits { get; set; } = new List<Unit>();
        public virtual List<Tile> SelectedTiles { get; set; } = new List<Tile>();

        public Unit SelectedUnit => SelectedUnits[0];

        public virtual HashSet<Unit> TargetedUnits { get; set; } = new HashSet<Unit>();
        public virtual HashSet<Tile> TargetedTiles { get; set; } = new HashSet<Tile>();

        /// <summary>
        /// Represents tiles that are being used by the selection process but are neither targeted nor selected
        /// </summary>
        public virtual List<Tile> TileBuffer { get; set; } = new List<Tile>();


        public virtual bool CanSelectUnits { get; set; } = true;
        public virtual bool CanSelectTiles { get; set; } = false;

        public virtual Tile SourceTile { get; set; }

        /// <summary>
        /// Determines whether the ability will be enacted when the selection completes or if the 
        /// ConditionsMet event will be fired.
        /// </summary>
        public virtual bool UseAbility { get; set; } = true;

        public virtual bool CreateVisuals { get; set; } = true;

        public event Action ConditionsMet;

        /// <summary>
        /// Use to initialize special case data in cases where the SelectionInfo instance is not being
        /// used normally. (Ex, in MultiSelectionType when data from the previous SelectionInfo instance
        /// needs to be used in the current SelectionInfo instance)
        /// </summary>
        public event Action Selected;
        public event Action Deselected;

        public ContextManager<SelectionInfoContext> Context = new ContextManager<SelectionInfoContext>();

        public UnitSearchParams UnitTargetParams = new UnitSearchParams()
        {
            Dead = UnitCheckEnum.False,
            IsFriendly = UnitCheckEnum.SoftTrue,
            IsHostile = UnitCheckEnum.SoftTrue,
            IsNeutral = UnitCheckEnum.SoftTrue,
            Self = UnitCheckEnum.False
        };

        public SelectionInfo(Ability ability)
        {
            Ability = ability;
        }


        public virtual void SelectAbility()
        {
            SourceTile = Ability.CastingUnit.Info.TileMapPosition;

            Selected?.Invoke();

            FindTargets();

            if (CreateVisuals)
            {
                CreateVisualIndicators();
            }
        }

        public virtual void SelectAbilityAI()
        {
            SourceTile = Ability.CastingUnit.Info.TileMapPosition;

            Selected?.Invoke();
        }

        public virtual void DeselectAbility()
        {
            Deselected?.Invoke();

            SourceTile = null;

            if (CreateVisuals)
            {
                RemoveVisualIndicators();
            }
            
            ClearTargets();
        }

        public virtual void DeselectAbilityAI()
        {
            Deselected?.Invoke();

            SourceTile = null;

            ClearTargets();
        }

        public virtual bool UnitClicked(Unit clickedUnit)
        {
            if (CanSelectUnits && TargetedUnits.Contains(clickedUnit))
            {
                if (SelectedUnits.Contains(clickedUnit))
                {
                    SelectedUnits.Remove(clickedUnit);
                }
                else
                {
                    SelectedUnits.Add(clickedUnit);
                }
                
                CheckSelectionStatus();
                return true;
            }

            if(CanSelectTiles && TargetedTiles.Contains(clickedUnit.Info.TileMapPosition) || TileBuffer.Contains(clickedUnit.Info.TileMapPosition))
            {
                TileClicked(clickedUnit.Info.TileMapPosition);
            }

            return false;
        }

        public virtual bool TileClicked(Tile clickedTile)
        {
            if (!CanSelectTiles)
                return false;

            if (TargetedTiles.Contains(clickedTile))
            {
                if (SelectedTiles.Contains(clickedTile))
                {
                    SelectedTiles.Remove(clickedTile);
                }
                else
                {
                    SelectedTiles.Add(clickedTile);
                }

                CheckSelectionStatus();

                return true;
            }

            return false;
        }

        public virtual void TileHovered(Tile tile)
        {

        }

        /// <summary>
        /// Determine if all of the required targets have been selected before enacting the ability effect
        /// </summary>
        protected virtual void CheckSelectionStatus()
        {

        }

        public virtual void FindTargets()
        {
            
        }

        private void ClearTargets()
        {
            SelectedUnits.Clear();
            SelectedTiles.Clear();

            TargetedTiles.Clear();
            TargetedUnits.Clear();
        }

        public virtual void CreateVisualIndicators()
        {

        }

        public virtual void RemoveVisualIndicators()
        {

        }

        public virtual void OnRightClick()
        {
            Scene.DeselectAbility();
        }

        protected void OnConditionsMet()
        {
            ConditionsMet?.Invoke();
        }

        public void ClearConditionsEvent()
        {
            ConditionsMet = null;
        }
    }
}
