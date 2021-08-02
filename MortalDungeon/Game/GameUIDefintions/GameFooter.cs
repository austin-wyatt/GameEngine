using MortalDungeon.Engine_Classes;
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

        private TextBox _unitNameTextBox;
        private HealthBar _unitHealthBar;
        private UIBlock _containingBlock;
        private ShieldBar _unitShieldBar;
        private Unit _currentUnit;

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
            
            UIDimensions textOffset = new UIDimensions(endTurnButton.TextBox.TextOffset);
            textOffset.Y = 0;

            UIScale textScale = endTurnButton.TextBox.TextField.GetTextDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 3;
            textScale.Y *= -1;
            endTurnButton.SetSize(textScale);

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
            AddChild(_containingBlock, 1);

            #region name box
            _unitNameTextBox = new TextBox(new Vector3(), new UIScale(0.5f, 0.1f), "", 0.075f, true);
            _unitNameTextBox.BaseComponent.MultiTextureData.MixTexture = false;
            _unitNameTextBox.SetColor(Colors.Transparent);
            _unitNameTextBox.GetBaseObject().OutlineParameters.SetAllInline(0);
            _unitNameTextBox.GetBaseObject().RenderData = new RenderData() { AlphaThreshold = 1 };
            _unitNameTextBox.TextField.SetColor(new Vector4(0.149f, 0.173f, 0.22f, 1));
            //_unitNameTextBox.TextField.SetColor(new Vector4(0.1f, 0.1f, 0.1f, 1));
            UIScale nameBoxScale = containingBlockDimensions;
            nameBoxScale.X /= 3;
            nameBoxScale.Y = 0.1f;

            _unitNameTextBox.SetSize(nameBoxScale);

            _containingBlock.AddChild(_unitNameTextBox, 100);
            #endregion

            #region health and shield bar
            _unitHealthBar = new HealthBar(new Vector3(), new UIScale(0.5f, 0.1f)) { Hoverable = true, HasTimedHoverEffect = true };

            void healthBarHover() 
            {
                CreateToolTip(_currentUnit.Health + "/" + Unit.MaxHealth, _unitHealthBar);
            }

            _unitHealthBar._onTimedHoverActions.Add(healthBarHover);

            _containingBlock.AddChild(_unitHealthBar, 100);

            _unitShieldBar = new ShieldBar(new Vector3(), new UIScale(0.5f, 0.1f)) { Hoverable = true, HasTimedHoverEffect = true };

            void shieldBarHover()
            {
                if (_currentUnit.CurrentShields >= 0)
                {
                    CreateToolTip(_currentUnit.CurrentShields * _currentUnit.ShieldBlock + " Damage will be blocked from the next attack", _unitShieldBar);
                }
                else 
                {
                    CreateToolTip("Next attack recieved will deal " + _currentUnit.CurrentShields * -1 * 25 + "% more damage", _unitShieldBar);
                }
            }

            _unitShieldBar._onTimedHoverActions.Add(shieldBarHover);

            _containingBlock.AddChild(_unitShieldBar, 100);
            #endregion




            UpdateFooterInfo(Scene.CurrentUnit);
        }

        private List<Icon> _currentIcons = new List<Icon>();
        public void UpdateFooterInfo(Unit unit) 
        {
            _currentUnit = unit;

            _unitNameTextBox.TextField.SetTextString(unit.Name);
            UIDimensions textOffset = new UIDimensions(_unitNameTextBox.TextOffset);
            textOffset.Y = 0;

            UIScale textScale = _unitNameTextBox.TextField.GetTextDimensions() * WindowConstants.AspectRatio * 2 + textOffset * 2;
            textScale.Y *= -1;
            //_unitNameTextBox.SetSize(textScale);
            

            _unitNameTextBox.SetPositionFromAnchor(_containingBlock.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            _unitHealthBar.SetPositionFromAnchor(_unitNameTextBox.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);
            _unitHealthBar.SetHealthPercent(unit.Health / Unit.MaxHealth, unit.Team);

            _unitShieldBar.SetPositionFromAnchor(_unitHealthBar.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitShieldBar.SetCurrentShields(unit.CurrentShields);


            #region ability icons
            _currentIcons.ForEach(i => RemoveChild(i.ObjectID));
            _currentIcons.Clear();
            UIScale iconSize = new UIScale(0.25f, 0.25f);
            foreach (Ability ability in Scene.CurrentUnit.Abilities.Values) 
            {
                Icon abilityIcon = ability.GenerateIcon(iconSize, true, Scene.CurrentUnit.Team == UnitTeam.Ally ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground);

                if (_currentIcons.Count == 0)
                {
                    abilityIcon.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.LeftCenter) + new Vector3(20, 0, 0), UIAnchorPosition.LeftCenter);
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

                void onHover() 
                {
                    abilityIcon.SetColor(Colors.IconHover);
                }

                void hoverEnd() 
                {
                    if (Scene._selectedAbility != null && Scene._selectedAbility.AbilityID == ability.AbilityID)
                    {
                        abilityIcon.SetColor(Colors.IconSelected);
                    }
                    else 
                    {
                        abilityIcon.SetColor(Colors.White);
                    }
                }

                Scene._onSelectAbilityActions.Add(onAbilitySelected);
                Scene._onDeselectAbilityActions.Add(onAbilityDeselected);
                Scene._onAbilityCastActions.Add(onAbilityCast);

                abilityIcon._onHoverActions.Add(onHover);
                abilityIcon._onHoverEndActions.Add(hoverEnd);

                abilityIcon._cleanUpAction = () =>
                {
                    Scene._onSelectAbilityActions.Remove(onAbilitySelected);
                    Scene._onDeselectAbilityActions.Remove(onAbilityDeselected);
                    Scene._onAbilityCastActions.Remove(onAbilityCast);
                };

                _currentIcons.Add(abilityIcon);
                AddChild(abilityIcon, 100);
            }
            #endregion
        }

        public void CreateToolTip(string text, UIObject parent) 
        {
            if (ContextManager.GetFlag(GameFooterContextFlags.TooltipOpen))
                return;

            ContextManager.SetFlag(GameFooterContextFlags.TooltipOpen, true);

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

            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(Scene._cursorObject.Position + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);

            Console.WriteLine(Scene._cursorObject.Position);

            AddChild(tooltip, 10000);

            void temp()
            {
                RemoveChild(tooltip.ObjectID);
                parent._onHoverEndActions.Remove(temp);
                ContextManager.SetFlag(GameFooterContextFlags.TooltipOpen, false);
            }

            parent._onHoverEndActions.Add(temp);
        }
    }
}
