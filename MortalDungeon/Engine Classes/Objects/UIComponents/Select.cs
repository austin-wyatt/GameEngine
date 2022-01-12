using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class SelectItem 
    {
        public string Name = "";
        public Action OnSelect = null;
    }
    public class Select : UIObject
    {
        public List<SelectItem> Items = new List<SelectItem>();
        public SelectItem SelectedItem = null;
        private SelectItem _emptyItem = new SelectItem();

        public UIList List;
        private Icon Chevron;

        public Select(UIScale listItemSize, float textScale = 0.1f) 
        {
            List = new UIList(default, listItemSize, textScale);

            Chevron = new Icon(new UIScale(listItemSize.Y, listItemSize.Y), UISheetIcons.Chevron, Spritesheets.UISheet);
            Chevron.SetColor(_Colors.UITextBlack);

            Chevron.BaseObject.BaseFrame.RotateZ(270);
            Chevron.BaseObject.BaseFrame.ScaleY(1 / WindowConstants.AspectRatio);

            BaseComponent = List;

            AddChild(BaseComponent);

            AddChild(Chevron, 100);

            ItemSelected(_emptyItem);
        }

        public SelectItem AddItem(string name, Action onSelect = null) 
        {
            SelectItem item = new SelectItem() { Name = name, OnSelect = onSelect };

            Items.Add(item);

            return item;
        }

        public void ClearItems() 
        {
            Items.Clear();
            ItemSelected(_emptyItem);
        }

        public void CreateItemList() 
        {
            Chevron.SetRender(false);

            List.ClearItems();

            List.AddItem("", (_) => ItemSelected(_emptyItem));
            Items.ForEach(item =>
            {
                List.AddItem(item.Name, (_) => ItemSelected(item));
            });

            ForceTreeRegeneration();
        }

        public void ItemSelected(SelectItem item) 
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

            ForceTreeRegeneration();
        }
    }
}
