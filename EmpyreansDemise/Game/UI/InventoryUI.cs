using Empyrean.Definitions;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.TextHandling;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Items;
using Empyrean.Game.Player;
using Empyrean.Game.Serializers;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Icon = Empyrean.Engine_Classes.UIComponents.Icon;

namespace Empyrean.Game.UI
{
    public class InventoryUI
    {
        public UIObject Window;
        public CombatScene Scene;

        public bool Displayed = false;

        public InventoryUI(CombatScene scene)
        {
            Scene = scene;
        }

        public void CreateWindow()
        {
            RemoveWindow();

            Window = UIHelpers.CreateWindow(new UIScale(2 * WindowConstants.AspectRatio, 2f), "Inventory", null, Scene, customExitAction: () =>
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

        private ScrollableArea _itemsScrollableArea;
        public void PopulateData()
        {
            Text_Drawing inventoryLabel = new Text_Drawing("Inventory", Text_Drawing.DEFAULT_FONT, 48, Brushes.Black);
            inventoryLabel.SetTextScale(0.1f);

            inventoryLabel.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(inventoryLabel);

            Icon goldIcon = new Icon(new UIScale(0.1f, 0.1f), UIControls.Gold, Spritesheets.UIControlsSpritesheet);
            goldIcon.SetPositionFromAnchor(inventoryLabel.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(25, 0, 0), UIAnchorPosition.LeftCenter);
            Window.AddChild(goldIcon);


            Text_Drawing goldLabel = new Text_Drawing($"{PlayerParty.Inventory.Gold}", Text_Drawing.DEFAULT_FONT, 48, Brushes.Black);
            goldLabel.SetTextScale(0.075f);

            goldLabel.SetPositionFromAnchor(goldIcon.GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(10, 0, 0), UIAnchorPosition.LeftCenter);
            Window.AddChild(goldLabel);

            _itemsScrollableArea = new ScrollableArea(default, new UIScale(2, 1.5f), default, new UIScale(2, 1.5f), enableScrollbar: false);
            _itemsScrollableArea.SetVisibleAreaPosition(inventoryLabel.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(_itemsScrollableArea, 100);

            UIBlock backdrop = new UIBlock(default, new UIScale(2, 1.5f));
            backdrop.SetPosition(_itemsScrollableArea.VisibleArea.Position);
            backdrop.SetColor(_Colors.Tan);
            Window.AddChild(backdrop, 50);

            _backdrop = backdrop;

            _selectedItemArea = new UIBlock(default, new UIScale(1, 1.5f));
            _selectedItemArea.SetPositionFromAnchor(backdrop.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);
            _selectedItemArea.SetColor(_Colors.Tan);
            Window.AddChild(_selectedItemArea);

            AddItems();
        }

        Item _selectedItem = null;

        UIBlock _selectedItemArea;
        UIBlock _backdrop;
        List<UIObject> _itemIcons = new List<UIObject>();
        public void AddItems()
        {
            _itemsScrollableArea.BaseComponent.RemoveChildren(_itemIcons);
            _itemIcons.Clear();

            Vector3 visibleBotRight = _itemsScrollableArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomRight);

            int rows = 0;
            int columns = 0;
            int columnCount = 0;

            foreach (var item in PlayerParty.Inventory.Items)
            {
                var icon = item.Generate(new UIScale(0.2f, 0.2f));

                if (_itemIcons.Count == 0)
                {
                    icon.SetPositionFromAnchor(_itemsScrollableArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(5, 5, 0), UIAnchorPosition.TopLeft);
                }
                else
                {
                    icon.SetPositionFromAnchor(_itemIcons[^1].GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

                    if(icon.GetAnchorPosition(UIAnchorPosition.TopRight).X > visibleBotRight.X)
                    {
                        columnCount = columns;
                        columns = 1;
                        icon.SetPositionFromAnchor(_itemIcons[rows * columnCount].GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
                        rows++;
                    }
                    else
                    {
                        columns++;
                    }
                }

                icon.Hoverable = true;
                icon.HoverColor = _Colors.IconHover;

                icon.Clickable = true;
                icon.Click += (s, e) =>
                {
                    SelectItem(item);
                };

                UIHelpers.AddTimedHoverTooltip(icon, item.Name.ToString(), Scene);

                _itemsScrollableArea.BaseComponent.AddChild(icon);

                _itemIcons.Add(icon);
            }
        }

        public void SelectItem(Item item)
        {
            _selectedItemArea.RemoveChildren();
            _selectedItem = item;

            Text_Drawing itemLabel = new Text_Drawing(item.Name.ToString(), Text_Drawing.DEFAULT_FONT, 48, Brushes.Black);
            itemLabel.SetTextScale(0.1f);

            itemLabel.SetPositionFromAnchor(_selectedItemArea.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            _selectedItemArea.AddChild(itemLabel);

            Text_Drawing descriptionLabel = new Text_Drawing(UIHelpers.WrapString(item.Description.ToString(), 30), Text_Drawing.DEFAULT_FONT, 32, Brushes.Black);
            descriptionLabel.SetTextScale(0.075f);

            descriptionLabel.SetPositionFromAnchor(itemLabel.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            _selectedItemArea.AddChild(descriptionLabel);

            string info = $"Sellable: {(item.Sellable ? "Yes" : "No")}\n" +
                $"Value: {item.SellPrice}\n" +
                $"Type: {item.ItemType.Name()}\n";

            if (item.Stackable)
            {
                info += $"Stack size: {item.StackSize}\n";
            }

            Text_Drawing infoLabel = new Text_Drawing(info, Text_Drawing.DEFAULT_FONT, 32, Brushes.Black);
            infoLabel.SetTextScale(0.075f);

            infoLabel.SetPositionFromAnchor(_selectedItemArea.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(10, 10, 0), UIAnchorPosition.BottomLeft);
            _selectedItemArea.AddChild(infoLabel);

            if (!item.Unique)
            {
                Icon deleteIcon = new Icon(new UIScale(0.1f, 0.1f), UI_1.Cancel, Spritesheets.UISpritesheet_1);

                deleteIcon.SetPositionFromAnchor(_selectedItemArea.GetAnchorPosition(UIAnchorPosition.BottomRight) + new Vector3(-10, -10, 0), UIAnchorPosition.BottomRight);

                deleteIcon.Clickable = true;
                deleteIcon.Click += (s, e) =>
                {
                    UIHelpers.CreateFocusedPopup("Discard item?", Scene, UIHelpers.FocusedPopupOptions.OkCancel, () =>
                    {
                        PlayerParty.Inventory.RemoveItemFromInventory(item);
                        _selectedItemArea.RemoveChildren();
                        AddItems();
                    });
                };

                UIHelpers.AddTimedHoverTooltip(deleteIcon, "Discard item", Scene);

                _selectedItemArea.AddChild(deleteIcon);
            }
        }
    }
}
