using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Events;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Structures;
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        EnableTileMapUpdate,

        DisableVisionMapUpdate,
        TileMapLoadInProgress,
        TileMapManagerLoading,

        SaveStateLoadInProgress,

        UpdateLighting,
        UpdateLightObstructionMap,

        UnitCollationRequired,

        CameraPanning,

        PatternToolEnabled,

        RightClickMovementEnabled,

        DisallowCameraMovement,

        ClearingTeamVision,

        EditingFeature
    }
    public class CombatScene : Scene
    {
        public int Round = 0;
        public QueuedList<Unit> InitiativeOrder = new QueuedList<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;
        public EnergyDisplayBar ActionEnergyBar;

        public TurnDisplay TurnDisplay;
        public GameFooter Footer;

        public QueuedList<TemporaryVision> TemporaryVision = new QueuedList<TemporaryVision>();

        public static _Color EnvironmentColor = new _Color(0.25f, 0.25f, 0.25f, 0.25f);
        public int Time = 0;
        public static int Days = 0;
        public DayNightCycle DayNightCycle;

        public static DialogueWindow DialogueWindow;

        public static TabMenu TabMenu = null;
        public SideBar SideBar = null;

        public WorldMap WorldMap = null;

        public Ability _selectedAbility = null;
        public List<Unit> _selectedUnits = new List<Unit>();


        public Unit CurrentUnit;
        public UnitTeam CurrentTeam = UnitTeam.PlayerUnits;

        public UnitTeam VisibleTeam = UnitTeam.PlayerUnits;
        
        public bool InCombat = false;

        public bool AbilityInProgress = false;

        public bool DisplayUnitStatuses = true;

        public EventLog EventLog = null;


        public int TimePassageRate = 1200;

        public UIBlock _tooltipBlock;
        public Action _closeContextMenu;

        public BaseTile _debugSelectedTile;

        public UIBlock _unitStatusBlock;

        public GameObject _fogQuad = null;

        public CombatScene() 
        {
            _fogQuad = new GameObject(Textures.TentTexture, 0);
            _fogQuad.BaseObject.BaseFrame.CameraPerspective = false;
            _fogQuad.SetColor(new Vector4(0.25f, 0.25f, 0.3f, 0.5f));
            _fogQuad.SetScale(3);

            _fogQuad.SetPosition(WindowConstants.CenterScreen);
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            BoxSelectHelper = new Game.SceneHelpers.BoxSelectHelper(this);

            _tileMapController = new TileMapController(this);

            _tooltipBlock = new UIBlock(new Vector3());
            _tooltipBlock.MultiTextureData.MixTexture = false;
            _tooltipBlock.SetColor(_Colors.Transparent);
            _tooltipBlock.SetAllInline(0);

            AddUI(_tooltipBlock, 99999999);

            _unitStatusBlock = new UIBlock(new Vector3());
            _unitStatusBlock.MultiTextureData.MixTexture = false;
            _unitStatusBlock.SetColor(_Colors.Transparent);
            _unitStatusBlock.SetAllInline(0);

            AddUI(_unitStatusBlock, -50);

            TabMenu = new TabMenu();
            TabMenu.AddToScene(this);
            TabMenu.Display(false);

            SideBar = new SideBar(this);
            AddUI(SideBar.ParentObject);

            DialogueWindow = new DialogueWindow(this);
            AddUI(DialogueWindow.Window, 1000);

            WorldMap = new WorldMap(this);
        }

        public override void Load(Camera camera = null, MouseRay mouseRay = null)
        {
            base.Load(camera, mouseRay);

            //1200 ms per step puts us at around 10 minutse per day/night cycle
            DayNightCycle = new DayNightCycle(TimePassageRate, DayNightCycle.MiddayStart, this);
            TimedTickableObjects.Add(DayNightCycle);
        }

        public void SetTime(int time)
        {
            TimedTickableObjects.Remove(DayNightCycle);
            DayNightCycle = new DayNightCycle(TimePassageRate, time, this);
            TimedTickableObjects.Add(DayNightCycle);

            Time = time;
        }

        private int _outOfCombatTimeCounter = 0;
        public void UpdateTime(int time) 
        {
            Time = time;

            if (!InCombat) 
            {
                _outOfCombatTimeCounter++;

                if(_outOfCombatTimeCounter % 5 == 0) 
                {
                    ResolveOutOfCombatTurn();
                }
            }
        }

        public void ResolveOutOfCombatTurn() 
        {
            lock (_units._lock) 
            {
                foreach(var unit in _units) 
                {
                    unit.OnTurnStart();
                    unit.OnTurnEnd();
                }

                //Footer.UpdateFooterInfo();
            }
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
            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderByDescending(i => i.Info.Speed));

            AdvanceRound();

            _units.ForEach(unit =>
            {
                unit.OnRoundEnd();
            });

            foreach (var item in TileEffectManager.TileEffects)
            {
                foreach (var effect in item.Value)
                {
                    effect.OnRoundEnd(item.Key);
                }
            }
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound()
        {
            UnitTakingTurn = 0;

            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderByDescending(i => i.Info.Speed)); //sort the list by speed

            TurnDisplay.SetUnits(InitiativeOrder, this);
            //TurnDisplay.SetCurrentUnit(UnitTakingTurn);

            //do calculations here (advance an event, show a cutscene, etc)

            _units.ForEach(unit =>
            {
                unit.OnRoundStart();
            });

            foreach(var item in TileEffectManager.TileEffects)
            {
                foreach(var effect in item.Value)
                {
                    effect.OnRoundStart(item.Key);
                }
            }

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
            UnitTeam prevTeam;

            if (CurrentUnit == null)
            {
                prevTeam = UnitTeam.PlayerUnits;
            }
            else
            {
                prevTeam = CurrentUnit.AI.Team;
            }
            

            CurrentUnit = InitiativeOrder[UnitTakingTurn];


            lock (TemporaryVision._lock) 
            {
                foreach(var t in TemporaryVision) 
                {
                    if (t.TickTarget == TickDurationTarget.OnUnitTurnStart && t.TargetUnit.ObjectID == CurrentUnit.ObjectID)
                    {
                        TickTemporaryVision(t);
                    }
                }
            }

            UpdateTemporaryVision();

            CurrentUnit.Info.Energy = CurrentUnit.Info.CurrentEnergy;
            CurrentUnit.Info.ActionEnergy = CurrentUnit.Info.CurrentActionEnergy;

            //max energy displayed is the larger between current energy with buffs and default max energy.
            //If buffs are reducing energy the max will still be the default max for the unit.
            if (CurrentUnit.AI.ControlType == ControlType.Controlled)
            {
                ShowEnergyDisplayBars(true);
                SetCurrentUnitEnergy();

                //DeselectAllUnits();

                Footer.UpdateFooterInfo(CurrentUnit);
                CurrentUnit.Select();

                if(!AbilityInProgress)
                    Footer.EndTurnButton.SetRender(true);

                //FillInTeamFog(true);

                SmoothPanCameraToUnit(CurrentUnit, 1);
            }
            else 
            {
                Footer.EndTurnButton.SetRender(false);
                ShowEnergyDisplayBars(false);
            }

            //FillInTeamFog(true);

            TurnDisplay.SetCurrentUnit(UnitTakingTurn);


            if (CurrentUnit.AI.ControlType == ControlType.Controlled)
            {
                Footer.UpdateFooterInfo(CurrentUnit);
            }

            Task.Run(CurrentUnit.OnTurnStart);
        }

        public void SetCurrentUnitEnergy() 
        {
            if (CurrentUnit == null)
                return;

            float maxEnergy = CurrentUnit.Info.MaxEnergy > CurrentUnit.Info.CurrentEnergy ? CurrentUnit.Info.MaxEnergy : CurrentUnit.Info.CurrentEnergy;
            EnergyDisplayBar.SetMaxEnergy(maxEnergy);
            EnergyDisplayBar.SetActiveEnergy(CurrentUnit.Info.CurrentEnergy);

            float maxActionEnergy = CurrentUnit.Info.MaxActionEnergy > CurrentUnit.Info.CurrentActionEnergy ? CurrentUnit.Info.MaxActionEnergy : CurrentUnit.Info.CurrentActionEnergy;
            ActionEnergyBar.SetMaxEnergy(maxActionEnergy);
            ActionEnergyBar.SetActiveEnergy(CurrentUnit.Info.CurrentActionEnergy);
        }

        public void RefillAllUnitEnergy() 
        {
            foreach(var unit in _units) 
            {
                unit.Info.Energy = unit.Info.MaxEnergy;
                unit.Info.ActionEnergy = unit.Info.MaxActionEnergy;
            }
        }

        public void ShowEnergyDisplayBars(bool render) 
        {
            EnergyDisplayBar.SetRender(render);
            ActionEnergyBar.SetRender(render);
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

            lock (TemporaryVision._lock)
            {
                foreach(var t in TemporaryVision)
                {
                    if (t.TickTarget == TickDurationTarget.OnUnitTurnEnd && t.TargetUnit.ObjectID == CurrentUnit.ObjectID)
                    {
                        TickTemporaryVision(t);
                    }
                }
            }

            UpdateTemporaryVision();

            UnitTakingTurn++;


            if (UnitTakingTurn == InitiativeOrder.Count)
            {
                CompleteRound();
                return;
            }

            if (InCombat)
            {
                StartTurn(); //Advance to the next unit's turn
            }
        }

        public virtual void StartCombat() 
        {
            if (InCombat)
                return;

            PlayerParty.EnterCombat();


            InitiativeOrder.RemoveAll(u => u.Info.NonCombatant || u.Info.Dead);
            EvaluatePacksInCombat();

            if (InitiativeOrder.Count == 0)
                return;

            TileMapManager.SetCenter(InitiativeOrder[0].Info.TileMapPosition.TileMap.TileMapCoords);
            TileMapManager.LoadMapsAroundCenter();

            ShowEnergyDisplayBars(true);

            //Footer.EndTurnButton.SetDisabled(false);
            //Footer.EndTurnButton.SetRender(true);

            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderByDescending(i => i.Info.Speed)); //sort the list by speed

            TurnDisplay.SetRender(true);
            TurnDisplay.SetUnits(InitiativeOrder, this);


            Unit enemy = InitiativeOrder.Find(u => u.AI.Team.GetRelation(UnitTeam.PlayerUnits) == Relation.Hostile);
            if(enemy != null)
            {
                EventLog.AddEvent("You've been accosted by a group of " + enemy.AI.Team.Name());
            }
            else 
            {
                EventLog.AddEvent("You appear to be under attack");
            }
            
            InCombat = true;

            Round = 0;


            //EvaluateVentureButton();

            StartRound();
        }

        public virtual void EndCombat() 
        {
            //InitiativeOrder.ForEach(unit =>
            //{
            //    unit.RefillAbilityCharges();
            //});

            InitiativeOrder.Clear();
            EvaluatePacksInCombat();
            InCombat = false;

            CurrentUnit = _units.Find(u => u.pack_name == "player_party" && !u.Info.Dead);

            ShowEnergyDisplayBars(false);
            Footer.EndTurnButton.SetRender(false);

            //EvaluateVentureButton();

            TurnDisplay.SetRender(false);
            TurnDisplay.SetUnits(InitiativeOrder, this);

            SetCurrentUnitEnergy();
            RefillAllUnitEnergy();

            Footer.UpdateFooterInfo();

            PlayerParty.ExitCombat();
        }

        public virtual void SelectAbility(Ability ability, Unit unit)
        {
            if (unit.AI.ControlType != ControlType.Controlled || (AbilityInProgress && !ability.MustCast) || ability.Type == AbilityTypes.Passive)
                return;

            if (unit.Info.Dead)
                return;

            if (_selectedAbility != null && _selectedAbility.MustCast)
                return;

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

            if (!InCombat)
            {
                CurrentUnit = unit;
            }

            BoxSelectHelper.AllowSelection = false;
        }

        public virtual void DeselectAbility()
        {
            if (_selectedAbility != null)
            {
                if (_selectedAbility.MustCast)
                    return;


                _tileMapController.DeselectTiles();

                _selectedAbility?.OnAbilityDeselect();
                _selectedAbility = null;
            }

            ContextManager.SetFlag(GeneralContextFlags.AbilitySelected, false);

            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);

            for(int i = 0; i < _onDeselectAbilityActions.Count; i++) 
            {
                _onDeselectAbilityActions[i]?.Invoke();
            }

            BoxSelectHelper.AllowSelection = true;
        }

        public void SelectUnit(Unit unit) 
        {
            if(_selectedUnits.Count == 1 && _selectedUnits[0] == unit)
            {
                Footer.UpdateFooterInfo(unit);
                return;
            }
            else if (_selectedUnits.Count > 0 && !KeyboardState.IsKeyDown(Keys.LeftShift)) 
            {
                DeselectUnits();
            }

            if (_selectedUnits.Count > 0 && KeyboardState.IsKeyDown(Keys.LeftShift)) 
            {
                if (unit.AI.ControlType == ControlType.Controlled)
                {
                    _selectedUnits.Add(unit);
                    unit.Select();
                }
                else
                {
                    return;
                }
            }
            else
            {
                _selectedUnits.Add(unit);
                unit.Select();
            }
            

            if (!InCombat && unit.AI.ControlType == ControlType.Controlled && _selectedUnits.Count == 1)
            {
                CurrentUnit = unit;
            }

            if(_selectedUnits.Count == 1)
            {
                Footer.UpdateFooterInfo(unit);
            }
            else if(_selectedUnits.Count > 1)
            {
                Footer.UpdateFooterInfo(unit, footerMode: FooterMode.MultiUnit);
            }
        }

        public void SelectUnits(List<Unit> units)
        {
            if (!KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                DeselectUnits();
            }

            foreach(var unit in units)
            {
                unit.Select();
                _selectedUnits.Add(unit);
            }

            if (_selectedUnits.Count == 1)
            {
                Footer.UpdateFooterInfo(_selectedUnits[0]);
            }
            else if (_selectedUnits.Count > 1)
            {
                Footer.UpdateFooterInfo(_selectedUnits[0], footerMode: FooterMode.MultiUnit);
            }
        }

        public void DeselectUnit(Unit unit) 
        {
            _selectedUnits.Remove(unit);
            unit.Deselect();

            if(Footer.CurrentUnit == unit)
            {
                Footer.UpdateFooterInfo(setNull: true);
            }
            else if (_selectedUnits.Count > 1)
            {
                Footer.UpdateFooterInfo(_selectedUnits[0], footerMode: FooterMode.MultiUnit);
            }
        }

        public void DeselectUnits() 
        {
            _selectedUnits.ForEach(u => 
            {
                u.Deselect();

                if(Footer.CurrentUnit == u)
                {
                    Footer.UpdateFooterInfo(setNull: true);
                }
            });
            _selectedUnits.Clear();
        }

        public void DeselectAllUnits()
        {
            lock (_units._lock)
            {
                _units.ForEach(u => u.Deselect());
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

                foreach (UnitTeam team in ActiveTeams) 
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
                            if (u.Info.Scouting.CouldSeeUnit(_units[i], TileMap.GetDistanceBetweenPoints(u.Info.Point, _units[i].Info.Point))) 
                            {
                                couldSee = true;
                            }
                        }
                    });

                    //if a unit is being revealed from stealth then we want to stop the current movement ability
                    if (team == CurrentTeam && !_units[i].Info.Stealth.Revealed[team] && couldSee) 
                    {
                        if (CurrentUnit.AI.ControlType == ControlType.Controlled && CurrentUnit.Info._movementAbility.Moving) 
                        {
                            CurrentUnit.Info._movementAbility.CancelMovement();
                        }
                    }

                    _units[i].Info.Stealth.SetRevealed(team, couldSee);
                }
            }
        }

        public void HideNonVisibleObjects() 
        {
            try
            {
                _units.ForEach(unit =>
                {
                    if (unit.Info.TileMapPosition != null && unit.Info.Visible(CurrentTeam))
                    {
                        unit.SetRender(true);
                    }
                    else
                    {
                        unit.SetRender(false);
                    }
                });

                for (int i = 0; i < TileMapManager.ActiveMaps.Count; i++)
                {
                    for (int j = 0; j < TileMapManager.ActiveMaps[i].TileChunks.Count; j++)
                    {
                        foreach(var structure in TileMapManager.ActiveMaps[i].TileChunks[j].Structures)
                        {
                            if (structure.Info.Visible(CurrentTeam))
                            {
                                structure.SetRender(true);
                            }
                            else
                            {
                                structure.SetRender(false);
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Error in HideNonVisibleObjects: {e.Message}");
            }
        }

        /// <summary>
        /// Determine which units should be present in the initiative order
        /// </summary>
        public void EvaluateCombat() 
        {
            if (ContextManager.GetFlag(GeneralContextFlags.SaveStateLoadInProgress) || 
                ContextManager.GetFlag(GeneralContextFlags.EditingFeature))
                return;

            lock (_units._lock)
            {
                foreach (var unit in _units) 
                {
                    if (unit.AI.Team != UnitTeam.PlayerUnits)
                        continue;

                    foreach (UnitTeam team in ActiveTeams) 
                    {
                        if (unit.AI.Team.GetRelation(team) == Relation.Hostile) 
                        {
                            if (unit.Info.Visible(team))
                            {
                                EvaluateUnitsInCombat(unit, 20);

                                if (!InCombat) 
                                {
                                    foreach (var initiativeUnit in InitiativeOrder)
                                    {
                                        if (InitiativeOrder.Exists(u => u.AI.Team.GetRelation(initiativeUnit.AI.Team) == Relation.Hostile)) 
                                        {
                                            //stop unit movement
                                            if (CurrentUnit != null && CurrentUnit.Info._movementAbility != null && CurrentUnit.Info._movementAbility.Moving) 
                                            {
                                                CurrentUnit.Info._movementAbility._moveCancelAction = () =>
                                                {
                                                    CurrentUnit.Info._movementAbility._moveCancelAction = null;
                                                    Task.Run(StartCombat);
                                                };

                                                CurrentUnit.Info._movementAbility.CancelMovement();
                                            }
                                            else 
                                            {
                                                //if we aren't already in combat and we can verify that at least one enemy is present, start combat
                                                Task.Run(StartCombat);
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public HashSet<string> PacksInCombat = new HashSet<string>();
        public void EvaluateUnitsInCombat(Unit seedUnit, int radius) 
        {
            List<Unit> unitsToCheck = new List<Unit>();
            HashSet<Unit> checkedUnits = new HashSet<Unit>();

            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            unitsToCheck.Add(seedUnit);
            checkedUnits.Add(seedUnit);

            lock (InitiativeOrder._lock)
            {
                for (int i = 0; i < unitsToCheck.Count; i++)
                {
                    lock (_units._lock)
                    {
                        foreach (var u in _units)
                        {
                            if (!u.Info.Dead && u.Info.TileMapPosition != null &&
                                (TileMap.GetDistanceBetweenPoints(u.Info.TileMapPosition, unitsToCheck[i].Info.TileMapPosition) <= radius 
                                || PacksInCombat.Contains(u.pack_name)))
                            {
                                Relation unitRelation = u.AI.Team.GetRelation(unitsToCheck[i].AI.Team);
                                if (unitRelation == Relation.Friendly || unitRelation == Relation.Hostile)
                                {
                                    if (!checkedUnits.Contains(u))
                                    {
                                        unitsToCheck.Add(u);
                                        checkedUnits.Add(u);
                                    }

                                    if (!InitiativeOrder.Contains(u))
                                    {
                                        if(u.pack_name != "")
                                            PacksInCombat.Add(u.pack_name);

                                        InitiativeOrder.AddImmediate(u);
                                        TurnDisplay.SetUnits(InitiativeOrder, this);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Console.WriteLine($"Combat unit evaluation completed in {timer.ElapsedMilliseconds}ms");
        }

        public void EvaluatePacksInCombat()
        {
            PacksInCombat.Clear();

            lock (InitiativeOrder._lock)
            {
                for(int i = 0; i < InitiativeOrder.Count; i++)
                {
                    if(InitiativeOrder[i].pack_name != "")
                        PacksInCombat.Add(InitiativeOrder[i].pack_name);
                }
            }
        }

        private bool _endTurnButtonShouldDisplayAfterAbility = false;
        public void SetAbilityInProgress(bool abilityInProgress) 
        {
            AbilityInProgress = abilityInProgress;

            if (AbilityInProgress && Footer.EndTurnButton.Render)
            {
                Footer.EndTurnButton.SetRender(false);
                _endTurnButtonShouldDisplayAfterAbility = true;
            }
            else if (!InCombat) 
            {
                _endTurnButtonShouldDisplayAfterAbility = false;
            }
            else if (_endTurnButtonShouldDisplayAfterAbility)
            {
                Footer.EndTurnButton.SetRender(true);
                _endTurnButtonShouldDisplayAfterAbility = false;
            }
            else if (InCombat && !AbilityInProgress && CurrentUnit.AI.ControlType == ControlType.Controlled) 
            {
                Footer.EndTurnButton.SetRender(true);
            }
        }

        public virtual void RemoveUnit(Unit unit, bool immediate = false)
        {
            if (immediate) 
            {
                _units.RemoveImmediate(unit);
                InitiativeOrder.RemoveImmediate(unit);
                EvaluatePacksInCombat();
            }
            else 
            {
                _units.Remove(unit);
                InitiativeOrder.Remove(unit);
            }

            //FillInTeamFog();

            if(Footer != null)
                Footer.RefreshFooterInfo();
        }

        #region Event handlers


        public void OnUnitMoved(Unit unit, BaseTile prevTile) 
        {
            //if (CurrentUnit == unit && CurrentUnit.AI.Team == UnitTeam.PlayerUnits && CurrentUnit.AI.ControlType == ControlType.Controlled) 
            //{
            //    EvaluateVentureButton();
            //}

            //UpdateVisionMap(() => 
            //{
            //    EvaluateCombat();
            //}, unit.AI.Team);

            VisionManager.CalculateVisionForUnit(unit);

            ObjectCulling.CullListOfUnits(new List<Unit>() { unit });

            if(prevTile != null)
            {
                Task.Run(() =>
                {
                    FeatureManager.OnUnitMoved(unit);
                });
            }
        }

        public void UpdateUnitStatusBars()
        {
            Window.RenderBegin -= UpdateUnitStatusBars;

            var cameraMatrices = _camera.GetViewMatrix() * _camera.ProjectionMatrix;

            for (int i = 0; i < _unitStatusBlock.Children.Count; i++)
            {
                var statusBar = _unitStatusBlock.Children[i] as UnitStatusBar;
                if(statusBar != null)
                {
                    statusBar.UpdateUnitStatusPosition(cameraMatrices);
                }
            }
        }

        public void OnStructureMoved() 
        {
            if (!ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading))
            {
                VisionManager.PrepareLightObstructions(LightObstructions);
                CreateStructureInstancedRenderData();
            }
        }

        public void CreateStructureInstancedRenderData()
        {
            List<Structure> structuresToRender = new List<Structure>();

            foreach(var structure in _structures)
            {
                if(structure.Info.TileMapPosition != null && TileMapManager.VisibleMaps.Contains(structure.Info.TileMapPosition.TileMap))
                {
                    structuresToRender.Add(structure);
                }
            }

            SyncToRender(() => 
            {
                RenderingQueue.GenerateStructureInstancedRenderData(structuresToRender);
            });
        }

        public override void OnRender()
        {
            base.OnRender();

            UpdateTemporaryVision();

            if (InitiativeOrder.HasQueuedItems()) 
            {
                InitiativeOrder.HandleQueuedItems();
                EvaluatePacksInCombat();
            }
        }

        public delegate void TileHoverEventHandler(BaseTile tile);
        public event TileHoverEventHandler TileHover;

        public override void EvaluateObjectHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            if (!TileMapsFocused)
                return;

            _tileMapController.EndHover();

            lock (TileMapManager._visibleMapLock)
            {
                //Finds the closest maps to the mouse point and then enumerates its chunks to find the closest chunks to the point
                //The chunks are then evaluated in order based on their distance
                //If a tile is found the loop breaks

                var chunksByDistance = TileMapHelpers.GetChunksByDistance(mouseRayNear, mouseRayFar);

                for (int i = 0; i < chunksByDistance.Count; i++)
                {
                    var chunk = chunksByDistance[i].Chunk;
                    if (!chunk.Cull)
                    {
                        ObjectCursorBoundsCheck(chunk.Tiles, mouseRayNear, mouseRayFar, (tile) =>
                        {
                            if (tile.Hoverable)
                            {
                                tile.TileMap.Controller.HoverTile(tile);

                                TileHover?.Invoke(tile);

                                if (_selectedAbility != null && _selectedAbility.HasHoverEffect)
                                {
                                    _selectedAbility.OnHover(tile, tile.TileMap);
                                }

                                if (Game.Settings.EnableTileTooltips)
                                {
                                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(this, BaseTile.GetTooltipString(tile, this), tile, _tooltipBlock)
                                    {
                                        TooltipFlag = GeneralContextFlags.TileTooltipOpen,
                                        Position = new Vector3(WindowConstants.ScreenUnits.X, 0, 0),
                                        Anchor = UIAnchorPosition.TopRight,
                                        BackgroundColor = new Vector4(0.85f, 0.85f, 0.85f, 0.9f),
                                        TextScale = 0.07f,
                                        EnforceScreenBounds = false
                                    };

                                    UIHelpers.CreateToolTip(param);
                                }
                            }

                            if (tile.HasTimedHoverEffect)
                            {
                                _hoverTimer.Restart();
                                _hoveredObject = tile;
                            }

                            i = chunksByDistance.Count; //break out of the loop
                        });
                    }
                }

                //Console.WriteLine(chunksByDistance.Count);
            }


            ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar, (unit) =>
            {
                if (unit.Hoverable)
                    unit.OnHover();

                if (unit.HasTimedHoverEffect) 
                {
                    _hoverTimer.Restart();
                    _hoveredObject = unit;
                }

            }, notFound => notFound.OnHoverEnd());
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
                    //TabMenu.Display(false); //We probably don't need to close the tab menu when a click happens
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
                        Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.5f, 0.5f) };
                        sound.Play();

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
                        else if (ContextManager.GetFlag(GeneralContextFlags.UITooltipOpen))
                        {
                            UIForceClose(new SceneEventArgs(this, EventHandlerAction.CloseTooltip));
                        }
                        else if (Footer.CurrentUnit != null && Footer.CurrentUnit.AI.ControlType != ControlType.Controlled)
                        {
                            DeselectUnits();

                            Unit firstControlledUnit = _units.Find(u => u.AI.ControlType == ControlType.Controlled);
                            if (firstControlledUnit != null) 
                            {
                                firstControlledUnit.Select();
                            }
                        }
                        else if (_selectedUnits.Count > 0) 
                        {
                            DeselectUnits();
                        }
                        else if (Footer.CurrentUnit != null)
                        {
                            Footer.UpdateFooterInfo(setNull: true);
                        }
                        else
                        {
                            MessageCenter.SendMessage(new Message(MessageType.Request, MessageBody.Flag, MessageTarget.All) { Flag = MessageFlag.OpenEscapeMenu });
                        }
                        break;
                    case Keys.Tab:
                        if (!e.IsRepeat) 
                        {
                            TabMenu.Display(!TabMenu.Render);
                        }
                        break;
                }
            }

            return processKeyStrokes;
        }

        public virtual void OnUnitKilled(Unit unit) 
        {
            Relation unitRelation = unit.AI.Team.GetRelation(UnitTeam.PlayerUnits);
            if (unitRelation == Relation.Friendly) 
            {
                EventLog.AddEvent(unit.Name + " has perished.", EventSeverity.Severe);
            }
            else if(unitRelation == Relation.Hostile) 
            {
                EventLog.AddEvent(unit.Name + " has been vanquished.", EventSeverity.Positive);
            }
            else if (unitRelation == Relation.Neutral)
            {
                EventLog.AddEvent(unit.Name + " has died.", EventSeverity.Info);
            }


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

            InitiativeOrder.RemoveImmediate(unit);
            EvaluatePacksInCombat();
            TurnDisplay.SetUnits(InitiativeOrder, this);

            EvaluateCombat();

            if (InitiativeOrder.All(unit => unit.AI.Team.GetRelation(UnitTeam.PlayerUnits) == Relation.Friendly || !unit.AI.Fighting))
            {
                EndCombat();
            }
            else if (InitiativeOrder.All(unit => unit.AI.Team.GetRelation(UnitTeam.PlayerUnits) != Relation.Friendly))
            {
                EndCombat();
            }

            FeatureManager.OnUnitKilled(unit);
        }

        public override void OnUnitClicked(Unit unit, MouseButton button)
        {
            base.OnUnitClicked(unit, button);

            if (button == MouseButton.Left)
            {
                if (_selectedAbility == null)
                {
                    if (unit.Selectable && unit.Info.Visible(CurrentTeam))
                    {
                        if (unit.Selected && KeyboardState.IsKeyDown(Keys.LeftControl))
                        {
                            DeselectUnit(unit);
                        }
                        else
                        {
                            SelectUnit(unit);
                        }
                    }
                        
                }
                else
                {
                    _selectedAbility.OnUnitClicked(unit);
                }
            }
            else 
            {
                unit.OnRightClick();

                if (InCombat)
                {
                    EventManager.FireEvent("unit_context_menu_in_combat", unit);
                }
                else
                {
                    EventManager.FireEvent("unit_context_menu_out_combat", unit);
                }
            }
        }

        public virtual void OnAbilityCast(Ability ability) 
        {
            _onAbilityCastActions.ForEach(a => a?.Invoke(ability));

            if(Footer.CurrentUnit == ability.CastingUnit)
            {
                Footer.RefreshFooterInfo(forceUpdate: true);
            }
            else
            {
                Footer.RefreshFooterInfo();
            }
        }

        public List<Action<Ability>> _onSelectAbilityActions = new List<Action<Ability>>();
        public List<Action> _onDeselectAbilityActions = new List<Action>();
        public List<Action<Ability>> _onAbilityCastActions = new List<Action<Ability>>();

        #endregion

        public void OpenContextMenu(Tooltip menu) 
        {
            Task.Run(() =>
            {
                Thread.Sleep(10);
                UIHelpers.CreateContextMenu(this, menu, _tooltipBlock);

                Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
                MessageCenter.SendMessage(msg);
            });
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
                var tuple = TemporaryVision.GetQueuedItems();

                HashSet<UnitTeam> affectedTeams = new HashSet<UnitTeam>();
                HashSet<TileMap> affectedMaps = new HashSet<TileMap>();

                for (int i = 0; i < tuple.itemsToAdd.Count; i++)
                {
                    affectedTeams.Add(tuple.itemsToAdd[i].Team);

                    foreach(var map in tuple.itemsToAdd[i].AffectedMaps)
                    {
                        affectedMaps.Add(map);
                    }
                }

                for (int i = 0; i < tuple.itemsToRemove.Count; i++)
                {
                    affectedTeams.Add(tuple.itemsToRemove[i].Team);

                    foreach (var map in tuple.itemsToRemove[i].AffectedMaps)
                    {
                        affectedMaps.Add(map);
                    }
                }

                TemporaryVision.HandleQueuedItems();

                foreach(var team in affectedTeams)
                {
                    VisionManager.ConsolidateVision(team);
                }

                foreach(var map in affectedMaps)
                {
                    map.UpdateTile(map.Tiles[0]);
                }
            }
        }

        protected override void _onCameraRotate(Camera cam)
        {
            base._onCameraRotate(cam);

            lock (_units._lock)
            {
                foreach(var unit in _units)
                {
                    unit.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(cam.CameraAngle) - unit.BaseObject.BaseFrame.RotationInfo.Z);
                }
            }
        }
    }
}
