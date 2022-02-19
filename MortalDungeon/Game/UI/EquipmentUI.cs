using MortalDungeon.Definitions;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Icon = MortalDungeon.Engine_Classes.UIComponents.Icon;

namespace MortalDungeon.Game.UI
{
    public class EquipmentUI
    {
        public UIObject Window;
        public CombatScene Scene;

        public bool Displayed = false;

        public EquipmentUI(CombatScene scene)
        {
            Scene = scene;
        }

        public void CreateWindow()
        {
            RemoveWindow();

            Window = UIHelpers.CreateWindow(new UIScale(2 * WindowConstants.AspectRatio, 2f), "Equipment", null, Scene, customExitAction: () =>
            {
                RemoveWindow();
            });

            Window.Draggable = false;

            Window.SetPosition(WindowConstants.CenterScreen);

            Scene.AddUI(Window, 1000);


            Displayed = true;

            PopulateData();
        }

        public void RemoveWindow()
        {
            if (Window != null)
            {
                Scene.UIManager.RemoveUIObject(Window);
            }

            Displayed = false;
        }

        private UIBlock _unitBlock;
        public void PopulateData()
        {
            _unitBlock = new UIBlock(default, new UIScale(0.32f, 0.32f));
            _unitBlock.SetPosition(WindowConstants.CenterScreen);
            _unitBlock.SetColor(_Colors.Tan);

            _unitBlock.HoverColor = _Colors.Tan - new Vector4(0.1f, 0.1f, 0.1f, 0);
            _unitBlock.Hoverable = true;
            _unitBlock.Clickable = true;
            _unitBlock.Click += (s, e) =>
            {
                CreateUnitSelectionDisplay();
            };

            _unitBlock.Hover += (s) =>
            {
                foreach(var child in _unitBlock.Children)
                {
                    child.SetColorOverride(ColorOverride.Hover);
                }
            };

            _unitBlock.HoverEnd += (s) =>
            {
                foreach (var child in _unitBlock.Children)
                {
                    child.SetColorOverride(ColorOverride.None);
                }
            };

            Window.AddChild(_unitBlock);

            FillEquipmentInfo();
        }


        private UIObject[] _equipmentSlots = new UIObject[11];
        private enum SlotMap
        {
            Armor,
            Boots,
            Gloves,
            Weapon,
            Jewelry_1,
            Jewelry_2,
            Trinket,
            Consumable_1,
            Consumable_2,
            Consumable_3,
            Consumable_4
        }
        public void FillEquipmentInfo()
        {
            bool unitSelected = _selectedUnit != null;

            _unitBlock.RemoveChildren();

            if (unitSelected)
            {
                UIBlock block = new UIBlock(default, new UIScale(0.3f, 0.3f));
                Scene.Tick += block.Tick;

                block.OnCleanUp += (e) =>
                {
                    Scene.Tick -= block.Tick;
                };

                BaseObject obj = _selectedUnit.CreateBaseObject();
                obj.EnableLighting = false;
                obj._currentAnimation.Pause();

                obj.BaseFrame.SetScale(block.Size.X / WindowConstants.AspectRatio, block.Size.Y, 1);

                block.BaseObjects.Clear();
                block.BaseObjects.Add(obj);
                block._baseObject = obj;

                block.Hoverable = true;
                block.HoverColor = _Colors.IconHover;
                block.SetPosition(_unitBlock.Position);
                _unitBlock.AddChild(block);

                block.Hover += (s) =>
                {
                    _unitBlock.SetColorOverride(ColorOverride.Hover);
                };

                block.HoverEnd += (s) =>
                {
                    _unitBlock.SetColorOverride(ColorOverride.None);
                };

                UIHelpers.AddTimedHoverTooltip(block, _selectedUnit.Name, Scene);
            }

            #region armor
            var armorBlock = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Armor] = armorBlock;

            armorBlock.SetPositionFromAnchor(_unitBlock.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(-5, 15, 0), UIAnchorPosition.BottomRight);
            _unitBlock.AddChild(armorBlock);

            Icon armorIcon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Armor, out var armor))
            {
                armorIcon = new Icon(new UIScale(0.15f, 0.15f), armor.AnimationSet.BuildAnimationsFromSet());

                armorIcon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Armor);
                    FillEquipmentInfo();
                };
            }
            else
            {
                armorIcon = new Icon(new UIScale(0.15f, 0.15f), Item_1.ArmorPlaceholder, Spritesheets.ItemSpritesheet_1);
                armorIcon.SetColor(_Colors.White);
            }

            armorIcon.Hoverable = true;
            armorIcon.HoverColor = _Colors.IconHover;

            armorIcon.Clickable = true;
            armorIcon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Armor, armorIcon, EquipmentSlot.Armor);
            };

            armorIcon.SetPosition(armorBlock.Position);

            armorBlock.AddChild(armorIcon);
            #endregion

            #region boots
            var bootsBlock = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Boots] = bootsBlock;

            bootsBlock.SetPositionFromAnchor(armorIcon.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitBlock.AddChild(bootsBlock);

            Icon bootsIcon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Boots, out var boots))
            {
                bootsIcon = new Icon(new UIScale(0.15f, 0.15f), boots.AnimationSet.BuildAnimationsFromSet());

                bootsIcon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Boots);
                    FillEquipmentInfo();
                };
            }
            else
            {
                bootsIcon = new Icon(new UIScale(0.15f, 0.15f), Item_1.BootPlaceholder, Spritesheets.ItemSpritesheet_1);
                bootsIcon.SetColor(_Colors.White);
            }

            bootsIcon.Hoverable = true;
            bootsIcon.HoverColor = _Colors.IconHover;

            bootsIcon.Clickable = true;
            bootsIcon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Boots, bootsIcon, EquipmentSlot.Boots);
            };

            bootsIcon.SetPosition(bootsBlock.Position);

            bootsBlock.AddChild(bootsIcon);
            #endregion

            #region gloves
            var glovesBlock = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Gloves] = glovesBlock;

            glovesBlock.SetPositionFromAnchor(bootsIcon.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitBlock.AddChild(glovesBlock);

            Icon glovesIcon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Gloves, out var gloves))
            {
                glovesIcon = new Icon(new UIScale(0.15f, 0.15f), gloves.AnimationSet.BuildAnimationsFromSet());

                glovesIcon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Gloves);
                    FillEquipmentInfo();
                };
            }
            else
            {
                glovesIcon = new Icon(new UIScale(0.15f, 0.15f), Item_1.BootPlaceholder, Spritesheets.ItemSpritesheet_1);
                glovesIcon.SetColor(_Colors.White);
            }

            glovesIcon.Hoverable = true;
            glovesIcon.HoverColor = _Colors.IconHover;

            glovesIcon.Clickable = true;
            glovesIcon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Gloves, glovesIcon, EquipmentSlot.Gloves);
            };

            glovesIcon.SetPosition(glovesBlock.Position);

            glovesBlock.AddChild(glovesIcon);
            #endregion

            #region weapon
            var weaponBlock = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Weapon] = weaponBlock;

            weaponBlock.SetPositionFromAnchor(_unitBlock.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, -10, 0), UIAnchorPosition.BottomCenter);
            _unitBlock.AddChild(weaponBlock);

            Icon weaponIcon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Weapon_1, out var weapon))
            {
                weaponIcon = new Icon(new UIScale(0.15f, 0.15f), weapon.AnimationSet.BuildAnimationsFromSet());

                weaponIcon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Weapon_1);
                    FillEquipmentInfo();
                };
            }
            else
            {
                weaponIcon = new Icon(new UIScale(0.15f, 0.15f), Item_1.WeaponPlaceholder, Spritesheets.ItemSpritesheet_1);
                weaponIcon.SetColor(_Colors.White);
            }

            weaponIcon.Hoverable = true;
            weaponIcon.HoverColor = _Colors.IconHover;

            weaponIcon.Clickable = true;
            weaponIcon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Weapon, weaponIcon, EquipmentSlot.Weapon_1);
            };

            weaponIcon.SetPosition(weaponBlock.Position);

            weaponBlock.AddChild(weaponIcon);
            #endregion

            #region jewelry_1
            var jewelry_1Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Jewelry_1] = jewelry_1Block;

            jewelry_1Block.SetPositionFromAnchor(_unitBlock.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(5, 15, 0), UIAnchorPosition.BottomLeft);
            _unitBlock.AddChild(jewelry_1Block);

            Icon jewelry_1Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Jewelry_1, out var jewelry_1))
            {
                jewelry_1Icon = new Icon(new UIScale(0.15f, 0.15f), jewelry_1.AnimationSet.BuildAnimationsFromSet());

                jewelry_1Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Jewelry_1);
                    FillEquipmentInfo();
                };
            }
            else
            {
                jewelry_1Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.JewelryPlaceholder, Spritesheets.ItemSpritesheet_1);
                jewelry_1Icon.SetColor(_Colors.White);
            }

            jewelry_1Icon.Hoverable = true;
            jewelry_1Icon.HoverColor = _Colors.IconHover;

            jewelry_1Icon.Clickable = true;
            jewelry_1Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, jewelry_1Icon, EquipmentSlot.Jewelry_1);
            };

            jewelry_1Icon.SetPosition(jewelry_1Block.Position);

            jewelry_1Block.AddChild(jewelry_1Icon);
            #endregion

            #region jewelry_2
            var jewelry_2Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Jewelry_2] = jewelry_2Block;

            jewelry_2Block.SetPositionFromAnchor(jewelry_1Icon.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitBlock.AddChild(jewelry_2Block);

            Icon jewelry_2Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Jewelry_2, out var jewelry_2))
            {
                jewelry_2Icon = new Icon(new UIScale(0.15f, 0.15f), jewelry_2.AnimationSet.BuildAnimationsFromSet());

                jewelry_2Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Jewelry_2);
                    FillEquipmentInfo();
                };
            }
            else
            {
                jewelry_2Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.JewelryPlaceholder, Spritesheets.ItemSpritesheet_1);
                jewelry_2Icon.SetColor(_Colors.White);
            }

            jewelry_2Icon.Hoverable = true;
            jewelry_2Icon.HoverColor = _Colors.IconHover;

            jewelry_2Icon.Clickable = true;
            jewelry_2Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, jewelry_2Icon, EquipmentSlot.Jewelry_2);
            };

            jewelry_2Icon.SetPosition(jewelry_2Block.Position);

            jewelry_2Block.AddChild(jewelry_2Icon);
            #endregion

            #region trinket
            var trinketBlock = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Trinket] = trinketBlock;

            trinketBlock.SetPositionFromAnchor(jewelry_2Icon.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _unitBlock.AddChild(trinketBlock);

            Icon trinketIcon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Trinket, out var trinket))
            {
                trinketIcon = new Icon(new UIScale(0.15f, 0.15f), trinket.AnimationSet.BuildAnimationsFromSet());

                trinketIcon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Trinket);
                    FillEquipmentInfo();
                };
            }
            else
            {
                trinketIcon = new Icon(new UIScale(0.15f, 0.15f), Item_1.TrinketPlaceholder, Spritesheets.ItemSpritesheet_1);
                trinketIcon.SetColor(_Colors.White);
            }

            trinketIcon.Hoverable = true;
            trinketIcon.HoverColor = _Colors.IconHover;

            trinketIcon.Clickable = true;
            trinketIcon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, trinketIcon, EquipmentSlot.Trinket);
            };

            trinketIcon.SetPosition(trinketBlock.Position);

            trinketBlock.AddChild(trinketIcon);
            #endregion

            var unitBlockPos = _unitBlock.Position;
            var botPosition = trinketIcon.GetAnchorPosition(UIAnchorPosition.BottomCenter);

            #region consumable_2
            var consumable_2Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Consumable_2] = consumable_2Block;



            consumable_2Block.SetPositionFromAnchor(new Vector3(unitBlockPos.X - 2.5f, botPosition.Y + 5, 0), UIAnchorPosition.TopRight);
            _unitBlock.AddChild(consumable_2Block);

            Icon consumable_2Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Consumable_2, out var consumable_2))
            {
                consumable_2Icon = new Icon(new UIScale(0.15f, 0.15f), consumable_2.AnimationSet.BuildAnimationsFromSet());

                consumable_2Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Consumable_2);
                    FillEquipmentInfo();
                };
            }
            else
            {
                consumable_2Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.ConsumablePlaceholder, Spritesheets.ItemSpritesheet_1);
                consumable_2Icon.SetColor(_Colors.White);
            }

            consumable_2Icon.Hoverable = true;
            consumable_2Icon.HoverColor = _Colors.IconHover;

            consumable_2Icon.Clickable = true;
            consumable_2Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, consumable_2Icon, EquipmentSlot.Consumable_2);
            };

            consumable_2Icon.SetPosition(consumable_2Block.Position);

            consumable_2Block.AddChild(consumable_2Icon);
            #endregion

            #region consumable_1
            var consumable_1Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Consumable_1] = consumable_1Block;



            consumable_1Block.SetPositionFromAnchor(consumable_2Icon.GetAnchorPosition(UIAnchorPosition.LeftCenter) + new Vector3(-5, 0, 0), UIAnchorPosition.RightCenter);
            _unitBlock.AddChild(consumable_1Block);

            Icon consumable_1Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Consumable_1, out var consumable_1))
            {
                consumable_1Icon = new Icon(new UIScale(0.15f, 0.15f), consumable_1.AnimationSet.BuildAnimationsFromSet());

                consumable_1Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Consumable_1);
                    FillEquipmentInfo();
                };
            }
            else
            {
                consumable_1Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.ConsumablePlaceholder, Spritesheets.ItemSpritesheet_1);
                consumable_1Icon.SetColor(_Colors.White);
            }

            consumable_1Icon.Hoverable = true;
            consumable_1Icon.HoverColor = _Colors.IconHover;

            consumable_1Icon.Clickable = true;
            consumable_1Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, consumable_1Icon, EquipmentSlot.Consumable_1);
            };

            consumable_1Icon.SetPosition(consumable_1Block.Position);

            consumable_1Block.AddChild(consumable_1Icon);
            #endregion

            #region consumable_3
            var consumable_3Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Consumable_3] = consumable_3Block;



            consumable_3Block.SetPositionFromAnchor(consumable_2Icon.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(5, 0, 0), UIAnchorPosition.LeftCenter);
            _unitBlock.AddChild(consumable_3Block);

            Icon consumable_3Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Consumable_3, out var consumable_3))
            {
                consumable_3Icon = new Icon(new UIScale(0.15f, 0.15f), consumable_3.AnimationSet.BuildAnimationsFromSet());

                consumable_3Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Consumable_3);
                    FillEquipmentInfo();
                };
            }
            else
            {
                consumable_3Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.ConsumablePlaceholder, Spritesheets.ItemSpritesheet_1);
                consumable_3Icon.SetColor(_Colors.White);
            }

            consumable_3Icon.Hoverable = true;
            consumable_3Icon.HoverColor = _Colors.IconHover;

            consumable_3Icon.Clickable = true;
            consumable_3Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, consumable_3Icon, EquipmentSlot.Consumable_3);
            };

            consumable_3Icon.SetPosition(consumable_3Block.Position);

            consumable_3Block.AddChild(consumable_3Icon);
            #endregion

            #region consumable_4
            var consumable_4Block = new UIBlock(default, new UIScale(0.15f, 0.15f));
            _equipmentSlots[(int)SlotMap.Consumable_4] = consumable_4Block;



            consumable_4Block.SetPositionFromAnchor(consumable_3Icon.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(5, 0, 0), UIAnchorPosition.LeftCenter);
            _unitBlock.AddChild(consumable_4Block);

            Icon consumable_4Icon;

            if (unitSelected && _selectedUnit.Info.Equipment.EquippedItems.TryGetValue(EquipmentSlot.Consumable_4, out var consumable_4))
            {
                consumable_4Icon = new Icon(new UIScale(0.15f, 0.15f), consumable_4.AnimationSet.BuildAnimationsFromSet());

                consumable_4Icon.RightClick += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.UnequipItem(EquipmentSlot.Consumable_4);
                    FillEquipmentInfo();
                };
            }
            else
            {
                consumable_4Icon = new Icon(new UIScale(0.15f, 0.15f), Item_1.ConsumablePlaceholder, Spritesheets.ItemSpritesheet_1);
                consumable_4Icon.SetColor(_Colors.White);
            }

            consumable_4Icon.Hoverable = true;
            consumable_4Icon.HoverColor = _Colors.IconHover;

            consumable_4Icon.Clickable = true;
            consumable_4Icon.Click += (s, e) =>
            {
                CreateItemSelectionDisplay(ItemType.Jewelry, consumable_4Icon, EquipmentSlot.Consumable_4);
            };

            consumable_4Icon.SetPosition(consumable_4Block.Position);

            consumable_4Block.AddChild(consumable_4Icon);
            #endregion
        }

        private List<UIBlock> _unitDisplays = new List<UIBlock>();
        private Unit _selectedUnit = null;
        private UIBlock _unitDisplayBlock;

        public void CreateUnitSelectionDisplay()
        {
            if (_unitDisplayBlock != null)
                _unitBlock.RemoveChild(_unitDisplayBlock);

            _unitDisplayBlock = new UIBlock(default, new UIScale(PlayerParty.UnitsInParty.Count * 0.26f, 0.28f));
            _unitDisplayBlock.SetPosition(WindowConstants.CenterScreen);
            _unitDisplayBlock.SetColor(_Colors.UILightGray);

            _unitDisplayBlock.RemoveChildren();
            _unitDisplayBlock.Clickable = true;
            _unitDisplayBlock.Hoverable = true;

            _unitDisplays.Clear();

            foreach (var unit in PlayerParty.UnitsInParty)
            {
                UIBlock block = new UIBlock(default, new UIScale(0.25f, 0.25f));
                Scene.Tick += block.Tick;

                block.OnCleanUp += (e) =>
                {
                    Scene.Tick -= block.Tick;
                };

                BaseObject obj = unit.CreateBaseObject();
                obj.EnableLighting = false;
                obj._currentAnimation.Pause();

                obj.BaseFrame.SetScale(block.Size.X / WindowConstants.AspectRatio, block.Size.Y, 1);

                block.BaseObjects.Clear();
                block.BaseObjects.Add(obj);
                block._baseObject = obj;

                block.Hoverable = true;
                block.HoverColor = _Colors.IconHover;

                block.SelectedColor = _Colors.IconSelected;

                if (_unitDisplays.Count == 0)
                {
                    block.SetPositionFromAnchor(_unitDisplayBlock.GetAnchorPosition(UIAnchorPosition.LeftCenter) + new Vector3(0, 0, 0),
                        UIAnchorPosition.LeftCenter);
                }
                else
                {
                    block.SetPositionFromAnchor(_unitDisplays[^1].GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(5, 0, 0),
                        UIAnchorPosition.TopLeft);
                }

                _unitDisplayBlock.AddChild(block);
                _unitDisplays.Add(block);

                block.Clickable = true;

                block.Click += (s, e) =>
                {
                    _selectedUnit = unit;
                    _unitBlock.RemoveChild(_unitDisplayBlock);
                    FillEquipmentInfo();
                };

                UIHelpers.AddTimedHoverTooltip(block, unit.Name, Scene);
            }

            _unitBlock.AddChild(_unitDisplayBlock, 1000);
        }

        private ScrollableArea _itemDisplayBlock;
        private List<UIObject> _itemDisplays = new List<UIObject>();
        public void CreateItemSelectionDisplay(ItemType itemType, UIObject selectedObject, EquipmentSlot slot)
        {
            if (_selectedUnit == null)
                return;

            if(_itemDisplayBlock != null)
            {
                _unitBlock.RemoveChild(_itemDisplayBlock);
            }

            _itemDisplays.Clear();

            var matchingItems = PlayerParty.Inventory.Items.FindAll(x => x.ItemType == itemType);


            _itemDisplayBlock = new ScrollableArea(default, new UIScale(0.9f, 0.75f), default, new UIScale(0.9f, 0.75f), enableScrollbar: false);

            _itemDisplayBlock.SetPosition(selectedObject.Position);

            _itemDisplayBlock.BaseComponent.SetColor(_Colors.Tan);
            _itemDisplayBlock.BaseComponent.Clickable = true;
            _itemDisplayBlock.BaseComponent.Hoverable = true;

            _unitBlock.AddChild(_itemDisplayBlock, 1000);

            float yPos = _itemDisplayBlock.BaseComponent.GAP(UIAnchorPosition.TopLeft).Y + 5;
            float xPos = _itemDisplayBlock.BaseComponent.GAP(UIAnchorPosition.TopLeft).X + 5;
            foreach (var item in matchingItems)
            {
                Icon itemIcon = new Icon(new UIScale(0.15f, 0.15f), item.AnimationSet.BuildAnimationsFromSet());

                if(_itemDisplays.Count != 0 && _itemDisplays.Count % 5 == 0)
                {
                    yPos = _itemDisplays[^1].GAP(UIAnchorPosition.BottomLeft).Y + 5;
                    xPos = _itemDisplays[0].GAP(UIAnchorPosition.BottomLeft).X;
                }

                itemIcon.SAP(new Vector3(xPos, yPos, 0), UIAnchorPosition.TopLeft);

                xPos = itemIcon.GAP(UIAnchorPosition.TopRight).X + 5;

                itemIcon.Hoverable = true;
                itemIcon.HoverColor = _Colors.IconHover;

                itemIcon.Clickable = true;
                itemIcon.Click += (s, e) =>
                {
                    _selectedUnit.Info.Equipment.EquipItem(item, slot);
                    _unitBlock.RemoveChild(_itemDisplayBlock);
                    FillEquipmentInfo();
                };

                UIHelpers.AddTimedHoverTooltip(itemIcon, item.Name.ToString(), Scene);

                _itemDisplays.Add(itemIcon);

                _itemDisplayBlock.BaseComponent.AddChild(itemIcon, 10);
            }
        }
    }
}
