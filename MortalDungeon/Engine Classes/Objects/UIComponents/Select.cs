using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal class SelectItem 
    {
        internal string Name = "";
        internal Action OnSelect = null;
    }
    internal class Select : UIObject
    {
        internal List<SelectItem> Items = new List<SelectItem>();
        internal SelectItem SelectedItem = null;
        private SelectItem _emptyItem = new SelectItem();

        internal UIList List;
        private Icon Chevron;

        internal Select(UIScale listItemSize, float textScale = 0.1f) 
        {
            List = new UIList(default, listItemSize, textScale);

            Chevron = new Icon(new UIScale(listItemSize.Y, listItemSize.Y), UISheetIcons.Chevron, Spritesheets.UISheet);
            Chevron.SetColor(Colors.UITextBlack);

            Chevron.BaseObject.BaseFrame.RotateZ(270);
            Chevron.BaseObject.BaseFrame.ScaleY(1 / WindowConstants.AspectRatio);

            BaseComponent = List;

            AddChild(BaseComponent);

            AddChild(Chevron, 100);

            ItemSelected(_emptyItem);
        }

        internal SelectItem AddItem(string name, Action onSelect = null) 
        {
            SelectItem item = new SelectItem() { Name = name, OnSelect = onSelect };

            Items.Add(item);

            return item;
        }

        internal void ClearItems() 
        {
            Items.Clear();
            ItemSelected(_emptyItem);
        }

        internal void CreateItemList() 
        {
            Chevron.SetRender(false);

            List.ClearItems();

            List.AddItem("", (_) => ItemSelected(_emptyItem));
            Items.ForEach(item =>
            {
                List.AddItem(item.Name, (_) => ItemSelected(item));
            });
        }

        internal void ItemSelected(SelectItem item) 
        {
            SelectedItem = item;

            SelectedItem.OnSelect?.Invoke();

            List.ClearItems();
            List.AddItem(item.Name, (_) => 
            {
                CreateItemList();
            });

            Chevron.SetPositionFromAnchor(List.GetAnchorPosition(UIAnchorPosition.RightCenter), UIAnchorPosition.RightCenter);
            Chevron.SetRender(true);
        }
    }
}
