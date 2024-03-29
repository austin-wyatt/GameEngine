﻿using Empyrean.Engine_Classes.TextHandling;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Empyrean.Engine_Classes.UIComponents
{
    public class UIList : UIObject
    {
        public UIScale Margin = new UIScale(0.02f, 0.02f);
        public UIScale ItemMargins = new UIScale(0f, 0.005f);
        public UIScale ListItemSize = new UIScale();
        public UIScale ListSize = new UIScale();

        public bool Outline = false;
        public bool Ascending = false;

        public List<ListItem> Items = new List<ListItem>();

        public float TextScale = 1;

        public Vector4 _textColor = _Colors.UITextBlack;
        public Vector4 _itemColor = _Colors.UILightGray;

        public UIList(Vector3 position, UIScale listItemSize, float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, Vector4 itemColor = default, bool ascending = false, bool outline = false)
        {
            Position = position;
            ListItemSize = listItemSize;
            //TextScale = textScale;
            Ascending = ascending;
            Outline = outline;

            ListSize = listItemSize;

            _scaleAspectRatio = true;

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
                BaseComponent.SetColor(_Colors.UIDefaultGray);
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


        //public void AddItem(UIObject item, Action onClickAction) 
        //{
        //    //if (Items.Count == 0)
        //    //{
        //    //    item.SetPosition()
        //    //}
        //}

        public ListItem AddItem(string text, Action<ListItem> onClickAction = null)
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

            ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor, Outline, _scaleAspectRatio);
            Items.Add(newItem);
            AddChild(newItem, 100);

            //newItem._backdrop.SetColor(new Vector4((float)new Random().NextDouble(), (float)new Random().NextDouble(), 0, 1));

            if(onClickAction != null)
            {
                newItem.Click += (s, e) =>
                {
                    onClickAction?.Invoke(newItem);
                };
            }

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

        //public override UIDimensions GetDimensions()
        //{
        //    UIDimensions dimensions = new UIDimensions();

        //    if(Items.Count > 0)
        //    {
        //        dimensions.X = Items[0].GAP(UIAnchorPosition.TopRight).X - Items[0].GAP(UIAnchorPosition.TopLeft).X;
        //        dimensions.Y = Items[^1].GAP(UIAnchorPosition.BottomLeft).Y - Items[0].GAP(UIAnchorPosition.TopLeft).Y;
        //    }

        //    return dimensions;
        //}

        public void ClearItems() 
        {
            //for (int i = 0; i < Items.Count; i++) 
            //{
            //    RemoveChild(Items[i]);
            //}

            RemoveChildren(Items);

            Items.Clear();

            RescaleList();
        }

        public void RemoveItem(ListItem item) 
        {
            RemoveChild(item);

            Items.Remove(item);

            RescaleList();
        }
    }

    public class ListItem : UIObject 
    {
        public Text_Drawing _textBox;
        public UIBlock _backdrop;
        public int Index = -1;

        public Vector4 _textColor = _Colors.White;
        public Vector4 _itemColor = _Colors.UIHoveredGray;

        public ListItem(Vector3 position, UIScale listItemSize, int index, string text, float textScale, 
            Vector4 textColor, Vector4 itemColor, bool outline = false, bool scaleAspectRatio = true) 
        {
            //TextBox textBox = new TextBox(position, listItemSize, text, textScale, false, new UIDimensions(20, 50));
            Text_Drawing textBox = new Text_Drawing(text, Text_Drawing.DEFAULT_FONT, 14, Brushes.Black);
            textBox.SetTextScale(textScale);

            _textBox = textBox;

            _textBox.HoverColor = new Vector4(0.8f, 0.8f, 0.8f, 1);


            //BaseComponent = textBox;

            Name = "ListItem";

            Position = position;

            _itemColor = itemColor;
            _textColor = textColor;

            //textBox.SetTextColor(textColor);
            //textBox.SetColor(itemColor);



            UIBlock backdrop = new UIBlock(default, listItemSize, scaleAspectRatio: scaleAspectRatio);
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

        public override void SetColor(Vector4 color, SetColorFlag setColorFlag = SetColorFlag.Base)
        {
            base.SetColor(color, setColorFlag);
            _textBox.SetColor(color);
            _itemColor = color;
        }

        public override void OnHover()
        {
            if (Hoverable && !Hovered && !Disabled)
            {
                Hovered = true;
                //_textBox.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                //_textBox.SetTextColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));

                _textBox.OnHover();
                _backdrop.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));

                HoverEvent(this);
            }
        }

        public override void OnHoverEnd()
        {
            if (Hovered && !Disabled)
            {
                Hovered = false;
                //_textBox.SetColor(_itemColor);
                //_textBox.SetTextColor(_textColor);
                _textBox.OnHoverEnd();
                _backdrop.SetColor(_itemColor);

                HoverEndEvent(this);
            }
        }

        public override void OnDisabled(bool disable)
        {
            base.OnDisabled(disable);

            if (Disabled)
            {
                //BaseComponent.SetColor(Colors.UIDisabledGray);
                _textBox.SetColor(_Colors.UIDisabledGray);
            }
            else 
            {
                //BaseComponent.SetColor(_itemColor);
                _textBox.SetColor(_textColor);
            }
        }
    }
}
