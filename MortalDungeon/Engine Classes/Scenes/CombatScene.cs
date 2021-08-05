using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MortalDungeon.Engine_Classes.Scenes
{
    enum GeneralContextFlags
    {
        TooltipOpen
    }
    public class CombatScene : Scene
    {
        public int Round = 0;
        public List<Unit> InitiativeOrder = new List<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;
        public GameFooter Footer;
        

        public Ability _selectedAbility = null;
        public List<Unit> _selectedUnits = new List<Unit>();

        public Unit CurrentUnit;

        public bool InCombat = true;

        public bool DisplayUnitStatuses = true;

        ContextManager<GeneralContextFlags> ContextManager = new ContextManager<GeneralContextFlags>();

        protected const AbilityTypes DefaultAbilityType = AbilityTypes.Move;

        Texture _normalFogTexture;
        public UIBlock _tooltipBlock;

        public CombatScene() 
        {
            Texture fogTex = Texture.LoadFromFile("Resources/FogTexture.png", default, TextureName.FogTexture);

            fogTex.Use(TextureUnit.Texture1);

            _normalFogTexture = fogTex;
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            _tooltipBlock = new UIBlock(new Vector3());
            _tooltipBlock.MultiTextureData.MixTexture = false;
            _tooltipBlock.SetColor(Colors.Transparent);
            _tooltipBlock.SetAllInline(0);

            AddUI(_tooltipBlock, 999999999);
        }

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
            InitiativeOrder = InitiativeOrder.OrderBy(i => i._movementAbility.GetEnergyCost()).ToList();

            AdvanceRound();

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Buffs.Count; i++)
                {
                    unit.Buffs[i].OnRoundEnd();
                }
            });
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound()
        {
            UnitTakingTurn = 0;

            InitiativeOrder = InitiativeOrder.OrderBy(i => i._movementAbility.GetEnergyCost()).ToList(); //sort the list by speed

            //do calculations here (advance an event, show a cutscene, etc)

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Buffs.Count; i++)
                {
                    unit.Buffs[i].OnRoundStart();
                }
            });

            StartTurn();
        }

        /// <summary>
        /// Start the turn for the unit that is currently up in the initiative order
        /// </summary>
        public virtual void StartTurn()
        {
            CurrentUnit = InitiativeOrder[UnitTakingTurn];

            //EnergyDisplayBar.SetEnergyFromUnit(CurrentUnit);

            //max energy displayed is the larger between current energy with buffs and default max energy.
            //If buffs are reducing energy the max will still be the default max for the unit.
            float maxEnergy = CurrentUnit.MaxEnergy > CurrentUnit.CurrentEnergy ? CurrentUnit.MaxEnergy : CurrentUnit.CurrentEnergy;
            EnergyDisplayBar.SetMaxEnergy(maxEnergy);
            EnergyDisplayBar.SetActiveEnergy(CurrentUnit.CurrentEnergy);

            Footer.UpdateFooterInfo(CurrentUnit);

            FillInTeamFog(CurrentUnit.Team);


            CurrentUnit.OnTurnStart();


            for (int i = 0; i < CurrentUnit.Buffs.Count; i++)
            {
                CurrentUnit.Buffs[i].OnTurnStart();
            }
            //change the UI, move the camera, show which unit is selected, etc
        }

        /// <summary>
        /// Complete the current unit's turn and start the next unit's turn
        /// </summary>
        public virtual void CompleteTurn()
        {
            if (CurrentUnit != null) 
            {
                CurrentUnit.OnTurnEnd();
            }

            UnitTakingTurn++;


            if (UnitTakingTurn == InitiativeOrder.Count)
            {
                CompleteRound();
                return;
            }

            StartTurn(); //Advance to the next unit's turn


            for (int i = 0; i < CurrentUnit.Buffs.Count; i++)
            {
                CurrentUnit.Buffs[i].OnTurnEnd();
            }
        }

        public virtual void SelectAbility(Ability ability)
        {
            if(_selectedAbility != null) 
            {
                DeselectAbility();
            }

            _selectedAbility = ability;
            ability.OnSelect(this, _tileMaps[0]);

            _onSelectAbilityActions.ForEach(a => a?.Invoke(ability));
        }

        public virtual void DeselectAbility()
        {
            if (_selectedAbility != null)
            {
                _selectedAbility.TileMap.DeselectTiles();

                _selectedAbility?.OnAbilityDeselect();
                _selectedAbility = null;
            }

            _onDeselectAbilityActions.ForEach(a => a?.Invoke());
        }

        public void SelectUnit(Unit unit) 
        {
            if (_selectedUnits.Count > 0) 
            {
                DeselectUnits();
            }

            _selectedUnits.Add(unit);
            unit.Select();
        }

        public void DeselectUnit(Unit unit) 
        {
            _selectedUnits.Remove(unit);
            unit.Deselect();
        }

        public void DeselectUnits() 
        {
            _selectedUnits.ForEach(u => u.Deselect());
            _selectedUnits.Clear();
        }

        public virtual void FillInTeamFog(UnitTeam currentTeam = UnitTeam.Ally) 
        {
            FillInAllFog(currentTeam);

            _units.ForEach(unit =>
            {
                if (unit.Team == currentTeam)
                {
                    List<BaseTile> tiles = _tileMaps[0].GetVisionInRadius(unit.TileMapPosition, unit.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, _units.FindAll(u => u.TileMapPosition != unit.TileMapPosition));

                    tiles.ForEach(tile =>
                    {
                        tile.SetExplored(true, currentTeam);
                        tile.SetFog(false, currentTeam);
                    });
                }
            });

            HideObjectsInFog();
        }

        public void FillInAllFog(UnitTeam currentTeam) 
        {
            _tileMaps[0].Tiles.ForEach(tile =>
            {
                tile.MultiTextureData.MixedTexture = _normalFogTexture;
                tile.MultiTextureData.MixedTextureLocation = TextureUnit.Texture1;
                tile.MultiTextureData.MixedTextureName = TextureName.FogTexture;

                //foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam))) 
                //{
                //tile.SetExplored(tile.Explored[team], team);
                //}

                tile.SetExplored(tile.Explored[currentTeam], currentTeam);
                tile.SetFog(true, currentTeam);
            });
        }

        public void HideObjectsInFog(List<Unit> units = null) 
        {
            if (units == null)
            {
                _units.ForEach(unit =>
                {
                    if (_tileMaps[0].Tiles[unit.TileMapPosition].InFog)
                    {
                        unit.SetRender(false);
                    }
                    else
                    {
                        unit.SetRender(true);
                    }
                });
            }
            else 
            {
                units.ForEach(unit =>
                {
                    if (_tileMaps[0].Tiles[unit.TileMapPosition].InFog)
                    {
                        unit.SetRender(false);
                    }
                    else
                    {
                        unit.SetRender(true);
                    }
                });
            }
        }

        public override void EvaluateObjectHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
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

                    if (tile.HasTimedHoverEffect)
                    {
                        _hoverTimer.Restart();
                        _hoveredObject = tile;
                    }
                });
            });

            ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar, (unit) =>
            {
                if (unit.Hoverable)
                    unit.OnHover();

                if (unit.HasTimedHoverEffect) 
                {
                    _hoverTimer.Restart();
                    _hoveredObject = unit;
                }

            }, notFound => notFound.HoverEnd());
        }

        public override void HandleRightClick()
        {
            base.HandleRightClick();

            DeselectUnits();

            if (_selectedAbility != null)
            {
                _selectedAbility.OnRightClick();
            }
        }

        public override bool onKeyDown(KeyboardKeyEventArgs e)
        {
            bool processKeyStrokes = base.onKeyDown(e);

            if (processKeyStrokes) 
            {
                switch (e.Key) 
                {
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        if (!e.IsRepeat) 
                        {
                            DisplayUnitStatuses = !DisplayUnitStatuses;
                            _units.ForEach(u =>
                            {
                                if (u.StatusBarComp != null && !u.Dead && u.Render) 
                                {
                                    u.StatusBarComp.SetWillDisplay(DisplayUnitStatuses);
                                }
                                    
                            });
                        }
                        break;
                }
            }

            return processKeyStrokes;
        }

        public virtual void onUnitKilled(Unit unit) 
        {
            if (unit.StatusBarComp != null) 
            {
                unit.StatusBarComp.SetWillDisplay(false);
            }

            int index = InitiativeOrder.FindIndex(u => u.ObjectID == unit.ObjectID);
            if (index != -1) 
            {
                if (index <= UnitTakingTurn) 
                {
                    UnitTakingTurn--;
                }
            }

            InitiativeOrder.Remove(unit);
        }

        public override void onUnitClicked(Unit unit)
        {
            base.onUnitClicked(unit);

            if (_selectedAbility == null)
            {
                if (unit.Selectable)
                    SelectUnit(unit);
                

                if (!InCombat)
                {
                    CurrentUnit = unit;
                    _selectedAbility = unit.GetFirstAbilityOfType(DefaultAbilityType);

                    if (_selectedAbility.Type != AbilityTypes.Empty)
                    {
                        SelectAbility(_selectedAbility);
                    }
                }
            }
            else
            {
                _selectedAbility.OnUnitClicked(unit);
            }
        }

        public virtual void onAbilityCast(Ability ability) 
        {
            _onAbilityCastActions.ForEach(a => a?.Invoke(ability));

            Footer.UpdateFooterInfo(Footer._currentUnit);
        }

        public List<Action<Ability>> _onSelectAbilityActions = new List<Action<Ability>>();
        public List<Action> _onDeselectAbilityActions = new List<Action>();
        public List<Action<Ability>> _onAbilityCastActions = new List<Action<Ability>>();



        public void CreateToolTip(string text, UIObject tooltipParent, UIObject baseObject)
        {
            if (ContextManager.GetFlag(GeneralContextFlags.TooltipOpen))
                return;

            ContextManager.SetFlag(GeneralContextFlags.TooltipOpen, true);

            TextBox tooltip = new TextBox(new Vector3(), new UIScale(), text, 0.05f, true);
            tooltip.SetColor(Colors.UILightGray);
            tooltip.SetTextColor(Colors.UITextBlack);
            tooltip.BaseComponent.MultiTextureData.MixTexture = false;

            tooltip.Hoverable = true;

            UIDimensions textOffset = new UIDimensions(tooltip.TextOffset);
            textOffset.Y = 0;

            UIScale textScale = tooltip.TextField.GetTextDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 2;
            textScale.Y *= -1;
            tooltip.SetSize(textScale);

            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(_cursorObject.Position + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);

            Console.WriteLine(_cursorObject.Position);

            baseObject.AddChild(tooltip, 100000);

            void temp()
            {
                baseObject.RemoveChild(tooltip.ObjectID);
                tooltipParent._onHoverEndActions.Remove(temp);
                ContextManager.SetFlag(GeneralContextFlags.TooltipOpen, false);
            }

            tooltipParent._onHoverEndActions.Add(temp);
        }
    }
}
