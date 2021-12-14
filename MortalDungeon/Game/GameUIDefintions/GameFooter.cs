using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class GameFooter : Footer
    {
        CombatScene Scene;

        private TextComponent _unitNameTextBox;
        private HealthBar _unitHealthBar;
        private UIBlock _containingBlock;
        private ShieldBar _unitShieldBar;
        private FocusBar _unitFocusBar;

        public Unit _currentUnit;

        public Icon _meditationIcon;

        public Button EndTurnButton;
        public Button VentureForthButton;

        private ScrollableArea _scrollableAreaBuff;
        private ScrollableArea _scrollableAreaAbility;

        public GameFooter(float height, CombatScene scene) : base(height)
        {
            Scene = scene;


            #region ability button
            //ToggleableButton abilityButton = new ToggleableButton(Position + new Vector3(-GetDimensions().X / 2 + 30, 0, 0), new UIScale(0.15f, 0.1f), "^", 0.1f);


            //abilityButton.OnSelectAction = () =>
            //{
            //    UIDimensions buttonDimensions = abilityButton.GetDimensions();

            //    Vector3 dim = abilityButton.GetAnchorPosition(UIAnchorPosition.TopLeft);

            //    UIList abilityList = new UIList(dim, new UIScale(0.4f, 0.15f), 0.05f) { Ascending = true };

            //    foreach (Ability ability in scene.CurrentUnit.Abilities.Values)
            //    {
            //        ListItem newItem = abilityList.AddItem(ability.Name, () =>
            //        {
            //            scene.DeselectAbility();
            //            scene.SelectAbility(ability);
            //            abilityButton.OnMouseUp();
            //        });

            //        if (ability.GetEnergyCost() > scene.EnergyDisplayBar.CurrentEnergy)
            //        {
            //            newItem.SetDisabled(true);
            //        }

            //        UIDimensions itemDim = newItem.GetDimensions();
            //        itemDim.Y *= 1.5f;
            //        itemDim.X = itemDim.Y;

            //        Icon icon = ability.GenerateIcon(itemDim, true, Icon.BackgroundType.BuffBackground);
            //        icon.Clickable = true;

            //        icon.SetPositionFromAnchor(newItem.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(-2, 0, 0), UIAnchorPosition.RightCenter);
            //        newItem.AddChild(icon, 1000);
            //    }

            //    abilityButton.AddChild(abilityList);

            //    abilityList.Anchor = UIAnchorPosition.BottomLeft;
            //    abilityList.SetPositionFromAnchor(dim);
            //};

            //abilityButton.OnDeselectAction = () =>
            //{
            //    List<int> childIDs = new List<int>();
            //    abilityButton.Children.ForEach(child =>
            //    {
            //        if (child != abilityButton.BaseComponent)
            //        {
            //            childIDs.Add(child.ObjectID);
            //        }
            //    });

            //    abilityButton.RemoveChildren(childIDs);
            //};

            //AddChild(abilityButton, 100);

            #endregion

            #region end turn button
            Button endTurnButton = new Button(new Vector3(), new UIScale(0.5f, 0.15f), "End Turn", 0.075f, default, default, false);

            EndTurnButton = endTurnButton;

            UIDimensions textOffset = new UIDimensions(80, 100);
            //textOffset.Y = 0;

            //UIScale textScale = endTurnButton.TextBox.TextField.GetDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 3;
            //textScale.Y *= -1;
            endTurnButton.SetSize(endTurnButton.TextBox.GetScale() + textOffset.ToScale());

            endTurnButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -10, 0), UIAnchorPosition.BottomRight);


            endTurnButton.OnClickAction = () =>
            {
                Scene.CompleteTurn();
                //Scene.DeselectUnits();
            };

            AddChild(endTurnButton, 100);
            #endregion

            #region venture forth button
            Button ventureForthButton = new Button(new Vector3(), new UIScale(0.5f, 0.15f), "Venture Forth", 0.075f, default, default, false);

            VentureForthButton = ventureForthButton;

            textOffset = new UIDimensions(80, 100);

            ventureForthButton.SetSize(ventureForthButton.TextBox.GetScale() + textOffset.ToScale());

            ventureForthButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -10, 0), UIAnchorPosition.BottomRight);

            ventureForthButton.OnClickAction = () =>
            {
                Scene._tileMapController.LoadSurroundingTileMaps(Scene.CurrentUnit.GetTileMap().TileMapCoords);
                ventureForthButton.SetRender(false);
            };

            AddChild(ventureForthButton, 100);
            #endregion

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

            #region name box
            Vector3 nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + containingBlockDimensions.X / 6;

            _unitNameTextBox = new TextComponent();
            _unitNameTextBox.SetColor(Colors.UITextBlack);
            _unitNameTextBox.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);
            _unitNameTextBox.SetTextScale(0.075f);

            _containingBlock.AddChild(_unitNameTextBox, 100);
            #endregion

            #region health and shield bar
            _unitHealthBar = new HealthBar(new Vector3(), new UIScale(0.5f, 0.1f)) { Hoverable = true, HasTimedHoverEffect = true };

            void healthBarHover(GameObject obj) 
            {
                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, _currentUnit.Info.Health + "/" + _currentUnit.Info.MaxHealth, _unitHealthBar, Scene._tooltipBlock);
                UIHelpers.CreateToolTip(param);
            }

            _unitHealthBar.OnTimedHoverEvent += healthBarHover;

            _containingBlock.AddChild(_unitHealthBar, 100);


            _unitFocusBar = new FocusBar(new Vector3(), new UIScale(0.25f, 0.05f)) { Hoverable = true, HasTimedHoverEffect = true };
            void focusBarHover(GameObject obj)
            {
                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, _currentUnit.Info.Focus + "/" + _currentUnit.Info.MaxFocus, _unitFocusBar, Scene._tooltipBlock);
                UIHelpers.CreateToolTip(param);
            }

            _unitFocusBar.OnTimedHoverEvent += focusBarHover;

            _containingBlock.AddChild(_unitFocusBar, 100);




            _unitShieldBar = new ShieldBar(new Vector3(), new UIScale(0.5f, 0.1f)) { Hoverable = true, HasTimedHoverEffect = true };

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

            _unitShieldBar.OnTimedHoverEvent += shieldBarHover;

            _containingBlock.AddChild(_unitShieldBar, 100);
            #endregion

            InitializeScrollableAreaBuff();
            InitializeScrollableAreaAbility();

            UpdateFooterInfo(Scene.CurrentUnit);
        }

        private List<Icon> _currentIcons = new List<Icon>();
        private bool _updatingFooterInfo = false;
        private Action _updateAction = null;
        public void UpdateFooterInfo(Unit unit = null) 
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

            if (unit != null) 
            {
                _currentUnit = unit;
            }

            if (_currentUnit == null)
                return;

            bool isPlayerUnitTakingTurn = _currentUnit.AI.ControlType == ControlType.Controlled && (Scene.InCombat ? _currentUnit == Scene.CurrentUnit : true);

            #region unit status box
            _unitNameTextBox.SetText(_currentUnit.Name);
            
            
            Vector3 nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + _containingBlock.GetDimensions().X / 6;
            nameBoxPos.Y = nameBoxPos.Y - _containingBlock.GetDimensions().Y / 4;

            _unitNameTextBox.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);

            nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + (_containingBlock.GetDimensions().X / 6) * 3;
            nameBoxPos.Y = nameBoxPos.Y - _containingBlock.GetDimensions().Y / 4;

            _unitHealthBar.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);
            _unitHealthBar.SetHealthPercent(_currentUnit.Info.Health / _currentUnit.Info.MaxHealth, _currentUnit.AI.Team);

            _unitShieldBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitShieldBar.SetCurrentShields(_currentUnit.Info.CurrentShields);
            #endregion

            #region ability icons
            for (int i = 0; i < _currentIcons.Count; i++) 
            {
                RemoveChild(_currentIcons[i].ObjectID);
            }

            _currentIcons.Clear();


            if (isPlayerUnitTakingTurn)
            {
                CreateMeditationIcon();

                _unitFocusBar.SetPositionFromAnchor(_meditationIcon.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                _unitFocusBar.SetFocusPercent(_currentUnit.Info.Focus / _currentUnit.Info.MaxFocus);
                _unitFocusBar.SetRender(true);
            }
            else 
            {
                RemoveMeditationIcon();
                _unitFocusBar.SetRender(false);
            }


            UIScale iconSize = new UIScale(0.25f, 0.25f);
            int count = 0;


            lock (_currentUnit.Info.Abilities)
            {
                foreach (Ability ability in _currentUnit.Info.Abilities)
                {
                    string hotkey = null;
                    if (_currentUnit.AI.ControlType == ControlType.Controlled) 
                    {
                        hotkey = (count + 1).ToString();
                    }

                    Icon abilityIcon = ability.GenerateIcon(iconSize, true, 
                        _currentUnit.AI.Team == UnitTeam.PlayerUnits ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground, 
                        false, null, hotkey, ability.GetMaxCharges() > 0);

                    int currIndex = count;

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
                            abilityIcon.Clickable = false;
                            abilityIcon.SetColor(Colors.IconDisabled);
                        }
                    }

                    checkAbilityClickable();

                    abilityIcon.OnClickAction = () =>
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
                            abilityIcon.SetColor(Colors.IconSelected);

                            Sound sound = new Sound(Sounds.Select) { Gain = 0.1f, Pitch = 0.5f + currIndex * 0.05f };
                            sound.Play();
                        }
                    }

                    void onAbilityDeselected()
                    {
                        if (isPlayerUnitTakingTurn && ability.CanCast())
                        {
                            abilityIcon.SetColor(Colors.White);
                        }
                        else
                        {
                            abilityIcon.Clickable = false;
                            abilityIcon.SetColor(Colors.IconDisabled);
                        }
                    }

                    void onAbilityCast(Ability castAbility)
                    {
                        //if (Scene.EnergyDisplayBar != null && !ability.GetEnergyIsSufficient())
                        //{
                        //    abilityIcon.Clickable = false;
                        //    abilityIcon.SetColor(Colors.IconDisabled);
                        //}
                    }

                    Scene._onSelectAbilityActions.Add(onAbilitySelected);
                    Scene._onDeselectAbilityActions.Add(onAbilityDeselected);
                    Scene._onAbilityCastActions.Add(onAbilityCast);

                    void selectAbilityByNum(SceneEventArgs args) 
                    {
                        if ((int)args.EventAction == currIndex && _currentUnit.AI.ControlType == ControlType.Controlled)
                        {
                            Scene.SelectAbility(ability, _currentUnit);
                        }
                    }

                    Scene.OnNumberPressed += selectAbilityByNum;


                    UIHelpers.AddAbilityIconHoverEffect(abilityIcon, Scene, ability);

                    void cleanUp(GameObject obj) 
                    {
                        Scene._onSelectAbilityActions.Remove(onAbilitySelected);
                        Scene._onDeselectAbilityActions.Remove(onAbilityDeselected);
                        Scene._onAbilityCastActions.Remove(onAbilityCast);

                        Scene.OnNumberPressed -= selectAbilityByNum;
                        abilityIcon.OnCleanUp -= cleanUp;
                    }
                    abilityIcon.OnCleanUp += cleanUp;

                    void abilityHover(GameObject obj)
                    {
                        UIHelpers.CreateToolTip(Scene, ability.GenerateTooltip(), abilityIcon, Scene._tooltipBlock);
                    }

                    abilityIcon.HasTimedHoverEffect = true;
                    abilityIcon.Hoverable = true;
                    abilityIcon.OnTimedHoverEvent += abilityHover;

                    abilityIcon.Name = ability.Name + " Icon";

                    _currentIcons.Add(abilityIcon);
                    AddChild(abilityIcon, 100);

                    count++;
                }
            }
            #endregion


            #region buff icons
            _scrollableAreaBuff.BaseComponent.RemoveChildren();

            UIScale buffSize = new UIScale(0.09f, 0.09f);

            List<Icon> icons = new List<Icon>();
            count = 0;
            int delimiter = -1;
            _scrollableAreaBuff.SetBaseAreaSize(new UIScale(_scrollableAreaBuff.Size.X, _scrollableAreaBuff.Size.Y));
            foreach (Buff buff in _currentUnit.Info.Buffs) 
            {
                if (buff.Hidden)
                    continue;

                Icon icon = buff.GenerateIcon(buffSize);
                icons.Add(icon);

                if (count == 0)
                {
                    icon.SetPositionFromAnchor(_scrollableAreaBuff.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
                }
                else 
                {
                    icon.SetPositionFromAnchor(icons[count - 1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                }

                if (icon.GetAnchorPosition(UIAnchorPosition.RightCenter).X > _scrollableAreaBuff.VisibleArea.GetAnchorPosition(UIAnchorPosition.RightCenter).X) 
                {
                    if (delimiter == -1) 
                    {
                        delimiter = count;
                    }
                    icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);

                    if (icon.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y > _scrollableAreaBuff.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y) 
                    {
                        _scrollableAreaBuff.SetBaseAreaSize(new UIScale(_scrollableAreaBuff._baseAreaSize.X, _scrollableAreaBuff._baseAreaSize.Y + icon.GetDimensions().ToScale().Y * 3));

                        icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);
                    }

                    //_scrollableArea.BaseComponent.SetSize(_scrollableArea._baseAreaSize);
                }

                void buffHover(GameObject obj) 
                {
                    UIHelpers.CreateToolTip(Scene, buff.GenerateTooltip(), icon, Scene._tooltipBlock);
                }

                icon.HasTimedHoverEffect = true;
                icon.Hoverable = true;
                icon.OnTimedHoverEvent += buffHover;


                _scrollableAreaBuff.BaseComponent.AddChild(icon, 1000);
                count++;
            }

            #endregion

            _updatingFooterInfo = false;
            if (_updateAction != null) 
            {
                _updateAction.Invoke();
            }
        }


        public override void OnResize()
        {
            base.OnResize();

            RemoveChild(_scrollableAreaBuff);
            InitializeScrollableAreaBuff();
            InitializeScrollableAreaAbility();
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

        private void CreateMeditationIcon() 
        {
            if (_meditationIcon != null) 
            {
                RemoveMeditationIcon();
            }

            _meditationIcon = new Icon(new UIScale(0.13f, 0.13f), IconSheetIcons.MonkBig, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);

            UIHelpers.AddAbilityIconHoverEffect(_meditationIcon, Scene);

            UIHelpers.AddTimedHoverTooltip(_meditationIcon, "Meditate to regain ability uses.", Scene);

            _meditationIcon.Hoverable = true;
            _meditationIcon.Clickable = true;

            _meditationIcon.OnClickAction = () =>
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

            UIList uiList = new UIList(default, new UIScale(0.5f, 0.05f), 0.03f);

            foreach (var ability in _currentUnit.Info.Abilities) 
            {
                ListItem listItem = uiList.AddItem(ability.Name, (_) =>
                {
                    if (ability.Charges < ability.MaxCharges && (ability.CastingUnit.Info.Focus >= ability.RechargeCost || ability.CastingUnit.Info.Health >= ability.RechargeCost)) 
                    {
                        if (ability.CastingUnit.Info.Focus >= ability.RechargeCost)
                        {
                            ability.CastingUnit.Info.Focus -= ability.RechargeCost;
                        }
                        else 
                        {
                            DamageInstance damage = new DamageInstance();
                            damage.Damage.Add(DamageType.Focus, ability.RechargeCost);

                            ability.CastingUnit.ApplyDamage(new Unit.DamageParams(damage));
                        }

                        ability.RestoreCharges(1);
                        UpdateFooterInfo();
                    }
                });

                TextComponent focusCostAdornment = new TextComponent();
                focusCostAdornment.SetTextScale(0.03f);
                focusCostAdornment.SetText(ability.RechargeCost.ToString());
                focusCostAdornment.SetColor(Colors.UITextBlack);

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
