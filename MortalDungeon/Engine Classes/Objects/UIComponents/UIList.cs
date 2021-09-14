using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
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

        public Vector4 _textColor = Colors.UITextBlack;
        public Vector4 _itemColor = Colors.UILightGray;
        public UIList(Vector3 position, UIScale listItemSize, float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, Vector4 itemColor = default, bool ascending = false, bool outline = false)
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


        public void AddItem(UIObject item, Action onClickAction) 
        {
            //if (Items.Count == 0)
            //{
            //    item.SetPosition()
            //}
        }

        //public ListItem AddItem(string text, Action onClickAction = null)
        //{
        //    Vector3 position;

        //    if (Items.Count == 0)
        //    {
        //        position = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);
        //        position.X += BaseComponent.GetAnchorPosition(UIAnchorPosition.LeftCenter).X;
        //        //position.X += Margin.X;
        //        position.Y += Margin.Y;
        //    }
        //    else 
        //    {
        //        position = Items[Items.Count - 1].BaseComponent.Position;
        //        position.Y += Items[Items.Count - 1].BaseComponent.GetDimensions().Y * (Ascending ? -1 : 1);
        //        position.Y += ItemMargins.Y * WindowConstants.ScreenUnits.Y * (Ascending ? -1 : 1);
        //    }

        //    //ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor);
        //    ListItem newItem = new ListItem(position, ListItemSize, Items.Count, text, TextScale, _textColor, _itemColor);
        //    Items.Add(newItem);
        //    AddChild(newItem, 100);

        //    //UIScale textScale = newItem._textBox.TextField.GetDimensions() * WindowConstants.AspectRatio * 2;
        //    UIScale textScale = newItem._textBox.GetDimensions() * WindowConstants.AspectRatio * 2;

        //    if (textScale.X > ListItemSize.X || textScale.Y > ListItemSize.Y) 
        //    {
        //        //ListItemSize.X = (textScale.X > ListItemSize.X ? textScale.X : ListItemSize.X) + newItem._textBox.TextOffset.ToScale().X + Margin.X * 2;
        //        ListItemSize.X = (textScale.X > ListItemSize.X ? textScale.X : ListItemSize.X) + Margin.X * 2;
        //        ListItemSize.Y = (textScale.Y > ListItemSize.Y ? textScale.Y : ListItemSize.Y);

        //        Items.ForEach(item => item.SetSize(ListItemSize));
        //        Console.WriteLine(ListItemSize);
        //    }

        //    newItem.OnClickAction = onClickAction;
        //    newItem.Clickable = true;
        //    newItem.Hoverable = true;


        //    UIScale listSize = ListItemSize + new UIScale(0, ItemMargins.Y + Margin.Y / 4);
        //    listSize.Y *= Items.Count;

        //    listSize.Y += Margin.Y;
        //    listSize.X += Margin.X;

        //    BaseComponent.SetSize(listSize);
        //    ListSize = listSize;

        //    if (Items.Count > 0) 
        //    {
        //        Position = (Items[0].BaseComponent.Position + Items[Items.Count - 1].BaseComponent.Position) / 2;
        //        //BaseComponent.SetPosition(Position);
        //    }

        //    return newItem;
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

            return newItem;
        }
    }

    public class ListItem : UIObject 
    {
        public TextComponent _textBox;
        public UIBlock _backdrop;
        public int Index = -1;

        public Vector4 _textColor = Colors.White;
        public Vector4 _itemColor = Colors.UIHoveredGray;

        public new Action<ListItem> OnClickAction;
        public ListItem(Vector3 position, UIScale listItemSize, int index, string text, float textScale, Vector4 textColor, Vector4 itemColor, bool outline = false) 
        {
            //TextBox textBox = new TextBox(position, listItemSize, text, textScale, false, new UIDimensions(20, 50));
            TextComponent textBox = new TextComponent();
            textBox.SetTextScale(textScale);
            textBox.SetText(text);

            _textBox = textBox;

            //BaseComponent = textBox;

            Name = "ListItem";

            Position = position;

            _itemColor = itemColor;
            _textColor = textColor;

            //textBox.SetTextColor(textColor);
            textBox.SetColor(textColor);
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


            AddChild(backdrop, 10);
            AddChild(textBox, 100);

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
                //_textBox.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                //_textBox.SetTextColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));

                _textBox.SetColor(_textColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
                _backdrop.SetColor(_itemColor - new Vector4(0.1f, 0.1f, 0.1f, 0));
            }
        }

        public override void OnHoverEnd()
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

        public override void OnDisabled(bool disable)
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

        public override void OnClick()
        {
            OnClickAction?.Invoke(this);
        }
    }
}
