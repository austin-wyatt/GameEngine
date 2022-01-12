using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Icon = MortalDungeon.Engine_Classes.UIComponents.Icon;

namespace MortalDungeon.Game.UI
{
    public class GameFooter : Footer
    {
        CombatScene Scene;

        private Text _unitNameTextBox;
        private HealthBar _unitHealthBar;
        private UIBlock _containingBlock;
        private ShieldBar _unitShieldBar;
        private FocusBar _unitFocusBar;

        public EventLog EventLog;

        private UIBlock _infoBlock;

        public Unit _currentUnit;

        public Icon _meditationIcon;

        public Button EndTurnButton;
        public Button VentureForthButton;

        private ScrollableArea _scrollableAreaBuff;
        private ScrollableArea _scrollableAreaAbility;

        private UIBlock _buffBlock;

        public GameFooter(float height, CombatScene scene) : base(height)
        {
            Scene = scene;

            Typeable = true;

            #region end turn button
            Button endTurnButton = new Button(new Vector3(), new UIScale(0.5f, 0.15f), "End Turn", 0.375f, default, default, false);

            EndTurnButton = endTurnButton;

            UIDimensions textOffset = new UIDimensions(80, 100);
            //textOffset.Y = 0;

            //UIScale textScale = endTurnButton.TextBox.TextField.GetDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 3;
            //textScale.Y *= -1;
            endTurnButton.SetSize(endTurnButton.TextBox.Size + textOffset.ToScale());

            endTurnButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -8, 0), UIAnchorPosition.BottomRight);


            endTurnButton.Click += (s, e) =>
            {
                Scene.CompleteTurn();
                //Scene.DeselectUnits();
            };

            AddChild(endTurnButton, 10000);
            #endregion

            #region venture forth button
            Button ventureForthButton = new Button(new Vector3(), new UIScale(0.5f, 0.15f), "Venture Forth", 0.375f, default, default, false);
            ventureForthButton.SetRender(false);

            VentureForthButton = ventureForthButton;

            textOffset = new UIDimensions(80, 100);

            ventureForthButton.SetSize(ventureForthButton.TextBox.Size + textOffset.ToScale());

            ventureForthButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -200, 0), UIAnchorPosition.BottomRight);

            ventureForthButton.Click += (s, e) =>
            {
                if (Scene.CurrentUnit == null)
                    return;

                Scene.DeselectAbility();
                Scene._tileMapController.LoadSurroundingTileMaps(Scene.CurrentUnit.GetTileMap().TileMapCoords);
                ventureForthButton.SetRender(false);

                Scene.EventLog.AddEvent("You venture onwards...", EventSeverity.Positive);
            };

            AddChild(ventureForthButton, 100);
            #endregion

            #region containing blocks

            UIDimensions containingBlockDimensions = Size;
            //containingBlockDimensions.X *= 1.5f;
            containingBlockDimensions.Y -= 20;

            _containingBlock = new UIBlock(Position, containingBlockDimensions);
            _containingBlock.MultiTextureData.MixTexture = true;
            _containingBlock.MultiTextureData.MixPercent = 0.4f;
            //_containingBlock.SetColor(Colors.UISelectedGray);
            _containingBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 1f));
            _containingBlock.SetSize(containingBlockDimensions);
            _containingBlock.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.LeftCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);

            AddChild(_containingBlock, 1);


            Vector3 infoBarPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(20, -4, 0);

            float sizeDiff = WindowConstants.ScreenUnits.X - infoBarPos.X;
            UIDimensions infoBarDimensions = new UIDimensions(sizeDiff * 2 * WindowConstants.AspectRatio, 200);

            _infoBlock = new UIBlock(default, infoBarDimensions);
            _infoBlock.MultiTextureData.MixTexture = false;
            _infoBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 0.3f));
            _infoBlock.SetSize(infoBarDimensions);
            _infoBlock.SetPositionFromAnchor(infoBarPos, UIAnchorPosition.BottomLeft);

            AddChild(_infoBlock, 1000);

            #endregion

            #region name box
            Vector3 nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + containingBlockDimensions.X / 6;

            _unitNameTextBox = new Text("", Text.DEFAULT_FONT, 64, Brushes.Black);
            _unitNameTextBox.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);
            _unitNameTextBox.SetTextScale(0.1f);

            _infoBlock.AddChild(_unitNameTextBox, 100);
            #endregion

            #region health and shield bar
            _unitHealthBar = new HealthBar(new Vector3(), new UIScale(0.4f, 0.075f)) { Hoverable = true, HasTimedHoverEffect = true };

            void healthBarHover(GameObject obj) 
            {
                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, _currentUnit.Info.Health + "/" + _currentUnit.Info.MaxHealth, _unitHealthBar, Scene._tooltipBlock);
                UIHelpers.CreateToolTip(param);
            }

            _unitHealthBar.TimedHover += healthBarHover;

            _infoBlock.AddChild(_unitHealthBar, 100);


            _unitFocusBar = new FocusBar(new Vector3(), new UIScale(0.4f, 0.03f)) { Hoverable = true, HasTimedHoverEffect = true };
            void focusBarHover(GameObject obj)
            {
                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, _currentUnit.Info.Focus + "/" + _currentUnit.Info.MaxFocus, _unitFocusBar, Scene._tooltipBlock);
                UIHelpers.CreateToolTip(param);
            }

            _unitFocusBar.TimedHover += focusBarHover;

            _infoBlock.AddChild(_unitFocusBar, 100);




            _unitShieldBar = new ShieldBar(new Vector3(), new UIScale(0.4f, 0.075f)) { Hoverable = true, HasTimedHoverEffect = true };

            void shieldBarHover(GameObject obj)
            {
                if (_currentUnit.Info.CurrentShields >= 0)
                {
                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, "", _unitShieldBar, Scene._tooltipBlock)
                    {
                        Text = _currentUnit.Info.CurrentShields * _currentUnit.Info.ShieldBlock + " Damage will be blocked from the next attack"
                    };
                    UIHelpers.CreateToolTip(param);
                }
                else 
                {
                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, "", _unitShieldBar, Scene._tooltipBlock)
                    {
                        Text = "Next attack recieved will deal " + _currentUnit.Info.CurrentShields * -1 * 25 + "% more damage"
                    };
                    UIHelpers.CreateToolTip(param);
                }
            }

            _unitShieldBar.TimedHover += shieldBarHover;

            _infoBlock.AddChild(_unitShieldBar, 100);
            #endregion


            _buffBlock = new UIBlock();

            _buffBlock.SetAllInline(0);

            AddChild(_buffBlock);



            EventLog = new EventLog(Scene);

            EventLog.LogArea.SetVisibleAreaPosition(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            AddChild(EventLog.LogArea, 10);

            Scene.EventLog = EventLog;


            //InitializeScrollableAreaBuff();
            //InitializeScrollableAreaAbility();

            UpdateFooterInfo(Scene.CurrentUnit);
        }


        private List<Ability> _currentAbilities = new List<Ability>();
        private List<Buff> _currentBuffs = new List<Buff>();

        private List<Icon> _currentIcons = new List<Icon>();
        private bool _updatingFooterInfo = false;
        private Action _updateAction = null;
        public void UpdateFooterInfo(Unit unit = null, bool setNull = false, bool forceUpdate = false) 
        {
            if (_updatingFooterInfo) 
            {
                _updateAction = () => 
                {
                    _updateAction = null;
                    UpdateFooterInfo(unit);
                };
                return;
            }

            _updatingFooterInfo = true;

            bool refreshingFooter = unit == null;

            if (unit != null) 
            {
                _currentUnit = unit;
            }

            if (_currentUnit == null && !setNull)
            {
                _updatingFooterInfo = false;
                _infoBlock.SetRender(false);
                _buffBlock.SetRender(false);
                return;
            }
            else if(setNull)
            {
                _currentUnit = null;

                for (int i = 0; i < _currentIcons.Count; i++)
                {
                    RemoveChild(_currentIcons[i]);
                }

                _currentIcons.Clear();
                _currentAbilities.Clear();
                _currentBuffs.Clear();

                //_scrollableAreaBuff.SetRender(false);
                _infoBlock.SetRender(false);
                _buffBlock.SetRender(false);
                _updatingFooterInfo = false;
                return;
            }

            _infoBlock.SetRender(true);
            //_scrollableAreaBuff.SetRender(true);
            _buffBlock.SetRender(true);

            bool isPlayerUnitTakingTurn = _currentUnit.AI.ControlType == ControlType.Controlled && (Scene.InCombat ? _currentUnit == Scene.CurrentUnit : true);

            #region unit status box

            float sizeDiff = WindowConstants.ScreenUnits.X - _infoBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter).X;
            UIDimensions infoBarDimensions = new UIDimensions(sizeDiff * 2 * WindowConstants.AspectRatio, 200);
            _infoBlock.SetSize(infoBarDimensions);
            _infoBlock.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(20, -4, 0), UIAnchorPosition.BottomLeft);
            _infoBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 0.75f));


            _unitNameTextBox.SetText(_currentUnit.Name);
            
            
            _unitHealthBar.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(25, -9, 0), UIAnchorPosition.BottomLeft);
            _unitHealthBar.SetHealthPercent(_currentUnit.Info.Health / _currentUnit.Info.MaxHealth, _currentUnit.AI.Team);

            _unitShieldBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(5, 0, 0), UIAnchorPosition.LeftCenter);
            _unitShieldBar.SetCurrentShields(_currentUnit.Info.CurrentShields);

            if (isPlayerUnitTakingTurn)
            {
                _unitHealthBar.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(25, -24, 0), UIAnchorPosition.BottomLeft);
                _unitFocusBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 0, 0), UIAnchorPosition.TopCenter);
                _unitFocusBar.SetFocusPercent(_currentUnit.Info.Focus / _currentUnit.Info.MaxFocus);
                _unitFocusBar.SetRender(true);

                if (Scene.EnergyDisplayBar != null && Scene.ActionEnergyBar != null)
                {
                    Scene.EnergyDisplayBar.SetActiveEnergy(_currentUnit.Info.Energy);
                    Scene.ActionEnergyBar.SetActiveEnergy(_currentUnit.Info.ActionEnergy);
                }
            }
            else
            {
                _unitFocusBar.SetRender(false);
            }
            

            Vector3 nameBoxPos = _infoBlock.GetAnchorPosition(UIAnchorPosition.TopLeft);
            UIDimensions nameBoxDim = _unitNameTextBox.GetDimensions();

            _unitNameTextBox.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, 2, 0), UIAnchorPosition.BottomCenter);

            
            _buffBlock.SetSize(new UIDimensions(infoBarDimensions.X * 0.4f, infoBarDimensions.Y));
            Vector3 buffBlockPos = _infoBlock.GetAnchorPosition(UIAnchorPosition.TopLeft);
            _buffBlock.SetPositionFromAnchor(buffBlockPos, UIAnchorPosition.BottomLeft);
            _buffBlock.SetColor(new Vector4(0, 0, 0, 0));

            #endregion
           

            VentureForthButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -102, 0), UIAnchorPosition.BottomRight);


            if (!Enumerable.SequenceEqual(_currentAbilities, _currentUnit.Info.Abilities) || forceUpdate)
            {
                _currentAbilities.Clear();
                CreateAbilityIcons(isPlayerUnitTakingTurn);
            }

            if (!Enumerable.SequenceEqual(_currentBuffs, _currentUnit.Info.Buffs) || forceUpdate)
            {
                _currentBuffs.Clear();
                CreateBuffIcons(isPlayerUnitTakingTurn);
            }

            ForceTreeRegeneration();

            _updatingFooterInfo = false;
            if (_updateAction != null) 
            {
                _updateAction.Invoke();
            }
        }


        public override void OnResize()
        {
            base.OnResize();

            //RemoveChild(_scrollableAreaBuff);
            //InitializeScrollableAreaBuff();
            //InitializeScrollableAreaAbility();
        }

        private void InitializeScrollableAreaBuff() 
        {
            if(_scrollableAreaBuff != null)
                RemoveChild(_scrollableAreaBuff);

            UIScale scrollableAreaSize = new UIScale(_containingBlock.Size);
            scrollableAreaSize.X /= 3.3f;
            scrollableAreaSize.Y -= .02f;

            //_buffContainer = new UIBlock(new Vector3(), scrollableAreaSize);
            _scrollableAreaBuff = new ScrollableArea(new Vector3(), scrollableAreaSize, new Vector3(), new UIScale(scrollableAreaSize.X, scrollableAreaSize.Y), 0.05f);

            float scrollbarWidth = 0;
            if (_scrollableAreaBuff.Scrollbar != null)
            {
                scrollbarWidth = _scrollableAreaBuff.Scrollbar.GetDimensions().X;
            }

            _scrollableAreaBuff.SetVisibleAreaPosition(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-3 - scrollbarWidth, 5, 0), UIAnchorPosition.TopRight);
            _scrollableAreaBuff.BaseComponent.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            _scrollableAreaBuff.BaseComponent.SetColor(new Vector4(0, 0, 0, 0));

            AddChild(_scrollableAreaBuff, 1000);
        }

        private void InitializeScrollableAreaAbility()
        {
            //if (_scrollableAreaAbility != null)
            //    RemoveChild(_scrollableAreaAbility);

            //UIScale scrollableAreaSize = new UIScale(_containingBlock.Size);
            ////scrollableAreaSize.X /= 2f;
            //scrollableAreaSize.Y -= .02f;

            ////_buffContainer = new UIBlock(new Vector3(), scrollableAreaSize);
            //_scrollableAreaAbility = new ScrollableArea(new Vector3(), scrollableAreaSize, new Vector3(), new UIScale(scrollableAreaSize.X, scrollableAreaSize.Y), 0.05f);

            

            //float scrollbarWidth = 0;
            //if (_scrollableAreaAbility.Scrollbar != null)
            //{
            //    scrollbarWidth = _scrollableAreaAbility.Scrollbar.GetDimensions().X;
            //}

            //_scrollableAreaAbility.SetVisibleAreaPosition(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-3 - scrollbarWidth, 5, 0), UIAnchorPosition.TopLeft);
            //_scrollableAreaAbility.BaseComponent.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopLeft);


            //_scrollableAreaAbility.BaseComponent.SetColor(new Vector4(1, 0, 0, 1f));

            //AddChild(_scrollableAreaAbility, 1500);
        }


        private Dictionary<int, Action<int>> _selectAbilityByNumList = new Dictionary<int, Action<int>>();
        private void CreateAbilityIcons(bool isPlayerUnitTakingTurn) 
        {
            for (int i = 0; i < _currentIcons.Count; i++)
            {
                RemoveChild(_currentIcons[i]);
            }

            _currentIcons.Clear();
            _selectAbilityByNumList.Clear();


            UIScale iconSize = new UIScale(0.25f, 0.25f);
            int count = 0;


            lock (_currentUnit.Info.Abilities)
            {
                foreach (Ability ability in _currentUnit.Info.Abilities)
                {
                    _currentAbilities.Add(ability);
                    string hotkey = null;
                    if (_currentUnit.AI.ControlType == ControlType.Controlled)
                    {
                        hotkey = (count + 1).ToString();
                    }

                    Icon abilityIcon = ability.GenerateIcon(iconSize, true,
                        _currentUnit.AI.Team == UnitTeam.PlayerUnits ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                        false, null, isPlayerUnitTakingTurn && ability.CanCast() ? hotkey : null, showCharges: true);

                    int currIndex = count;

                    abilityIcon.DisabledColor = _Colors.IconDisabled;
                    abilityIcon.SelectedColor = _Colors.IconSelected;
                    abilityIcon.HoverColor = _Colors.IconHover;

                    if (_currentIcons.Count == 0)
                    {
                        abilityIcon.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(20, 0, 0), UIAnchorPosition.LeftCenter);
                    }
                    else
                    {
                        abilityIcon.SetPositionFromAnchor(
                            _currentIcons[_currentIcons.Count - 1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                    }

                    void checkAbilityClickable()
                    {
                        if (isPlayerUnitTakingTurn && ability.CanCast())
                        {
                            abilityIcon.Clickable = true;
                            abilityIcon.Hoverable = true;
                        }
                        else
                        {
                            //abilityIcon.Clickable = false;
                            //abilityIcon.SetDisabled(true);
                            abilityIcon.OnDisabled(true);
                        }
                    }

                    checkAbilityClickable();

                    abilityIcon.Click += (s, e) =>
                    {
                        if (isPlayerUnitTakingTurn && ability.CanCast())
                        {
                            Scene.SelectAbility(ability, _currentUnit);
                        }
                    };


                    void onAbilitySelected(Ability selectedAbility)
                    {
                        if (selectedAbility.AbilityID == ability.AbilityID)
                        {
                            abilityIcon.OnSelect(true);

                            Sound sound = new Sound(Sounds.Select) { Gain = 0.1f, Pitch = 0.5f + currIndex * 0.05f };
                            sound.Play();
                        }
                    }

                    void onAbilityDeselected()
                    {
                        abilityIcon.OnSelect(false);

                        if (!isPlayerUnitTakingTurn || !ability.CanCast())
                        {
                            //abilityIcon.Clickable = false;
                            abilityIcon.SetDisabled(true);
                        }
                    }


                    Scene._onSelectAbilityActions.Add(onAbilitySelected);
                    Scene._onDeselectAbilityActions.Add(onAbilityDeselected);
                    //Scene._onAbilityCastActions.Add(onAbilityCast);

                    void selectAbilityByNum(int numPressed)
                    {
                        if (numPressed == currIndex && isPlayerUnitTakingTurn && ability.CanCast() && _currentAbilities.Count > 0)
                        {
                            Scene.SelectAbility(ability, _currentUnit);
                        }

                        if (numPressed == currIndex && !isPlayerUnitTakingTurn && Scene.CurrentUnit != null
                            && Scene.CurrentUnit.AI.ControlType == ControlType.Controlled)
                        {
                            UpdateFooterInfo(Scene.CurrentUnit);
                        }
                    }

                    _selectAbilityByNumList.Add(currIndex, selectAbilityByNum);




                    void cleanUp(GameObject obj)
                    {
                        Scene._onSelectAbilityActions.Remove(onAbilitySelected);
                        Scene._onDeselectAbilityActions.Remove(onAbilityDeselected);
                        //Scene._onAbilityCastActions.Remove(onAbilityCast);

                        abilityIcon.OnCleanUp -= cleanUp;
                    }
                    abilityIcon.OnCleanUp += cleanUp;

                    void abilityHover(GameObject obj)
                    {
                        UIHelpers.CreateToolTip(Scene, ability.GenerateTooltip(), abilityIcon, Scene._tooltipBlock);
                    }

                    abilityIcon.HasTimedHoverEffect = true;
                    abilityIcon.Hoverable = true;
                    abilityIcon.TimedHover += abilityHover;

                    abilityIcon.Name = ability.Name + " Icon";

                    abilityIcon.Name = "Icon " + count;

                    _currentIcons.Add(abilityIcon);
                    AddChild(abilityIcon, 100);

                    count++;
                }
            }
        }

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            int numPressed = (int)e.Key - (int)Keys.D1;

            if(numPressed < _selectAbilityByNumList.Count && numPressed >= 0)
            {
                _selectAbilityByNumList[numPressed].Invoke(numPressed);
            }
        }

        private void CreateBuffIcons(bool isPlayerUnitTakingTurn) 
        {
            _buffBlock.RemoveChildren();

            UIScale buffSize = new UIScale(0.09f, 0.09f);

            List<Icon> icons = new List<Icon>();
            int count = 0;
            int delimiter = -1;
            foreach (Buff buff in _currentUnit.Info.Buffs)
            {
                if (buff.Hidden)
                    continue;

                Icon icon = buff.GenerateIcon(buffSize, true);
                icons.Add(icon);

                if (count == 0)
                {
                    icon.SetPositionFromAnchor(_buffBlock.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(10, -10, 0), UIAnchorPosition.BottomLeft);
                }
                else if (count % 5 == 0)
                {
                    icon.SetPositionFromAnchor(icons[count - 5].GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(0, -10, 0), UIAnchorPosition.BottomLeft);
                }
                else
                {
                    icon.SetPositionFromAnchor(icons[count - 1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                }

                //if (icon.GetAnchorPosition(UIAnchorPosition.RightCenter).X > _scrollableAreaBuff.VisibleArea.GetAnchorPosition(UIAnchorPosition.RightCenter).X)
                //{
                //    if (delimiter == -1)
                //    {
                //        delimiter = count;
                //    }
                //    icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);

                //    if (icon.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y > _scrollableAreaBuff.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y)
                //    {
                //        _scrollableAreaBuff.SetBaseAreaSize(new UIScale(_scrollableAreaBuff._baseAreaSize.X, _scrollableAreaBuff._baseAreaSize.Y + icon.GetDimensions().ToScale().Y * 3));

                //        icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);
                //    }

                //    //_scrollableArea.BaseComponent.SetSize(_scrollableArea._baseAreaSize);
                //}

                void buffHover(GameObject obj)
                {
                    UIHelpers.CreateToolTip(Scene, buff.GenerateTooltip(), icon, Scene._tooltipBlock);
                }

                icon.HasTimedHoverEffect = true;
                icon.Hoverable = true;
                icon.TimedHover += buffHover;


                //_scrollableAreaBuff.BaseComponent.AddChild(icon, 1000);
                _buffBlock.AddChild(icon, 1000);
                count++;
            }
        }

        private void CreateMeditationIcon() 
        {
            if (_meditationIcon != null) 
            {
                RemoveMeditationIcon();
            }

            _meditationIcon = new Icon(new UIScale(0.13f, 0.13f), IconSheetIcons.MonkBig, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);

            _meditationIcon.HoverColor = _Colors.IconHover;

            UIHelpers.AddTimedHoverTooltip(_meditationIcon, "Meditate to regain ability uses.", Scene);

            _meditationIcon.Hoverable = true;
            _meditationIcon.Clickable = true;

            _meditationIcon.Click += (s, e) =>
            {
                CreateMeditationWindow();

                UpdateFooterInfo();
            };

            _meditationIcon.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(10, -GetDimensions().Y / 2, 0), UIAnchorPosition.TopLeft);
            AddChild(_meditationIcon, 1000);
        }

        private void RemoveMeditationIcon()
        {
            if (_meditationIcon != null) 
            {
                RemoveChild(_meditationIcon);
                UIHelpers.NukeTooltips(GeneralContextFlags.UITooltipOpen, Scene);
            }
        }

        private void CreateMeditationWindow() 
        {
            UIObject window = UIHelpers.CreateWindow(new UIScale(0.75f, 0.6f), "meditation_window", this, Scene, true);

            window.SetPosition(WindowConstants.CenterScreen);

            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(0.5f, 0.6f), default, new UIScale(0.5f, 1), 0.05f);

            scrollableArea.SetVisibleAreaPosition(window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            UIList uiList = new UIList(default, new UIScale(0.5f, 0.05f), 0.075f);

            foreach (var ability in _currentUnit.Info.Abilities) 
            {
                ListItem listItem = uiList.AddItem(ability.Name, (_) =>
                {
                    if (ability.GetCharges() < ability.MaxCharges && ability.CanRecharge()) 
                    {
                        ability.ApplyChargeRechargeCost();

                        ability.RestoreCharges(1);
                        UpdateFooterInfo();
                    }
                });

                TextComponent focusCostAdornment = new TextComponent();
                focusCostAdornment.SetTextScale(0.03f);
                focusCostAdornment.SetText(ability.ChargeRechargeCost.ToString());
                focusCostAdornment.SetColor(_Colors.UITextBlack);

                focusCostAdornment.SetPositionFromAnchor(listItem.BaseComponent.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(-10, 0, 0), UIAnchorPosition.RightCenter);

                listItem.BaseComponent.AddChild(focusCostAdornment);
            }

            uiList.SetPositionFromAnchor(scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);

            scrollableArea.BaseComponent.AddChild(uiList);
            window.AddChild(scrollableArea);

            AddChild(window);
        }
    }
}
