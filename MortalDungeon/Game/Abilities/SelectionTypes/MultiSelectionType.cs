using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.SelectionTypes
{

    /// <summary>
    /// Allows multiple selection info instances to be chained together for more advanced handling. <para/>
    /// 
    /// When switching between selection info instances, any data initialization should be done in 
    /// the upcoming selection info's Selected event.
    /// </summary>
    public class MultiSelectionType : SelectionInfo
    {
        public override List<Tile> SelectedTiles { get => CurrentInfo.SelectedTiles; }
        public override List<Unit> SelectedUnits { get => CurrentInfo.SelectedUnits; }

        public override HashSet<Tile> TargetedTiles { get => CurrentInfo.TargetedTiles; }
        public override HashSet<Unit> TargetedUnits { get => CurrentInfo.TargetedUnits; }

        public override bool CanSelectTiles { get => CurrentInfo.CanSelectTiles; }
        public override bool CanSelectUnits { get => CurrentInfo.CanSelectUnits; }

        public override bool CreateVisuals { get => CurrentInfo.CreateVisuals; }
        public override Tile SourceTile { get => CurrentInfo.SourceTile; }
        public override List<Tile> TileBuffer { get => CurrentInfo.TileBuffer; }

        public override bool UseAbility { get => CurrentInfo.UseAbility; }



        private List<SelectionInfo> SelectionTypes = new List<SelectionInfo>();
        public int CurrentSelection = 0;

        private SelectionInfo CurrentInfo => SelectionTypes[CurrentSelection];

        public MultiSelectionType(Ability ability) : base(ability) { }

        public override void DeselectAbility()
        {
            SelectionTypes[CurrentSelection].DeselectAbility();
            CurrentSelection = 0;
        }

        public override void OnRightClick()
        {
            SelectionTypes[CurrentSelection].OnRightClick();
        }

        public override void SelectAbility()
        {
            CurrentSelection = 0;
            SelectionTypes[CurrentSelection].SelectAbility();
        }

        public override bool TileClicked(Tile clickedTile)
        {
            return SelectionTypes[CurrentSelection].TileClicked(clickedTile);
        }

        public override void TileHovered(Tile tile)
        {
            SelectionTypes[CurrentSelection].TileHovered(tile);
        }

        public override bool UnitClicked(Unit clickedUnit)
        {
            return SelectionTypes[CurrentSelection].UnitClicked(clickedUnit);
        }


        private List<Action> _selectionAdvancementActions = new List<Action>();
        public void AddChainedSelectionInfo(SelectionInfo info)
        {
            SelectionTypes.Add(info);
            
            for(int i = 0; i < SelectionTypes.Count - 1; i++)
            {
                int capturedIndex = i;

                SelectionTypes[i].UseAbility = false;
                SelectionTypes[i].ClearConditionsEvent();

                Action action = () =>
                {
                    SelectionTypes[capturedIndex + 1].SelectAbility();
                    SelectionTypes[capturedIndex].DeselectAbility();
                    CurrentSelection = capturedIndex + 1;

                    //CanSelectTiles = SelectionTypes[capturedIndex + 1].CanSelectTiles;
                    //CanSelectUnits = SelectionTypes[capturedIndex + 1].CanSelectUnits;
                    //SelectedUnits = SelectionTypes[capturedIndex].SelectedUnits;
                    //SelectedTiles = SelectionTypes[capturedIndex].SelectedTiles;
                    //TileBuffer = SelectionTypes[capturedIndex].TileBuffer;
                    //TargetedTiles = SelectionTypes[capturedIndex].TargetedTiles;
                    //TargetedUnits = SelectionTypes[capturedIndex].TargetedUnits;
                };

                if(_selectionAdvancementActions.Count > i)
                {
                    SelectionTypes[i].ConditionsMet -= _selectionAdvancementActions[i];
                    _selectionAdvancementActions[i] = action;
                }
                else
                {
                    _selectionAdvancementActions.Add(action);
                }
                
                SelectionTypes[i].ConditionsMet += action;
            }

            SelectionTypes[^1].UseAbility = true;
        }
    }
}
