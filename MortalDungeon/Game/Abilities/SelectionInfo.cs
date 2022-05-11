using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public enum SelectionType
    {
        UseOnValidTargetSelected,
    }

    public enum SelectionInfoContext
    {
        LineRequiredToTarget
    }

    public class SelectionInfo
    {
        public Ability Ability;

        public CombatScene Scene { get => TileMapManager.Scene; }

        public List<Unit> SelectedUnits = new List<Unit>();
        public List<Tile> SelectedTiles = new List<Tile>();

        public Unit SelectedUnit => SelectedUnits[0];

        public HashSet<Unit> TargetedUnits = new HashSet<Unit>();
        public HashSet<Tile> TargetedTiles = new HashSet<Tile>();

        /// <summary>
        /// Represents tiles that are being used by the selection process but are neither targeted nor selected
        /// </summary>
        public List<Tile> TileBuffer = new List<Tile>();

        public SelectionType TargetSelectionType;

        public bool CanSelectUnits = true;
        public bool CanSelectTiles = false;

        public Tile SourceTile = null;

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

        public void SelectAbility()
        {
            SourceTile = Ability.CastingUnit.Info.TileMapPosition;

            FindTargets();
            CreateVisualIndicators();
        }

        public void DeselectAbility()
        {
            SourceTile = null;

            RemoveVisualIndicators();
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

        protected virtual void FindTargets()
        {
            
        }

        private void ClearTargets()
        {
            SelectedUnits.Clear();
            SelectedTiles.Clear();

            TargetedTiles.Clear();
            TargetedUnits.Clear();
        }

        protected virtual void CreateVisualIndicators()
        {

        }

        protected virtual void RemoveVisualIndicators()
        {

        }

        public virtual void OnRightClick()
        {
            Scene.DeselectAbility();
        }
    }
}
