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
using System.Threading.Tasks;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public enum GeneralContextFlags
    {
        UITooltipOpen,
        TileTooltipOpen,
        ContextMenuOpen,
        AbilitySelected,
        TabMenuOpen,
        EnableTileMapUpdate
    }
    public class CombatScene : Scene
    {
        public int Round = 0;
        public QueuedList<Unit> InitiativeOrder = new QueuedList<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;
        public GameFooter Footer;

        public QueuedList<TemporaryVision> TemporaryVision = new QueuedList<TemporaryVision>();

        public static TabMenu TabMenu = new TabMenu();

        public Ability _selectedAbility = null;
        public List<Unit> _selectedUnits = new List<Unit>();

        public Unit CurrentUnit;
        public UnitTeam CurrentTeam = UnitTeam.Ally;

        public bool InCombat = false;

        public bool DisplayUnitStatuses = true;

        protected const AbilityTypes DefaultAbilityType = AbilityTypes.Move;

        Texture _normalFogTexture;
        public UIBlock _tooltipBlock;
        public Action _closeContextMenu;

        public CombatScene() 
        {
            Texture fogTex = Texture.LoadFromFile("Resources/FogTexture.png", default, TextureName.FogTexture);

            fogTex.Use(TextureUnit.Texture1);

            _normalFogTexture = fogTex;
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            _tileMapController = new TileMapController(this);

            _tooltipBlock = new UIBlock(new Vector3());
            _tooltipBlock.MultiTextureData.MixTexture = false;
            _tooltipBlock.SetColor(Colors.Transparent);
            _tooltipBlock.SetAllInline(0);

            AddUI(_tooltipBlock, 10000);

            TabMenu.AddToScene(this);
            TabMenu.Display(false);
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
            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderBy(i => i.Info.Speed));

            AdvanceRound();

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Info.Buffs.Count; i++)
                {
                    unit.Info.Buffs[i].OnRoundEnd();
                }
            });
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound()
        {
            UnitTakingTurn = 0;

            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderBy(i => i.Info.Speed)); //sort the list by speed

            //do calculations here (advance an event, show a cutscene, etc)

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Info.Buffs.Count; i++)
                {
                    unit.Info.Buffs[i].OnRoundStart();
                }
            });

            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnRoundStart)
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            StartTurn();
        }

        /// <summary>
        /// Start the turn for the unit that is currently up in the initiative order
        /// </summary>
        public virtual void StartTurn()
        {
            UnitTeam prevTeam = CurrentUnit.AI.Team;

            CurrentUnit = InitiativeOrder[UnitTakingTurn];

            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnUnitTurnStart && t.TargetUnit.ObjectID == CurrentUnit.ObjectID) 
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            //EnergyDisplayBar.SetEnergyFromUnit(CurrentUnit);

            //max energy displayed is the larger between current energy with buffs and default max energy.
            //If buffs are reducing energy the max will still be the default max for the unit.
            if (CurrentUnit.AI.ControlType == ControlType.Controlled) 
            {
                SetCurrentUnitEnergy();

                Footer.UpdateFooterInfo(CurrentUnit);
                Task.Run(() => FillInTeamFog(CurrentUnit.AI.Team, prevTeam, true));
            }

            CurrentUnit.OnTurnStart();

            for (int i = 0; i < CurrentUnit.Info.Buffs.Count; i++)
            {
                CurrentUnit.Info.Buffs[i].OnTurnStart();
            }
        }

        public void SetCurrentUnitEnergy() 
        {
            float maxEnergy = CurrentUnit.Info.MaxEnergy > CurrentUnit.Info.CurrentEnergy ? CurrentUnit.Info.MaxEnergy : CurrentUnit.Info.CurrentEnergy;
            EnergyDisplayBar.SetMaxEnergy(maxEnergy);
            EnergyDisplayBar.SetActiveEnergy(CurrentUnit.Info.CurrentEnergy);
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

            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnUnitTurnEnd && t.TargetUnit.ObjectID == CurrentUnit.ObjectID)
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            UnitTakingTurn++;


            if (UnitTakingTurn == InitiativeOrder.Count)
            {
                CompleteRound();
                return;
            }

            StartTurn(); //Advance to the next unit's turn


            for (int i = 0; i < CurrentUnit.Info.Buffs.Count; i++)
            {
                CurrentUnit.Info.Buffs[i].OnTurnEnd();
            }
        }

        public virtual void StartCombat() 
        {
            InitiativeOrder = new QueuedList<Unit>(_units);
            InitiativeOrder.RemoveAll(u => u.Info.NonCombatant);

            if (InitiativeOrder.Count == 0)
                return;

            EnergyDisplayBar.SetRender(true);
            //Footer.EndTurnButton.SetDisabled(false);
            Footer.EndTurnButton.SetRender(true);

            InCombat = true;

            Round = 0;

            StartRound();
        }

        public virtual void EndCombat() 
        {
            InitiativeOrder.Clear();
            InCombat = false;

            EnergyDisplayBar.SetRender(false);
            //Footer.EndTurnButton.SetDisabled(true);
            Footer.EndTurnButton.SetRender(false);
            SetCurrentUnitEnergy();
        }

        public virtual void SelectAbility(Ability ability)
        {
            if(_selectedAbility != null) 
            {
                DeselectAbility();
            }

            ContextManager.SetFlag(GeneralContextFlags.AbilitySelected, true);

            Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
            MessageCenter.SendMessage(msg);

            _selectedAbility = ability;
            ability.OnSelect(this, ability.CastingUnit.GetTileMap());

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

            ContextManager.SetFlag(GeneralContextFlags.AbilitySelected, false);

            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);

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

        public virtual void FillInTeamFog(UnitTeam currentTeam = UnitTeam.Ally, UnitTeam previousTeam = UnitTeam.Unknown, bool updateAll = false) 
        {
            //don't allow the tilemap to be re-rendered until all of the changes are applied
            ContextManager.SetFlag(GeneralContextFlags.EnableTileMapUpdate, false);

            FillInAllFog(currentTeam, previousTeam, false, updateAll);


            _units.ForEach(unit =>
            {
                List<BaseTile> tiles = unit.GetTileMap().GetVisionInRadius(unit.Info.TileMapPosition, unit.Info.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, _units.FindAll(u => u.Info.TileMapPosition != unit.Info.TileMapPosition));

                tiles.ForEach(tile =>
                {
                    tile.SetExplored(true, unit.AI.Team, UnitTeam.Unknown);
                    tile.SetFog(false, unit.AI.Team);
                });
            });

            TemporaryVision.ForEach(vision =>
            {
                vision.TilesToReveal.ForEach(tile =>
                {
                    tile.SetExplored(true, vision.Team);
                    tile.SetFog(false, vision.Team);
                });
            });

            CalculateRevealedUnits();

            HideNonVisibleObjects();

            ContextManager.SetFlag(GeneralContextFlags.EnableTileMapUpdate, true);
        }

        public List<BaseTile> GetTeamVision(UnitTeam team) 
        {
            List<BaseTile> returnList = new List<BaseTile>();

            _units.ForEach(unit =>
            {
                if (unit.AI.Team == team)
                {
                    List<BaseTile> tiles;

                    if (unit.Info.TemporaryPosition != null)
                    {
                        tiles = unit.GetTileMap().GetVisionInRadius(unit.Info.TemporaryPosition, unit.Info.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, _units.FindAll(u => u.Info.TileMapPosition != unit.Info.TileMapPosition));
                    }
                    else 
                    {
                        tiles = unit.GetTileMap().GetVisionInRadius(unit.Info.TileMapPosition, unit.Info.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, _units.FindAll(u => u.Info.TileMapPosition != unit.Info.TileMapPosition));
                    }
                    tiles.ForEach(tile =>
                    {
                        returnList.Add(tile);
                    });
                }
            });

            return returnList;
        }


        public void FillInAllFog(UnitTeam currentTeam, UnitTeam previousTeam = UnitTeam.Unknown, bool reveal = false, bool updateAll = false)
        {
            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam)))
            {
                _tileMapController.TileMaps.ForEach(m =>
                {
                    if (!m.Render)
                        return;

                    m.Tiles.ForEach(tile =>
                    {
                        tile.MultiTextureData.MixedTexture = _normalFogTexture;
                        tile.MultiTextureData.MixedTextureLocation = TextureUnit.Texture1;
                        tile.MultiTextureData.MixedTextureName = TextureName.FogTexture;


                        if (reveal)
                        {
                            tile.SetExplored(true, team, UnitTeam.Unknown);
                            tile.SetFog(false, team);
                        }
                        else
                        {
                            tile.SetExplored(tile.Explored[team], team, UnitTeam.Unknown);
                            tile.SetFog(true, team);
                        }

                        if (updateAll)
                            tile.Update();
                    });
                });
            }
        }

        public void CalculateRevealedUnits() 
        {
            for (int i = 0; i < _units.Count; i++) 
            {
                //if the unit isn't attempting to hide then it is revealed
                if (!_units[i].Info.Stealth.Hiding) 
                {
                    _units[i].Info.Stealth.SetAllRevealed();
                    continue;
                }

                foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam))) 
                {
                    //we don't need to check the unit's team
                    if (team == _units[i].AI.Team)
                        continue;

                    //if the unit's position is in fog for a team then we can assume that it's hidden
                    if (_units[i].Info.Stealth.PositionInFog(team))
                    {
                        _units[i].Info.Stealth.SetRevealed(team, false);
                        continue;
                    }

                    //if we get to this point then we need to compare and contrast Stealth and Scout skill levels for the units
                    bool couldSee = false;
                    _units.ForEach(u =>
                    {
                        if (u.AI.Team == team) 
                        {
                            if (u.Info.Scouting.CouldSeeUnit(_units[i], _units[i].Info.Map.GetDistanceBetweenPoints(u.Info.Point, _units[i].Info.Point))) 
                            {
                                couldSee = true;
                            }
                        }
                    });

                    _units[i].Info.Stealth.SetRevealed(team, couldSee);
                }
            }
        }

        public void HideNonVisibleObjects() 
        {
            _units.ForEach(unit =>
            {
                if (unit.Info.Visible(CurrentTeam))
                {
                    unit.SetRender(true);
                }
                else 
                {
                    unit.SetRender(false);
                }
            });
        }

        public override void OnRender()
        {
            base.OnRender();

            UpdateTemporaryVision();

            if (InitiativeOrder.HasQueuedItems()) 
            {
                InitiativeOrder.HandleQueuedItems();
            }
        }

        public override void EvaluateObjectHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            _tileMapController.TileMaps.ForEach(map =>
            {
                if (!map.Render)
                    return;

                map.EndHover();

                map.TileChunks.ForEach(chunk =>
                {
                    if (!chunk.Cull) 
                    {
                        ObjectCursorBoundsCheck(chunk.Tiles, mouseRayNear, mouseRayFar, (tile) =>
                        {
                            if (tile.Hoverable)
                            {
                                map.HoverTile(tile);
                                if (_selectedAbility != null && _selectedAbility.HasHoverEffect)
                                {
                                    _selectedAbility.OnHover(tile, map);
                                }

                                if (Game.Settings.EnableTileTooltips) 
                                {
                                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(this, BaseTile.GetTooltipString(tile, this), tile, _tooltipBlock)
                                    {
                                        TooltipFlag = GeneralContextFlags.TileTooltipOpen,
                                        Position = new Vector3(WindowConstants.ScreenUnits.X, 0, 0),
                                        Anchor = UIAnchorPosition.TopRight,
                                        BackgroundColor = new Vector4(0.85f, 0.85f, 0.85f, 0.5f),
                                        TextScale = 0.04f
                                    };

                                    UIHelpers.CreateToolTip(param);
                                }
                            }

                            if (tile.HasTimedHoverEffect)
                            {
                                _hoverTimer.Restart();
                                _hoveredObject = tile;
                            }
                        });
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


        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            SetMouseStateFlags();

            if ((e.Button == MouseButton.Right) && e.Action == InputAction.Release && !GetBit(_interceptClicks, ObjectType.All))
            {
                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ContextMenuOpen))
                {
                    CloseContextMenu();
                }

                DeselectUnits();

                if (_selectedAbility != null)
                {
                    _selectedAbility.OnRightClick();
                    return;
                }
            }

            CheckMouseUp(e);
        }

        protected override void SetMouseStateFlags()
        {
            base.SetMouseStateFlags();

            MouseUpStateFlags.SetFlag(MouseUpFlags.ContextMenuOpen, ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen));
            MouseUpStateFlags.SetFlag(MouseUpFlags.AbilitySelected, ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen));
            MouseUpStateFlags.SetFlag(MouseUpFlags.TabMenuOpen, ContextManager.GetFlag(GeneralContextFlags.TabMenuOpen));
        }

        protected override void ActOnMouseStateFlag(MouseUpFlags flag)
        {
            base.ActOnMouseStateFlag(flag);

            switch (flag) 
            {
                case MouseUpFlags.ContextMenuOpen:
                    MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                    CloseContextMenu();
                    break;
                case MouseUpFlags.TabMenuOpen:
                    MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                    TabMenu.Display(false);
                    break;
            }
        }

        public override bool OnKeyDown(KeyboardKeyEventArgs e)
        {
            bool processKeyStrokes = base.OnKeyDown(e);

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
                                if (u.StatusBarComp != null && !u.Info.Dead && u.Render) 
                                {
                                    u.StatusBarComp.SetWillDisplay(DisplayUnitStatuses);
                                }
                                    
                            });
                        }
                        break;
                    case Keys.Escape:
                        if (ContextManager.GetFlag(GeneralContextFlags.TabMenuOpen)) 
                        {
                            TabMenu.Display(false);
                        }
                        else if (ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen))
                        {
                            CloseContextMenu();
                        }
                        else if (ContextManager.GetFlag(GeneralContextFlags.AbilitySelected))
                        {
                            DeselectAbility();
                        }
                        break;
                    case Keys.Tab:
                        if (!e.IsRepeat) 
                        {
                            TabMenu.Display(!TabMenu.Render);
                        }
                        break;
                    case Keys.F12:
                        if (!e.IsRepeat)
                        {
                            _tileMapController.LoadSurroundingTileMaps(CurrentUnit.GetTileMap().TileMapCoords);
                        }
                        break;
                }
            }

            return processKeyStrokes;
        }

        public virtual void OnUnitKilled(Unit unit) 
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

        public override void OnUnitClicked(Unit unit, MouseButton button)
        {
            base.OnUnitClicked(unit, button);

            if (button == MouseButton.Left)
            {
                if (_selectedAbility == null)
                {
                    if (unit.Selectable && unit.Info.Visible(CurrentTeam))
                        SelectUnit(unit);
                }
                else
                {
                    _selectedAbility.OnUnitClicked(unit);
                }
            }
            else 
            {
                unit.OnRightClick();
            }
        }

        public virtual void OnAbilityCast(Ability ability) 
        {
            _onAbilityCastActions.ForEach(a => a?.Invoke(ability));

            Footer.UpdateFooterInfo(Footer._currentUnit);
        }

        public List<Action<Ability>> _onSelectAbilityActions = new List<Action<Ability>>();
        public List<Action> _onDeselectAbilityActions = new List<Action>();
        public List<Action<Ability>> _onAbilityCastActions = new List<Action<Ability>>();


        public void OpenContextMenu(Tooltip menu) 
        {
            UIHelpers.CreateContextMenu(this, menu, _tooltipBlock);

            Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public void CloseContextMenu() 
        {
            _closeContextMenu?.Invoke();

            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public void TickTemporaryVision(TemporaryVision t) 
        {
            t.TickDuration();

            if (t.Duration <= 0) 
            {
                TemporaryVision.Remove(t);
                t.TilesToReveal.Clear();
            }
        }

        /// <summary>
        /// Add or remove any new TemporaryVision objects and apply those changes to the vision.
        /// </summary>
        public void UpdateTemporaryVision() 
        {
            if (TemporaryVision.HasQueuedItems()) 
            {
                TemporaryVision.HandleQueuedItems();
                FillInTeamFog();
            }
        }
    }
}
