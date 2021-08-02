using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    class UIList : UIObject
    {
        public UIScale Margin = new UIScale(0.02f, 0.02f);
        public UIScale ItemMargins = new UIScale(0f, 0.005f);
        public UIScale ListItemSize = new UIScale();
        public UIScale ListSize = new UIScale();

        public bool Ascending = false;

        public List<ListItem> Items = new List<ListItem>();

        public float TextScale = 1;

        public Vector4 _textColor = Colors.White;
        public Vector4 _itemColor = Colors.UIHoveredGray;
        public UIList(Vector3 position, UIScale listItemSize, float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, Vector4 itemColor = default, bool ascending = false)
        {
            Position = position;
            ListItemSize = listItemSize;
            TextScale = textScale;
            Ascending = ascending;

            ListSize = listItemSize;

            //Clickable = true;
            //Draggable = true;
            //Hoverable = true;

            Name = "UIList";

            BaseComponent = new UIBlock(position, (ListItemSize + ItemMargins));

            AddChild(BaseComponent);


            if (boxColor != default)
            {
                BaseComponent.SetColor(boxColor);
            }
            else
            {
                BaseComponent.SetColor(Colors.UIDefaultGray);
            }

            if (textColor != default)
            {
                _textColor = textColor;
            }

            if (itemColor != default) 
            {
                _itemColor = itemColor;
            }

            ValidateObject(this);
        }


        public void AddItem(UIObject item, Action onClickAction) 
        {
            //if (Items.Count == 0)
            //{
            //    item.SetPosition()
            //}
        }

        public ListItem AddItem(string text, Action onClickAction = null)
        {
            Vector3 position;

            if (Items.Count == 0)
            {
                position = BaseComponent.Origin;
                position.X += BaseComponent.GetAnchorPosition(UIAnchorPosition.LeftCenter).X;
                //position.X += Margin.X;
                position.Y += Margin.Y;
            }
            else 
            {
                position = Items[Items.Count - 1].BaseComponent.Position;
                position.Y += Items[Items.Count - 1].BaseComponent.GetDimensions().Y * (Ascending ? -1 : 1);
                position.Y += ItemMargins.Y * WindowConstants.ScreenUnits.Y * (Ascending ? -1 : 1);
            }

            //ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor);
            ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor);
            Items.Add(newItem);
            AddChild(newItem, 100);

            UIScale textScale = newItem._textBox.TextField.GetTextDimensions() * WindowConstants.AspectRatio * 2;

            if (textScale.X > ListItemSize.X || textScale.Y > ListItemSize.Y) 
            {
                ListItemSize.X = (textScale.X > ListItemSize.X ? textScale.X : ListItemSize.X) + newItem._textBox.TextOffset.ToScale().X + Margin.X * 2;
                ListItemSize.Y = (textScale.Y > ListItemSize.Y ? textScale.Y : ListItemSize.Y);

                Items.ForEach(item => item.SetSize(ListItemSize));
                Console.WriteLine(ListItemSize);
            }

            newItem.OnClickAction = onClickAction;
            newItem.Clickable = true;
            newItem.Hoverable = true;


            UIScale listSize = ListItemSize + new UIScale(0, ItemMargins.Y + Margin.Y / 4);
            listSize.Y *= Items.Count;

            listSize.Y += Margin.Y;
            listSize.X += Margin.X;

            BaseComponent.SetSize(listSize);
            ListSize = listSize;

            if (Items.Count > 0) 
            {
                Position = (Items[0].BaseComponent.Position + Items[Items.Count - 1].BaseComponent.Position) / 2;
                BaseComponent.SetPosition(Position);
            }

            return newItem;
        }

        //public override void SetSize(Vector2 size)
        //{
        //    Children.ForEach()

        //    base.SetSize(size);
        //}

        //public void SetListSize(Vector2 listItemSize) 
        //{
        //    ListItemSize = listItemSize;
        //}
    }

    public class ListItem : UIObject 
    {
        public TextBox _textBox;
        public int Index = -1;

        public Vector4 _textColor = Colors.White;
        public Vector4 _itemColor = Colors.UIHoveredGray;
        public ListItem(Vector3 position, UIScale listItemSize, int index, string text, float textScale, Vector4 textColor, Vector4 itemColor) 
        {
            TextBox textBox = new TextBox(position, listItemSize, text, textScale, false, new UIDimensions(20, 50));
            _textBox = textBox;

            BaseComponent = textBox;

            Name = "ListItem";

            Position = position;

            _itemColor = itemColor;
            _textColor = textColor;

            textBox.SetTextColor(textColor);
            textBox.SetColor(itemColor);

            AddChild(textBox);

            Index = index;

            ValidateObject(this);
        }

        public override void SetColor(Vector4 color)
        {
            base.SetColor(color);
            _textBox.SetColor(color);
            _itemColor = color;
        }

        public override void OnHover()
        {
            if (Hoverable && !Hovered && !Disabled)
            {
                Hovered = true;
                _textBox.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                _textBox.SetTextColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
            }
        }

        public override void HoverEnd()
        {
            if (Hovered && !Disabled)
            {
                Hovered = false;
                _textBox.SetColor(_itemColor);
                _textBox.SetTextColor(_textColor);

                base.HoverEnd();
            }
        }

        public override void OnDisabled(bool disable)
        {
            base.OnDisabled(disable);

            if (Disabled)
            {
                BaseComponent.SetColor(Colors.UIDisabledGray);
            }
            else 
            {
                BaseComponent.SetColor(_itemColor);
            }
        }
    }
}
