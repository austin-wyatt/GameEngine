﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public enum GameFooterContextFlags
    {
        TooltipOpen
    }
    public class GameFooter : Footer
    {
        CombatScene Scene;

        private TextComponent _unitNameTextBox;
        private HealthBar _unitHealthBar;
        private UIBlock _containingBlock;
        private ShieldBar _unitShieldBar;
        public Unit _currentUnit;

        private ScrollableArea _scrollableArea;

        public ContextManager<GameFooterContextFlags> ContextManager = new ContextManager<GameFooterContextFlags>();

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

            UIDimensions textOffset = new UIDimensions(80, 100);
            //textOffset.Y = 0;

            //UIScale textScale = endTurnButton.TextBox.TextField.GetDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 3;
            //textScale.Y *= -1;
            endTurnButton.SetSize(endTurnButton.TextBox.GetScale() + textOffset.ToScale());

            endTurnButton.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, -10, 0), UIAnchorPosition.BottomRight);


            endTurnButton.OnClickAction = () =>
            {
                Scene.CompleteTurn();
                Scene.DeselectUnits();
            };

            AddChild(endTurnButton, 100);
            #endregion

            UIDimensions containingBlockDimensions = Size;
            //containingBlockDimensions.X *= 1.5f;
            containingBlockDimensions.Y -= 20;

            _containingBlock = new UIBlock(Position, containingBlockDimensions);
            _containingBlock.MultiTextureData.MixTexture = true;
            _containingBlock.MultiTextureData.MixPercent = 0.4f;
            //_containingBlock.SetColor(Colors.UISelectedGray);
            _containingBlock.SetColor(new Vector4(0.447f, 0.51f, 0.639f, 0.75f));
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

            void healthBarHover() 
            {
                UIHelpers.CreateToolTip(Scene, _currentUnit.Health + "/" + Unit.MaxHealth, _unitHealthBar, Scene._tooltipBlock);
            }

            _unitHealthBar._onTimedHoverActions.Add(healthBarHover);

            _containingBlock.AddChild(_unitHealthBar, 100);

            _unitShieldBar = new ShieldBar(new Vector3(), new UIScale(0.5f, 0.1f)) { Hoverable = true, HasTimedHoverEffect = true };

            void shieldBarHover()
            {
                if (_currentUnit.CurrentShields >= 0)
                {
                    UIHelpers.CreateToolTip(Scene, _currentUnit.CurrentShields * _currentUnit.ShieldBlock + " Damage will be blocked from the next attack", _unitShieldBar, Scene._tooltipBlock);
                }
                else 
                {
                    UIHelpers.CreateToolTip(Scene, "Next attack recieved will deal " + _currentUnit.CurrentShields * -1 * 25 + "% more damage", _unitShieldBar, Scene._tooltipBlock);
                }
            }

            _unitShieldBar._onTimedHoverActions.Add(shieldBarHover);

            _containingBlock.AddChild(_unitShieldBar, 100);
            #endregion

            UIScale scrollableAreaSize = new UIScale(containingBlockDimensions);
            scrollableAreaSize.X /= 1.8f;
            scrollableAreaSize.Y -= .02f;

            //_buffContainer = new UIBlock(new Vector3(), scrollableAreaSize);
            _scrollableArea = new ScrollableArea(new Vector3(), scrollableAreaSize, new Vector3(), new UIScale(scrollableAreaSize.X, scrollableAreaSize.Y), 0.05f);

            float scrollbarWidth = 0;
            if (_scrollableArea.Scrollbar != null) 
            {
                scrollbarWidth = _scrollableArea.Scrollbar.GetDimensions().X;
            }

            _scrollableArea.SetVisibleAreaPosition(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3( -3 - scrollbarWidth, 5, 0), UIAnchorPosition.TopRight);
            _scrollableArea.BaseComponent.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            _scrollableArea.BaseComponent.SetColor(new Vector4(0, 0, 0, 0));

            AddChild(_scrollableArea, 1000);


            UpdateFooterInfo(Scene.CurrentUnit);
        }

        private List<Icon> _currentIcons = new List<Icon>();
        public void UpdateFooterInfo(Unit unit) 
        {
            _currentUnit = unit;

            #region unit status box
            _unitNameTextBox.SetText(unit.Name);
            
            
            Vector3 nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + _containingBlock.GetDimensions().X / 6;
            nameBoxPos.Y = nameBoxPos.Y - _containingBlock.GetDimensions().Y / 4;

            _unitNameTextBox.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);

            nameBoxPos = _containingBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter);
            nameBoxPos.X = nameBoxPos.X + (_containingBlock.GetDimensions().X / 6) * 3;
            nameBoxPos.Y = nameBoxPos.Y - _containingBlock.GetDimensions().Y / 4;

            _unitHealthBar.SetPositionFromAnchor(nameBoxPos, UIAnchorPosition.Center);
            _unitHealthBar.SetHealthPercent(unit.Health / Unit.MaxHealth, unit.Team);

            _unitShieldBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitShieldBar.SetCurrentShields(unit.CurrentShields);
            #endregion

            #region ability icons
            _currentIcons.ForEach(i => RemoveChild(i.ObjectID));
            _currentIcons.Clear();
            UIScale iconSize = new UIScale(0.25f, 0.25f);
            int count = 0;
            foreach (Ability ability in Scene.CurrentUnit.Abilities.Values) 
            {
                Icon abilityIcon = ability.GenerateIcon(iconSize, true, Scene.CurrentUnit.Team == UnitTeam.Ally ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground, true);

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
                    if (Scene.EnergyDisplayBar != null && ability.GetEnergyCost() <= Scene.EnergyDisplayBar.CurrentEnergy)
                    {
                        abilityIcon.Clickable = true;
                        abilityIcon.Hoverable = true;
                    }
                    else
                    {
                        abilityIcon.SetColor(Colors.IconDisabled);
                    }
                }

                checkAbilityClickable();

                abilityIcon.OnClickAction = () =>
                {
                    Scene.SelectAbility(ability);
                };


                void onAbilitySelected(Ability selectedAbility) 
                {
                    if (selectedAbility.AbilityID == ability.AbilityID) 
                    {
                        abilityIcon.SetColor(Colors.IconSelected);
                    }
                }

                void onAbilityDeselected() 
                {
                    abilityIcon.SetColor(Colors.White);
                }

                void onAbilityCast(Ability castAbility) 
                {
                    if (Scene.EnergyDisplayBar != null && ability.GetEnergyCost() > Scene.EnergyDisplayBar.CurrentEnergy) 
                    {
                        abilityIcon.Clickable = false;
                        abilityIcon.Hoverable = false;
                        abilityIcon.SetColor(Colors.IconDisabled);
                    }
                }

                Scene._onSelectAbilityActions.Add(onAbilitySelected);
                Scene._onDeselectAbilityActions.Add(onAbilityDeselected);
                Scene._onAbilityCastActions.Add(onAbilityCast);

                Scene.EventActions[(EventAction)count] = () => {
                    Scene.SelectAbility(ability);
                };

                UIHelpers.AddAbilityIconHoverEffect(abilityIcon, Scene, ability);


                abilityIcon._cleanUpAction = () =>
                {
                    Scene._onSelectAbilityActions.Remove(onAbilitySelected);
                    Scene._onDeselectAbilityActions.Remove(onAbilityDeselected);
                    Scene._onAbilityCastActions.Remove(onAbilityCast);
                };

                _currentIcons.Add(abilityIcon);
                AddChild(abilityIcon, 100);

                count++;
            }
            #endregion


            #region buff icons
            _scrollableArea.BaseComponent.ClearChildren();

            UIScale buffSize = new UIScale(0.09f, 0.09f);

            List<Icon> icons = new List<Icon>();
            count = 0;
            int delimiter = -1;
            _scrollableArea.SetBaseAreaSize(new UIScale(_scrollableArea.Size.X, _scrollableArea.Size.Y));
            foreach (Buff buff in unit.Buffs) 
            {
                if (buff.Hidden)
                    continue;

                Icon icon = buff.GenerateIcon(buffSize);
                icons.Add(icon);

                if (count == 0)
                {
                    icon.SetPositionFromAnchor(_scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
                }
                else 
                {
                    icon.SetPositionFromAnchor(icons[count - 1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                }

                if (icon.GetAnchorPosition(UIAnchorPosition.RightCenter).X > _scrollableArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.RightCenter).X) 
                {
                    if (delimiter == -1) 
                    {
                        delimiter = count;
                    }
                    icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);

                    if (icon.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y > _scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y) 
                    {
                        _scrollableArea.SetBaseAreaSize(new UIScale(_scrollableArea._baseAreaSize.X, _scrollableArea._baseAreaSize.Y + icon.GetDimensions().ToScale().Y * 3));

                        icon.SetPositionFromAnchor(icons[count - delimiter].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);
                    }

                    //_scrollableArea.BaseComponent.SetSize(_scrollableArea._baseAreaSize);
                }

                void buffHover() 
                {
                    UIHelpers.CreateToolTip(Scene, buff.GenerateTooltip(), icon, Scene._tooltipBlock);
                }

                icon.HasTimedHoverEffect = true;
                icon.Hoverable = true;
                icon._onTimedHoverActions.Add(buffHover);


                _scrollableArea.BaseComponent.AddChild(icon, 1000);
                count++;
            }

            #endregion
        }
    }
}
