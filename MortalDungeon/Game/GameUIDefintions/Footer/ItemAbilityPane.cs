using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Items;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.UI
{
    internal class ItemAbilityPane : UIObject
    {
        public Unit CurrentUnit;
        GameFooter Footer;

        private UIObject WeaponIcon;

        public ItemAbilityPane(Unit unit, bool isPlayerUnitTakingTurn, GameFooter footer)
        {
            BaseComponent = new UIBlock();
            BaseComponent.SetAllInline(0);
            BaseComponent.SetColor(_Colors.Transparent);
            AddChild(BaseComponent);


            CurrentUnit = unit;
            Footer = footer;


            List<(Ability, string hotkey)> abilities = new List<(Ability, string hotkey)>();

            int count = 7;
            foreach(var item in unit.Info.Equipment.EquippedItems)
            {
                if(item.Value.ItemAbility != null && item.Key != EquipmentSlot.Weapon_1 && item.Key != EquipmentSlot.Weapon_2)
                {
                    abilities.Add((item.Value.ItemAbility, count.ToString()));
                    count++;
                }
            }

            UIBlock divider = new UIBlock(default, new UIScale(0.01f, footer.Size.Y));
            divider.SetAllInline(0);
            divider.SetColor(_Colors.Black);

            divider.SAP(BaseComponent.GAP(UIAnchorPosition.LeftCenter) + new Vector3(-2, 0, 0), UIAnchorPosition.LeftCenter);
            AddChild(divider);

            WeaponIcon = null;

            CreateWeaponIcons(isPlayerUnitTakingTurn);

            CreateNonWeaponAbilityIcons(isPlayerUnitTakingTurn, abilities);

            ValidateObject(this);
        }

        private void CreateWeaponIcons(bool isPlayerUnitTakingTurn)
        {
            Item primaryWeapon = null;
            Item secondaryWeapon = null;

            //Get the correct primary weapon
            switch (CurrentUnit.Info.Equipment.PrimaryWeaponSlot)
            {
                case EquipmentSlot.Weapon_1:
                    CurrentUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Weapon_1, out primaryWeapon);
                    CurrentUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Weapon_2, out secondaryWeapon);
                    break;
                case EquipmentSlot.Weapon_2:
                    CurrentUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Weapon_2, out primaryWeapon);
                    CurrentUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Weapon_1, out secondaryWeapon);
                    break;
            }

            if (primaryWeapon == null && secondaryWeapon == null)
                return;

            //if the primary weapon is an empty slot then show the secondary weapon as the primary.
            if(primaryWeapon == null && secondaryWeapon != null)
            {
                primaryWeapon = secondaryWeapon;
                secondaryWeapon = null;
            }

            string primaryHotkey = "6";

            Icon primaryIcon = null;
            UIScale primaryIconSize = new UIScale(0.175f, 0.175f);

            if (primaryWeapon.ItemAbility != null)
            {
                primaryIcon = primaryWeapon.ItemAbility.GenerateIcon(primaryIconSize, true,
                    CurrentUnit.AI.Team == UnitTeam.PlayerUnits ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                    false, null, primaryHotkey, showCharges: true, hotkeyTextScale: 0.07f);

                InitializeBasicIconInfo(primaryIcon, primaryWeapon.ItemAbility, isPlayerUnitTakingTurn, primaryHotkey);

                primaryIcon.SAP(BaseComponent.GAP(UIAnchorPosition.LeftCenter) + new Vector3(10, -15, 0), UIAnchorPosition.LeftCenter);

                WeaponIcon = primaryIcon;
            }

            Icon secondaryIcon = null;
            UIScale secondaryIconSize = new UIScale(0.075f, 0.075f);

            if (secondaryWeapon?.ItemAbility != null && !(secondaryWeapon.Tags.HasFlag(ItemTag.Weapon_Concealed) && 
                (CurrentUnit.AI.Team != UnitTeam.PlayerUnits) && !isPlayerUnitTakingTurn))
            {
                secondaryIcon = secondaryWeapon.ItemAbility.GenerateIcon(secondaryIconSize, true,
                    CurrentUnit.AI.Team == UnitTeam.PlayerUnits ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                    false, null, null, showCharges: false, hotkeyTextScale: 0.07f);

                InitializeBasicIconInfo(secondaryIcon, secondaryWeapon.ItemAbility, isPlayerUnitTakingTurn, null, canCastOverride: false);

                secondaryIcon.SAP(primaryIcon.GAP(UIAnchorPosition.BottomLeft) + new Vector3(2, 3, 0), UIAnchorPosition.TopLeft);
            }

            if(secondaryIcon != null)
            {
                UIBlock weaponSwapButton = new UIBlock(size: secondaryIconSize,
                    spritesheetPosition: (int)UISheetIcons.RefreshIcon, spritesheet: Spritesheets.UISheet);
                weaponSwapButton.DisabledColor = _Colors.IconDisabled;
                weaponSwapButton.SelectedColor = _Colors.IconSelected;
                weaponSwapButton.HoverColor = _Colors.IconHover;

                weaponSwapButton.Disabled = !CurrentUnit.Info.CanSwapWeapons();

                weaponSwapButton.Hoverable = true;
                weaponSwapButton.Click += (s, e) =>
                {
                    CurrentUnit.Info.Equipment.SwapWeapons();
                    Footer.CreateItemIcons(isPlayerUnitTakingTurn);
                };

                weaponSwapButton.Clickable = true;

                weaponSwapButton.SAP(primaryIcon.GAP(UIAnchorPosition.BottomRight) + new Vector3(-2, 3, 0), UIAnchorPosition.TopRight);
                AddChild(weaponSwapButton);
            }
        }

        private void CreateNonWeaponAbilityIcons(bool isPlayerUnitTakingTurn, List<(Ability, string hotkey)> abilities)
        {
            UIScale iconSize = new UIScale(0.15f, 0.15f);
            int count = 0;

            List<Icon> icons = new List<Icon>();

            foreach ((Ability ability, string hotkey) in abilities)
            {
                Icon abilityIcon = ability.GenerateIcon(iconSize, true,
                    CurrentUnit.AI.Team == UnitTeam.PlayerUnits ? Icon.BackgroundType.BuffBackground : Icon.BackgroundType.DebuffBackground,
                    false, null, hotkey, showCharges: true, hotkeyTextScale: 0.07f);

                int currIndex = count;

                InitializeBasicIconInfo(abilityIcon, ability, isPlayerUnitTakingTurn, hotkey);

                if (count == 0)
                {
                    if(WeaponIcon != null)
                    {
                        Vector3 basePos = BaseComponent.GAP(UIAnchorPosition.LeftCenter);
                        Vector3 weapPos = WeaponIcon.GAP(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0);
                        abilityIcon.SAP(new Vector3(weapPos.X, basePos.Y, basePos.Z), UIAnchorPosition.LeftCenter);
                    }
                    else
                    {
                        abilityIcon.SAP(BaseComponent.GAP(UIAnchorPosition.LeftCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                    }
                }
                else
                {
                    abilityIcon.SetPositionFromAnchor(
                        icons[^1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
                }

                icons.Add(abilityIcon);
                count++;
            }
        }

        /// <summary>
        /// Creates most of the expected basic functionality for an ability icon.
        /// </summary>
        private void InitializeBasicIconInfo(Icon abilityIcon, Ability ability, bool isPlayerUnitTakingTurn, string hotkey, bool canCastOverride = true)
        {
            abilityIcon.DisabledColor = _Colors.IconDisabled;
            abilityIcon.SelectedColor = _Colors.IconSelected;
            abilityIcon.HoverColor = _Colors.IconHover;

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
                if (isPlayerUnitTakingTurn && ability.CanCast() && canCastOverride)
                {
                    CurrentUnit.Scene.SelectAbility(ability, CurrentUnit);
                }
            };


            void onAbilitySelected(Ability selectedAbility)
            {
                if (selectedAbility.AbilityID == ability.AbilityID)
                {
                    abilityIcon.OnSelect(true);

                    int pitchIndex = 0;

                    int.TryParse(hotkey, out pitchIndex);

                    Sound sound = new Sound(Sounds.Select) { Gain = 0.1f, Pitch = 0.5f + pitchIndex * 0.05f };
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
            }

            if (hotkey != null)
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

            abilityIcon.Name = "Icon " + hotkey;

            
            AddChild(abilityIcon, 100);
        }
    }
}
