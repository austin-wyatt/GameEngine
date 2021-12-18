using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal class UIList : UIObject
    {
        internal UIScale Margin = new UIScale(0.02f, 0.02f);
        internal UIScale ItemMargins = new UIScale(0f, 0.005f);
        internal UIScale ListItemSize = new UIScale();
        internal UIScale ListSize = new UIScale();

        internal bool Outline = false;
        internal bool Ascending = false;

        internal List<ListItem> Items = new List<ListItem>();

        internal float TextScale = 1;

        internal Vector4 _textColor = Colors.UITextBlack;
        internal Vector4 _itemColor = Colors.UILightGray;
        internal UIList(Vector3 position, UIScale listItemSize, float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, Vector4 itemColor = default, bool ascending = false, bool outline = false)
        {
            Position = position;
            ListItemSize = listItemSize;
            TextScale = textScale;
            Ascending = ascending;
            Outline = outline;

            ListSize = listItemSize;

            //Clickable = true;
            //Draggable = true;
            //Hoverable = true;

            Name = "UIList";

            //BaseComponent = new UIBlock(position, (ListItemSize + ItemMargins));
            BaseComponent = new UIBlock(position, ListItemSize);

            if (!outline) 
            {
                BaseComponent.BaseObject.OutlineParameters.SetAllInline(0);
            }

            AddChild(BaseComponent);


            if (boxColor != default)
            {
                BaseComponent.SetColor(boxColor);
            }
            else
            {
                BaseComponent.SetColor(Colors.UIDefaultGray);
            }

            //BaseComponent.SetColor(Colors.Transparent); //temp

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


        internal void AddItem(UIObject item, Action onClickAction) 
        {
            //if (Items.Count == 0)
            //{
            //    item.SetPosition()
            //}
        }

        internal ListItem AddItem(string text, Action<ListItem> onClickAction = null)
        {
            Vector3 position;

            if (Items.Count == 0)
            {
                position = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);
            }
            else
            {
                position = Items[^1].BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft);
            }

            ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor, Outline);
            Items.Add(newItem);
            AddChild(newItem, 100);


            newItem.OnClickAction = onClickAction;
            newItem.Clickable = true;
            newItem.Hoverable = true;


            UIScale listSize = new UIScale(ListItemSize);
            listSize.Y *= Items.Count;


            BaseComponent.SetSize(listSize);
            ListSize = listSize;

            if (Items.Count > 0)
            {
                Position = (Items[0].BaseComponent.Position + Items[^1].BaseComponent.Position) / 2;
                BaseComponent.SetPosition(Position);
            }

            //RescaleList();

            return newItem;
        }

        private void RescaleList() 
        {
            UIScale listSize = new UIScale(ListItemSize);
            listSize.Y *= Items.Count;

            Vector3 topLeftPos = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

            BaseComponent.SetSize(listSize);
            ListSize = listSize;

            BaseComponent.SetPositionFromAnchor(topLeftPos, UIAnchorPosition.TopLeft);

            for (int i = 0; i < Items.Count; i++) 
            {
                Vector3 position;

                if (i == 0)
                {
                    position = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);
                }
                else
                {
                    position = Items[i - 1].BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft);
                }

                Items[i].SetPositionFromAnchor(position, UIAnchorPosition.TopLeft);
            }

            //if (Items.Count > 0)
            //{
            //    Position = (Items[0].BaseComponent.Position + Items[^1].BaseComponent.Position) / 2;
            //    BaseComponent.SetPosition(Position);
            //}
        }

        internal void ClearItems() 
        {
            for (int i = 0; i < Items.Count; i++) 
            {
                RemoveChild(Items[i]);
            }

            Items.Clear();

            RescaleList();
        }

        internal void RemoveItem(ListItem item) 
        {
            RemoveChild(item);

            Items.Remove(item);

            RescaleList();
        }
    }

    internal class ListItem : UIObject 
    {
        internal TextComponent _textBox;
        internal UIBlock _backdrop;
        internal int Index = -1;

        internal Vector4 _textColor = Colors.White;
        internal Vector4 _itemColor = Colors.UIHoveredGray;

        internal new Action<ListItem> OnClickAction;
        internal ListItem(Vector3 position, UIScale listItemSize, int index, string text, float textScale, Vector4 textColor, Vector4 itemColor, bool outline = false) 
        {
            //TextBox textBox = new TextBox(position, listItemSize, text, textScale, false, new UIDimensions(20, 50));
            TextComponent textBox = new TextComponent();
            textBox.SetTextScale(textScale);
            textBox.SetText(text);
            textBox.SetColor(textColor);

            _textBox = textBox;
            

            //BaseComponent = textBox;

            Name = "ListItem";

            Position = position;

            _itemColor = itemColor;
            _textColor = textColor;

            //textBox.SetTextColor(textColor);
            //textBox.SetColor(itemColor);



            UIBlock backdrop = new UIBlock(default, listItemSize);
            backdrop.SetColor(_itemColor);
            //backdrop.MultiTextureData.MixPercent = 0;
            backdrop.MultiTextureData.MixTexture = false;

            if (!outline) 
            {
                backdrop.BaseObject.OutlineParameters.SetAllInline(0);
            }

            _backdrop = backdrop;
            BaseComponent = backdrop;

            backdrop.SetPositionFromAnchor(position, UIAnchorPosition.TopLeft);

            UIDimensions textMargins = new UIDimensions(10, 0); //TEMP

            textBox.SetPositionFromAnchor(backdrop.GetAnchorPosition(UIAnchorPosition.LeftCenter) + textMargins, UIAnchorPosition.LeftCenter);
            AddChild(textBox, 100);


            AddChild(backdrop, 10);

            Index = index;

            ValidateObject(this);
        }

        internal override void SetColor(Vector4 color)
        {
            base.SetColor(color);
            _textBox.SetColor(color);
            _itemColor = color;
        }

        internal override void OnHover()
        {
            if (Hoverable && !Hovered && !Disabled)
            {
                Hovered = true;
                //_textBox.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                //_textBox.SetTextColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));

                _textBox.SetColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                _backdrop.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
            }
        }

        internal override void OnHoverEnd()
        {
            if (Hovered && !Disabled)
            {
                Hovered = false;
                //_textBox.SetColor(_itemColor);
                //_textBox.SetTextColor(_textColor);

                _textBox.SetColor(_textColor);
                _backdrop.SetColor(_itemColor);

                base.OnHoverEnd();
            }
        }

        internal override void OnDisabled(bool disable)
        {
            base.OnDisabled(disable);

            if (Disabled)
            {
                //BaseComponent.SetColor(Colors.UIDisabledGray);
                _textBox.SetColor(Colors.UIDisabledGray);
            }
            else 
            {
                //BaseComponent.SetColor(_itemColor);
                _textBox.SetColor(_textColor);
            }
        }

        internal override void OnClick()
        {
            OnClickAction?.Invoke(this);
        }
    }
}
