using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.TextHandling;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityDefinitions;
using Empyrean.Game.Player;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Icon = Empyrean.Engine_Classes.UIComponents.Icon;

namespace Empyrean.Game.UI
{
    public enum FooterMode
    {
        SingleUnit,
        MultiUnit,
        Group
    }

    public class GameFooter : Footer
    {
        CombatScene Scene;


        public Unit LastSelectedControllableUnit = null;

        private Text _unitNameTextBox;
        private HealthBar _unitHealthBar;
        private UIBlock _generalBlock;
        private UIBlock _itemBlock;
        private ShieldBar _unitShieldBar;
        public StaminaBar _unitStaminaBar;

        private UIBlock _infoBlock;

        public Unit CurrentUnit;
        public FooterMode CurrentFooterMode;

        public Icon _meditationIcon;

        public Button EndTurnButton;

        private ScrollableArea _scrollableAreaBuff;

        private UIBlock _buffBlock;

        public GameFooter(float height, CombatScene scene) : base(height)
        {
            Scene = scene;

            Typeable = true;

            #region end turn button
            Button endTurnButton = new Button(new Vector3(), new UIScale(0.5f, 0.15f), "End Turn", centerText: true, fontSize: 28);

            EndTurnButton = endTurnButton;

            endTurnButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -8, 0), UIAnchorPosition.BottomRight);


            endTurnButton.Click += (s, e) =>
            {
                Scene.CompleteTurn();
                //Scene.DeselectUnits();
            };

            AddChild(endTurnButton, 10000);
            #endregion


            #region containing blocks

            UIDimensions containingBlockDimensions = Size;
            //containingBlockDimensions.X *= 1.5f;
            containingBlockDimensions.Y -= 20;
            containingBlockDimensions.X *= 0.5f;

            _generalBlock = new UIBlock(Position, containingBlockDimensions);
            _generalBlock.MultiTextureData.MixTexture = true;
            _generalBlock.MultiTextureData.MixPercent = 0.4f;
            //_containingBlock.SetColor(Colors.UISelectedGray);
            _generalBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 1f));
            _generalBlock.SetSize(containingBlockDimensions);
            _generalBlock.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.LeftCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);

            AddChild(_generalBlock, 2);

            _itemBlock = new UIBlock();
            _itemBlock.SetAllInline(0);
            _itemBlock.SetColor(_Colors.Transparent);
            AddChild(_itemBlock, 2);

            Vector3 infoBarPos = _generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(20, -4, 0);

            float sizeDiff = WindowConstants.ScreenUnits.X - infoBarPos.X;
            UIDimensions infoBarDimensions = new UIDimensions(sizeDiff * 2 * WindowConstants.AspectRatio, 200);

            _infoBlock = new UIBlock(new Vector3(), infoBarDimensions);
            _infoBlock.MultiTextureData.MixTexture = false;
            _infoBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 0.3f));
            _infoBlock.SetSize(infoBarDimensions);
            _infoBlock.SetPositionFromAnchor(infoBarPos, UIAnchorPosition.BottomLeft);

            AddChild(_infoBlock, 1000);

            #endregion

            #region name box
            Vector3 nameBoxPos = _generalBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + containingBlockDimensions.X / 6;
            _unitNameTextBox = new Text("", Text.DEFAULT_FONT, 18, Brushes.Black, Color.FromArgb(114, 130, 163));
            _unitNameTextBox.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);

            _infoBlock.AddChild(_unitNameTextBox, 100);
            #endregion

            #region health and shield bar
            _unitHealthBar = new HealthBar(new Vector3(), new UIScale(0.4f, 0.075f)) { Hoverable = true, HasTimedHoverEffect = true };

            void healthBarHover(GameObject obj)
            {
                if(CurrentUnit != null)
                {
                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, CurrentUnit.GetResF(ResF.Health) + 
                        "/" + CurrentUnit.GetResF(ResF.MaxHealth), _unitHealthBar, Scene._tooltipBlock);
                    UIHelpers.CreateToolTip(param);
                }
            }

            _unitHealthBar.TimedHover += healthBarHover;

            _infoBlock.AddChild(_unitHealthBar, 100);


            _unitStaminaBar = new StaminaBar(new Vector3(), new UIScale(0.4f, 0.03f)) { Hoverable = true };
            //_unitStaminaBar = new StaminaBar(new Vector3(), new UIScale(0.4f, 0.03f));
            void staminaBarHover(GameObject obj)
            {
                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, CurrentUnit.GetResI(ResI.Stamina) + "/" + 
                    CurrentUnit.GetResI(ResI.MaxStamina), obj, Scene._tooltipBlock);
                UIHelpers.CreateToolTip(param);
            }

            _unitStaminaBar.TimedHover += staminaBarHover;

            _infoBlock.AddChild(_unitStaminaBar, 100);




            _unitShieldBar = new ShieldBar(new Vector3(), new UIScale(0.4f, 0.075f)) { Hoverable = true, HasTimedHoverEffect = true };

            void shieldBarHover(GameObject obj)
            {
                if (CurrentUnit.GetResI(ResI.Shields) >= 0)
                {
                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, "", _unitShieldBar, Scene._tooltipBlock)
                    {
                        Text = CurrentUnit.GetResI(ResI.Shields) * CurrentUnit.GetResF(ResF.ShieldBlock) + " Damage will be blocked from the next attack"
                    };
                    UIHelpers.CreateToolTip(param);
                }
                else 
                {
                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, "", _unitShieldBar, Scene._tooltipBlock)
                    {
                        Text = "Next attack recieved will deal " + CurrentUnit.GetResI(ResI.Shields) * -1 * 25 + "% more damage"
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


            //InitializeScrollableAreaBuff();
            //InitializeScrollableAreaAbility();

            UpdateFooterInfo(Scene.CurrentUnit);
        }

        private void ToggleUnitInfo(bool render)
        {
            _infoBlock.SetRender(render);
            _buffBlock.SetRender(render);
            _itemBlock.SetRender(render);
        }


        public void RefreshFooterInfo(bool forceUpdate = false)
        {
            UpdateFooterInfo(CurrentUnit, forceUpdate: forceUpdate, footerMode: CurrentFooterMode);
        }


        private List<Ability> _currentAbilities = new List<Ability>();
        private List<Buff> _currentBuffs = new List<Buff>();

        private List<Icon> _currentIcons = new List<Icon>();
        private bool _updatingFooterInfo = false;
        private Action _updateAction = null;
        public void UpdateFooterInfo(Unit unit = null, bool setNull = false, bool forceUpdate = false, FooterMode footerMode = FooterMode.SingleUnit) 
        {
            if (Scene.ContextManager.GetFlag(GeneralContextFlags.DisallowFooterUpdate))
                return;

            if (_updatingFooterInfo) 
            {
                _updateAction = () => 
                {
                    _updateAction = null;
                    Scene.QueueToRenderCycle(() =>
                    {
                        UpdateFooterInfo(unit, setNull, forceUpdate, footerMode);
                    });
                };
                return;
            }
            

            Scene.QueueToRenderCycle(() =>
            {
                if (Scene.ContextManager.GetFlag(GeneralContextFlags.DisallowFooterUpdate))
                    return;

                _updatingFooterInfo = true;

                CurrentFooterMode = footerMode;

                if (unit != null)
                {
                    CurrentUnit = unit;
                }

                if(unit != null && !unit.EntityHandle.Loaded || (CurrentUnit != null && !CurrentUnit.EntityHandle.Loaded))
                {
                    setNull = true;
                }

                if (CurrentUnit == null && !setNull)
                {
                    _updatingFooterInfo = false;
                    ToggleUnitInfo(false);
                    return;
                }
                else if (setNull)
                {
                    CurrentUnit = null;

                    for (int i = 0; i < _currentIcons.Count; i++)
                    {
                        RemoveChild(_currentIcons[i]);
                    }

                    _currentIcons.Clear();
                    _currentAbilities.Clear();
                    _currentBuffs.Clear();

                    _generalBlock.RemoveChildren();
                    _itemBlock.RemoveChildren();

                    _selectAbilityByHotkeyList.Clear();

                    //_scrollableAreaBuff.SetRender(false);
                    ToggleUnitInfo(false);
                    _updatingFooterInfo = false;
                    return;
                }

                if (Scene.InCombat)
                    footerMode = FooterMode.SingleUnit;


                if (CurrentUnit.AI.GetControlType() == ControlType.Controlled)
                {
                    LastSelectedControllableUnit = CurrentUnit;
                }

                switch (footerMode)
                {
                    case FooterMode.SingleUnit:
                        PopulateSingleUnitFooter(forceUpdate);
                        break;
                    case FooterMode.MultiUnit:
                        PopulateMultiUnitFooter();
                        break;
                    case FooterMode.Group:
                        PopulateGroupFooter();
                        break;
                }

                ForceTreeRegeneration();

                _updatingFooterInfo = false;
                if (_updateAction != null)
                {
                    _updateAction.Invoke();
                }
            });
        }

        public void PopulateSingleUnitFooter(bool forceUpdate)
        {
            ToggleUnitInfo(true);

            bool isPlayerUnitTakingTurn = CurrentUnit.AI.GetControlType() == ControlType.Controlled && (Scene.InCombat ? CurrentUnit == Scene.CurrentUnit : true);

            #region unit status box

            float sizeDiff = WindowConstants.ScreenUnits.X - _infoBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter).X;
            UIDimensions infoBarDimensions = new UIDimensions(sizeDiff * 2 * WindowConstants.AspectRatio, 200);
            _infoBlock.SetSize(infoBarDimensions);
            _infoBlock.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(20, -4, 0), UIAnchorPosition.BottomLeft);
            _infoBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 0.75f));


            _unitNameTextBox.SetText(CurrentUnit.Name);

            _unitHealthBar.SetRender(true);
            //_unitHealthBar.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(25, -9, 0), UIAnchorPosition.BottomLeft);
            _unitHealthBar.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(25, -24, 0), UIAnchorPosition.BottomLeft);
            _unitHealthBar.SetHealthPercent(CurrentUnit.GetResF(ResF.Health) / CurrentUnit.GetResF(ResF.MaxHealth), CurrentUnit.AI.GetTeam());

            _unitShieldBar.SetRender(true);
            _unitShieldBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(5, 0, 0), UIAnchorPosition.LeftCenter);
            _unitShieldBar.SetCurrentShields(CurrentUnit.GetResI(ResI.Shields));

            
            _unitStaminaBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 0, 0), UIAnchorPosition.TopCenter);
            _unitStaminaBar.SetStaminaAmount(CurrentUnit.GetResI(ResI.Stamina), CurrentUnit.GetResI(ResI.MaxStamina));
            _unitStaminaBar.SetRender(true);

            if (isPlayerUnitTakingTurn)
            {
                //_unitHealthBar.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(25, -24, 0), UIAnchorPosition.BottomLeft);
                //_unitStaminaBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 0, 0), UIAnchorPosition.TopCenter);
                //_unitStaminaBar.SetStaminaAmount(CurrentUnit.GetResI(ResI.Stamina), CurrentUnit.GetResI(ResI.MaxStamina));
                //_unitStaminaBar.SetRender(true);

                if (Scene.EnergyDisplayBar != null && Scene.ActionEnergyBar != null)
                {
                    Scene.EnergyDisplayBar.SetActiveEnergy(CurrentUnit.GetResF(ResF.MovementEnergy));
                    Scene.ActionEnergyBar.SetActiveEnergy(CurrentUnit.GetResF(ResF.ActionEnergy));
                }
            }
            else
            {
                //_unitStaminaBar.SetRender(false);
            }


            Vector3 nameBoxPos = _infoBlock.GetAnchorPosition(UIAnchorPosition.TopLeft);
            UIDimensions nameBoxDim = _unitNameTextBox.GetDimensions();

            _unitNameTextBox.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, 2, 0), UIAnchorPosition.BottomCenter);

            _buffBlock.SetRender(true);
            _buffBlock.SetSize(new UIDimensions(infoBarDimensions.X * 0.4f, infoBarDimensions.Y));
            Vector3 buffBlockPos = _infoBlock.GetAnchorPosition(UIAnchorPosition.TopLeft);
            _buffBlock.SetPositionFromAnchor(buffBlockPos, UIAnchorPosition.BottomLeft);
            _buffBlock.SetColor(new Vector4(0, 0, 0, 0));

            #endregion

            _selectAbilityByHotkeyList.Clear();

            GeneralAbilityPane pane = new GeneralAbilityPane(CurrentUnit, isPlayerUnitTakingTurn, this);
            pane.SAP(_generalBlock.GAP(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            _generalBlock.RemoveChildren();
            _generalBlock.AddChild(pane, 100);

            CreateAbilityIcons(isPlayerUnitTakingTurn, CurrentUnit.Info.Abilities);

            CreateItemIcons(isPlayerUnitTakingTurn);

            #region Buffs
            //if (!Enumerable.SequenceEqual(_currentBuffs, CurrentUnit.Info.BuffManager.Buffs) || forceUpdate)
            //{
            _currentBuffs.Clear();
            CreateBuffIcons(isPlayerUnitTakingTurn);
            //}
            #endregion
        }

        public void PopulateMultiUnitFooter()
        {
            var castingUnit = Scene._selectedUnits.Find(u => u.AI.GetControlType() == ControlType.Controlled);

            if (castingUnit != null)
            {
                var selectedUnits = Scene._selectedUnits.ToHashSet();

                List<Ability> abilities = new List<Ability>();

                #region Create group
                bool allControllable = true;
                foreach (var unit in Scene._selectedUnits)
                {
                    if(unit.AI.GetControlType() != ControlType.Controlled) 
                    {
                        allControllable = false;
                        break;
                    }
                }

                if (allControllable)
                {
                    GroupCreate create = new GroupCreate(Scene._selectedUnits);
                    abilities.Add(create);
                }
                #endregion

                CreateAbilityIcons(true, abilities);
            }

        }

        public void PopulateGroupFooter()
        {
            var castingUnit = Scene._selectedUnits.Find(u => u.AI.GetControlType() == ControlType.Controlled);

            if (castingUnit != null && castingUnit.Info.Group != null)
            {
                List<Ability> abilities = new List<Ability>();

                foreach(var ability in castingUnit.Info.Group.GroupAbilities)
                {
                    abilities.Add(ability);
                }

                GroupDissolve groupDissolve = new GroupDissolve(castingUnit.Info.Group);
                abilities.Add(groupDissolve);

                CreateAbilityIcons(true, abilities);


                #region unit status box
                _infoBlock.SetRender(true);

                float sizeDiff = WindowConstants.ScreenUnits.X - _infoBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter).X;
                UIDimensions infoBarDimensions = new UIDimensions(sizeDiff * 2 * WindowConstants.AspectRatio, 200);
                _infoBlock.SetSize(infoBarDimensions);
                _infoBlock.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(20, -4, 0), UIAnchorPosition.BottomLeft);
                _infoBlock.SetColor(castingUnit.Info.Group.GroupColor);

                _unitNameTextBox.SetText("Group of units");

                _unitHealthBar.SetRender(false);
                _unitShieldBar.SetRender(false);
                _unitStaminaBar.SetRender(false);
                _buffBlock.SetRender(false);

                Vector3 nameBoxPos = _infoBlock.GetAnchorPosition(UIAnchorPosition.TopLeft);
                UIDimensions nameBoxDim = _unitNameTextBox.GetDimensions();

                _unitNameTextBox.SAP(_infoBlock.GAP(UIAnchorPosition.TopLeft) + new Vector3(5, 5, 0), UIAnchorPosition.TopLeft);
                #endregion
            }
        }

        public override void OnResize()
        {
            base.OnResize();

            //RemoveChild(_scrollableAreaBuff);
            //InitializeScrollableAreaBuff();
            //InitializeScrollableAreaAbility();
        }

        //private void InitializeScrollableAreaBuff() 
        //{
        //    if(_scrollableAreaBuff != null)
        //        RemoveChild(_scrollableAreaBuff);

        //    UIScale scrollableAreaSize = new UIScale(_generalBlock.Size);
        //    scrollableAreaSize.X /= 3.3f;
        //    scrollableAreaSize.Y -= .02f;

        //    //_buffContainer = new UIBlock(new Vector3(), scrollableAreaSize);
        //    _scrollableAreaBuff = new ScrollableArea(new Vector3(), scrollableAreaSize, new Vector3(), new UIScale(scrollableAreaSize.X, scrollableAreaSize.Y), 0.05f);

        //    float scrollbarWidth = 0;
        //    if (_scrollableAreaBuff.Scrollbar != null)
        //    {
        //        scrollbarWidth = _scrollableAreaBuff.Scrollbar.GetDimensions().X;
        //    }

        //    _scrollableAreaBuff.SetVisibleAreaPosition(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-3 - scrollbarWidth, 5, 0), UIAnchorPosition.TopRight);
        //    _scrollableAreaBuff.BaseComponent.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

        //    _scrollableAreaBuff.BaseComponent.SetColor(new Vector4(0, 0, 0, 0));

        //    AddChild(_scrollableAreaBuff, 1000);
        //}



        public Dictionary<string, Action<string>> _selectAbilityByHotkeyList = new Dictionary<string, Action<string>>();
        private void CreateAbilityIcons(bool isPlayerUnitTakingTurn, List<Ability> abilities) 
        {
            _currentAbilities.Clear();

            RemoveChildren(_currentIcons);

            _currentIcons.Clear();

            UIScale iconSize = new UIScale(0.2f, 0.2f);
            int count = 0;


            foreach (Ability ability in abilities)
            {
                _currentAbilities.Add(ability);
                string hotkey = null;
                if (CurrentUnit.AI.GetControlType() == ControlType.Controlled)
                {
                    hotkey = (count + 2).ToString();
                }

                Icon abilityIcon = ability.GenerateIcon(iconSize, true,
                    CurrentUnit.AI.GetTeam().GetRelation(UnitTeam.PlayerUnits) == Relation.Friendly ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                    false, null, isPlayerUnitTakingTurn && ability.CanCast() ? hotkey : null, showCharges: true);

                int currIndex = count;

                abilityIcon.DisabledColor = _Colors.IconDisabled;
                abilityIcon.SelectedColor = _Colors.IconSelected;
                abilityIcon.HoverColor = _Colors.IconHover;

                if (_currentIcons.Count == 0)
                {
                    abilityIcon.SetPositionFromAnchor(_generalBlock.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(20, 0, 0), UIAnchorPosition.LeftCenter);
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
                        Scene.SelectAbility(ability, CurrentUnit);
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

                void selectAbilityByNum(string keyPressed)
                {
                    if (keyPressed == hotkey && isPlayerUnitTakingTurn && ability.CanCast() && _currentAbilities.Count > 0)
                    {
                        Scene.SelectAbility(ability, CurrentUnit);
                    }
                    //if (keyPressed == hotkey && !isPlayerUnitTakingTurn && Scene.CurrentUnit != null
                    //    && Scene.CurrentUnit.AI.ControlType == ControlType.Controlled)
                    //{
                    //    UpdateFooterInfo(Scene.CurrentUnit);
                    //}
                }

                if(hotkey != null)
                {
                    _selectAbilityByHotkeyList.AddOrSet(hotkey, selectAbilityByNum);
                }


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

        
        public void CreateItemIcons(bool isPlayerUnitTakingTurn)
        {
            _itemBlock.RemoveChildren();

            ItemAbilityPane itemPane = new ItemAbilityPane(CurrentUnit, isPlayerUnitTakingTurn, this);

            if(_currentIcons.Count > 0)
            {
                itemPane.SAP(_currentIcons[^1].GAP(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
            }
            else
            {
                itemPane.SAP(_generalBlock.GAP(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
            }

            _itemBlock.AddChild(itemPane);
        }

        /// <summary>
        /// This should be dynamically set later but for now hardcoding it will be fine.
        /// </summary>
        public static HashSet<string> HotkeyStrings = new HashSet<string>()
        {
            "`", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };
        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);


            string keyVal = TextHelper.KeyStrokeToString(e);
            if (_selectAbilityByHotkeyList.ContainsKey(keyVal))
            {
                _selectAbilityByHotkeyList[keyVal]?.Invoke(keyVal);
            }
            else if (HotkeyStrings.Contains(keyVal) && (CurrentUnit == null || CurrentUnit.AI.GetControlType() != ControlType.Controlled))
            {
                if(Scene.InCombat && CurrentUnit != Scene.CurrentUnit && Scene.CurrentUnit.AI.GetControlType() == ControlType.Controlled)
                {
                    Scene.SelectUnit(Scene.CurrentUnit);
                    //UpdateFooterInfo(Scene.CurrentUnit, forceUpdate: true);
                }
                else if(CurrentUnit != LastSelectedControllableUnit && LastSelectedControllableUnit != null)
                {
                    Scene.SelectUnit(LastSelectedControllableUnit);
                    //UpdateFooterInfo(LastSelectedControllableUnit, forceUpdate: true);
                }
            }
        }

        private void CreateBuffIcons(bool isPlayerUnitTakingTurn) 
        {
            _buffBlock.RemoveChildren();

            UIScale buffSize = new UIScale(0.09f, 0.09f);

            List<Icon> icons = new List<Icon>();
            int count = 0;

            lock (CurrentUnit.Info.BuffManager._buffLock)
            {
                foreach (Buff buff in CurrentUnit.Info.BuffManager.Buffs)
                {
                    if (buff.Invisible)
                        continue;

                    Icon buffIcon = buff.GetIcon();

                    if (buffIcon == null)
                        continue;

                    Icon icon = new Icon(buffIcon, buffSize, true);
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

                    if (buff.Stacks != int.MinValue)
                    {
                        UIScale stackSize = new UIScale(buffSize.X * 0.333f, buffSize.Y * 0.333f);

                        Text text = new Text(buff.Stacks.ToString(), Text.DEFAULT_FONT, 16, Brushes.Black, Color.DarkBlue);

                        text.SAP(icon.GAP(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomRight);

                        icon.AddChild(text);
                    }

                    Scene.Tick += icon.Tick;

                    icon.OnCleanUp += (_) =>
                    {
                        Scene.Tick -= icon.Tick;

                        icon.Hovered = true;
                        icon.OnHoverEnd();
                    };

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
                        //UIHelpers.CreateToolTip(Scene, buff.GenerateTooltip(), icon, Scene._tooltipBlock);
                    }

                    icon.HasTimedHoverEffect = true;
                    icon.Hoverable = true;
                    icon.TimedHover += buffHover;


                    //_scrollableAreaBuff.BaseComponent.AddChild(icon, 1000);
                    _buffBlock.AddChild(icon, 1000);
                    count++;
                }
            }
        }
    }
}
