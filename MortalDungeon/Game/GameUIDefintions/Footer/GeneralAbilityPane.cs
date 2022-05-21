using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.UI
{
    public class GeneralAbilityPane : UIObject
    {
        public Unit CurrentUnit;
        GameFooter Footer;
        public GeneralAbilityPane(Unit unit, bool isPlayerUnitTakingTurn, GameFooter footer)
        {
            BaseComponent = new UIBlock();
            BaseComponent.SetAllInline(0);
            BaseComponent.SetColor(_Colors.Transparent);
            AddChild(BaseComponent);

            CurrentUnit = unit;
            Footer = footer;

            List<(Ability, string hotkey)> abilities = new List<(Ability, string hotkey)>();

            if(unit.Info._movementAbility != null)
            {
                abilities.Add((unit.Info._movementAbility, isPlayerUnitTakingTurn ? "1" : null));
            }

            CreateGeneralAbilityIcons(isPlayerUnitTakingTurn, abilities);

            ValidateObject(this);
        }

        private void CreateGeneralAbilityIcons(bool isPlayerUnitTakingTurn, List<(Ability, string hotkey)> abilities)
        {
            UIScale iconSize = new UIScale(0.15f, 0.15f);
            int count = 0;

            List<Icon> icons = new List<Icon>();

            foreach ((Ability ability, string hotkey) in abilities)
            {
                Icon abilityIcon = ability.GenerateIcon(iconSize, true,
                    CurrentUnit.AI.GetTeam().GetRelation(UnitTeam.PlayerUnits) == Relation.Friendly ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                    false, null, hotkey, showCharges: true, hotkeyTextScale: 0.07f);

                int currIndex = count;

                abilityIcon.DisabledColor = _Colors.IconDisabled;
                abilityIcon.SelectedColor = _Colors.IconSelected;
                abilityIcon.HoverColor = _Colors.IconHover;

                if (count == 0)
                {
                    abilityIcon.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
                }
                else
                {
                    abilityIcon.SetPositionFromAnchor(
                        icons[^1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
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
                        abilityIcon.OnDisabled(true);
                    }
                }

                checkAbilityClickable();

                abilityIcon.Click += (s, e) =>
                {
                    if (isPlayerUnitTakingTurn && ability.CanCast())
                    {
                        CurrentUnit.Scene.SelectAbility(ability, CurrentUnit);
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


                CurrentUnit.Scene._onSelectAbilityActions.Add(onAbilitySelected);
                CurrentUnit.Scene._onDeselectAbilityActions.Add(onAbilityDeselected);


                void selectAbilityByNum(string keyPressed)
                {
                    if (keyPressed == hotkey && isPlayerUnitTakingTurn && ability.CanCast())
                    {
                        CurrentUnit.Scene.SelectAbility(ability, CurrentUnit);
                    }

                    //if (keyPressed == hotkey && !isPlayerUnitTakingTurn && CurrentUnit.Scene.CurrentUnit != null
                    //    && CurrentUnit.Scene.CurrentUnit.AI.ControlType == ControlType.Controlled)
                    //{
                    //    Footer.UpdateFooterInfo(CurrentUnit.Scene.CurrentUnit);
                    //}
                }

                if(hotkey != null)
                {
                    Footer._selectAbilityByHotkeyList.AddOrSet(hotkey, selectAbilityByNum);
                }


                void cleanUp(GameObject obj)
                {
                    CurrentUnit.Scene._onSelectAbilityActions.Remove(onAbilitySelected);
                    CurrentUnit.Scene._onDeselectAbilityActions.Remove(onAbilityDeselected);
                    //Scene._onAbilityCastActions.Remove(onAbilityCast);

                    abilityIcon.OnCleanUp -= cleanUp;
                }
                abilityIcon.OnCleanUp += cleanUp;

                void abilityHover(GameObject obj)
                {
                    UIHelpers.CreateToolTip(CurrentUnit.Scene, ability.GenerateTooltip(), abilityIcon, CurrentUnit.Scene._tooltipBlock);
                }

                abilityIcon.HasTimedHoverEffect = true;
                abilityIcon.Hoverable = true;
                abilityIcon.TimedHover += abilityHover;

                abilityIcon.Name = ability.Name + " Icon";

                abilityIcon.Name = "Icon " + count;

                icons.Add(abilityIcon);
                AddChild(abilityIcon, 100);

                count++;
            }
        }
    }
}
