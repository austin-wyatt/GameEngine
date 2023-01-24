using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UIComponents
{
    public enum ScrollbarSide
    {
        Right,
        Left
    }

    public class ScrollableArea : UIObject
    {
        public UIBlock VisibleArea;
        public Scrollbar Scrollbar;

        public UIScale _baseAreaSize;
        private float _scrollPercent = 0;

        private bool _showScrollbar = true;
        public bool EnableScrollbar = true;
        float _scrollbarWidth = 0.1f;

        public Action OnScrollAction = null;

        public bool MaintainBaseAreaRelativePosition = false;
        ScrollbarSide ScrollbarSide = ScrollbarSide.Right;

        public ScrollableArea(Vector3 position, UIScale visibleAreaSize, Vector3 baseAreaPosition, UIScale baseAreaSize, 
            float scrollbarWidth = 0.1f, bool enableScrollbar = true, bool setScrollable = true, bool scaleAspectRatio = true,
            ScrollbarSide scrollSide = ScrollbarSide.Right) 
        {
            Size = visibleAreaSize;
            Position = position;
            Name = "ScrollableArea";
            Anchor = UIAnchorPosition.Center;
            EnableScrollbar = enableScrollbar;
            ScrollbarSide = scrollSide;

            _scaleAspectRatio = scaleAspectRatio;


            _baseAreaSize = baseAreaSize;

            _scrollbarWidth = scrollbarWidth;

            _showScrollbar = _baseAreaSize.Y != Size.Y;
            if(!_showScrollbar)
                _baseAreaSize.Y += 0.00001f;

            VisibleArea = new UIBlock(Position, Size, default, 71, _scaleAspectRatio);
            VisibleArea.Name = "VisibleArea";

            VisibleArea.SetColor(new Vector4(0, 1, 0, 0));
            VisibleArea.SetAllInline(0);

            if (setScrollable)
            {
                VisibleArea.Scrollable = true;

                VisibleArea.Scroll += (s, mouseState) =>
                {
                    OnUpdate(mouseState);
                };
            }

            UIBlock scrollableArea = new UIBlock(default, baseAreaSize, default, 71, _scaleAspectRatio);
            scrollableArea.MultiTextureData.MixTexture = false;
            scrollableArea._baseObject.OutlineParameters.SetAllInline(0);
            scrollableArea.Name = "ScrollableAreaMainComp";
            scrollableArea.SetColor(new Vector4(0, 0, 0, 0));
            //scrollableArea.SetColor(new Vector4(1, 0, 0, 1));

            BaseComponent = scrollableArea;


            //scrollableArea.Draggable = true;

            SetVisibleAreaPosition(Position);


            InitializeScrollbar();

            AddChild(scrollableArea);
            AddChild(VisibleArea);

            //BaseComponent.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);

            //AddChild(VisibleArea); //FOR DEBUGGING
        }

        public override void SetPosition(Vector3 position)
        {
            SetVisibleAreaPosition(position, Anchor, MaintainBaseAreaRelativePosition);

            base.SetPosition(position);
        }

        public override void CleanUp()
        {
            base.CleanUp();

            VisibleArea.CleanUp();
        }

        public void SetVisibleAreaPosition(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center, bool maintainBaseAreaRelativePosition = false) 
        {
            Anchor = anchor;
            Position = position;

            Vector3 visibleDeltaPos = new Vector3(VisibleArea.Position);

            VisibleArea.SetPositionFromAnchor(position, anchor);

            visibleDeltaPos = VisibleArea.Position - visibleDeltaPos;

            Vector3 scissorAreaPos = VisibleArea.Position;

            if (BaseComponent.ScissorData.ScissoredArea == null)
            {
                ScissorData scissor = new ScissorData()
                {
                    Scissor = true,
                };

                BaseComponent.ScissorData = scissor;
            }

            BaseComponent.ScissorData.ScissoredArea.SetPosition(scissorAreaPos);
            BaseComponent.ScissorData.ScissoredArea.SetSize(VisibleArea.GetDimensions(), scaleAspectRatio: false);


            UpdateScrollableAreaBounds();

            if (maintainBaseAreaRelativePosition)
            {
                BaseComponent.SetPosition(BaseComponent.Position + visibleDeltaPos);
            }
            else
            {
                BaseComponent.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            }


            InitializeScrollbar(_scrollPercent);
        }

        public void SetVisibleAreaSize(UIScale size) 
        {
            VisibleArea.SetSize(size);
            Size = size;

            _showScrollbar = _baseAreaSize.Y != Size.Y;
            if (!_showScrollbar)
                _baseAreaSize.Y += 0.00001f;

            SetVisibleAreaPosition(Position, Anchor);

            InitializeScrollbar();
            SetScrollbarPosition();
            //Scrollbar.ScrollByPercentage(_scrollPercent);
        }

        public void SetBaseAreaSize(UIScale size) 
        {
            Vector3 oldTopLeft = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);


            _baseAreaSize = size;

            _showScrollbar = _baseAreaSize.Y != Size.Y;
            if (!_showScrollbar)
                _baseAreaSize.Y += 0.00001f;

            BaseComponent.SetSize(_baseAreaSize);

            Vector3 newTopLeft = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

            BaseComponent.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            InitializeScrollbar();
            SetScrollbarPosition();
            //Scrollbar.ScrollByPercentage(_scrollPercent);

            Vector3 offset = new Vector3(oldTopLeft.X - newTopLeft.X, newTopLeft.Y - oldTopLeft.Y, 0);

            for (int i = 0; i < BaseComponent.Children.Count; i++) 
            {
                BaseComponent.Children[i].SetPosition(BaseComponent.Children[i].Position + offset);    
            }
            
        }
        
        public void UpdateScrollableAreaBounds() 
        {
            AdditionalBounds = VisibleArea._baseObject.Bounds;
            BaseComponent.ForEach((obj) =>
            {
                obj.AdditionalBounds = VisibleArea._baseObject.Bounds;
            });
        }

        public void SetScrollbarPosition() 
        {
            switch (ScrollbarSide)
            {
                case ScrollbarSide.Right:
                    Scrollbar.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopLeft);
                    break;
                case ScrollbarSide.Left:
                    Scrollbar.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopRight);
                    break;
            }
            
        }

        private void InitializeScrollbar(float scrollPercent = 0) 
        {
            Texture tex = null;

            //if (Scrollbar != null) 
            //{
            //    tex = Scrollbar._baseObject.BaseFrame.Material.Diffuse;

            //    RemoveChild(Scrollbar.ObjectID);
            //}

            Vector3 visibleTopRight = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight);
            Vector3 visibleBottomRight = VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomRight);

            float J = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight).Y;
            float K = VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomRight).Y;

            float A = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft).Y;
            float B = BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

            float H = (J - K) * (J - K) / (A - B) * -1;

            UIScale scrollbarScale = new UIScale(_scrollbarWidth, UIScale.CoordToScale(H * 2));

            if(Scrollbar == null)
            {
                Scrollbar = new Scrollbar(new Vector3(), scrollbarScale, new Scrollbar.ScrollInfo(new Vector2(K, J)), H);

                Scrollbar.OnScrollAction = OnScroll;

                VisibleArea.AddChild(Scrollbar, 10000);
            }
            else
            {
                Scrollbar.SetScrollBounds(new Scrollbar.ScrollInfo(new Vector2(K, J)));
            }

            //Scrollbar = new Scrollbar(new Vector3(), scrollbarScale, new Scrollbar.ScrollInfo(new Vector2(K, J)), H);

            SetScrollbarPosition();

            //Scrollbar.OnScrollAction = OnScroll;

            //AddChild(Scrollbar, 10000);

            if (tex != null) 
            {
                Scrollbar.GetDisplay().Material.Diffuse = tex;
            }

            Scrollbar.ScrollByPercentage(scrollPercent);

            Scrollbar.SetRender(_showScrollbar && EnableScrollbar);
        }

        public override void OnUpdate(MouseState mouseState)
        {
            base.OnUpdate(mouseState);

            bool scrolled = false;
            if (mouseState.ScrollDelta[1] < 0)
            {
                _scrollPercent += 0.05f;
                if (_scrollPercent > 1)
                {
                    _scrollPercent = 1;
                }
                scrolled = true;
            }
            else if (mouseState.ScrollDelta[1] > 0)
            {
                _scrollPercent -= 0.05f;

                if (_scrollPercent < 0) 
                {
                    _scrollPercent = 0;
                }
                
                scrolled = true;
            }

            if (scrolled) 
            {
                Scrollbar.ScrollByPercentage(_scrollPercent);
            }
        }

        public void OnScroll(float percent) 
        {
            if (!EnableScrollbar)
                return;

            Vector3 scrollableRange = new Vector3();

            Vector3 pos = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft);

            float J = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight).Y;
            float K = VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomRight).Y;

            float A = J;

            float H = (BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft).Y - BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft).Y) * -1;
            float B = A + H;

            pos.Y = A - (B + (J - K) - A) * percent;

            _scrollPercent = percent;

            BaseComponent.SetPositionFromAnchor(pos + scrollableRange, UIAnchorPosition.TopLeft);

            OnScrollAction?.Invoke();
        }

        public override void OnResize() 
        {
            SetVisibleAreaSize(Size);
        }

        public void FitToChildren()
        {
            float newY = 0;
            foreach(var child in BaseComponent.Children)
            {
                float childYPos = child.GetDimensions().Y + child.GAP(UIAnchorPosition.TopLeft).Y;

                if (childYPos > BaseComponent.GAP(UIAnchorPosition.BottomLeft).Y)
                {
                    float y = BaseComponent.GAP(UIAnchorPosition.TopLeft).Y + childYPos;

                    if(y > newY)
                    {
                        newY = y;
                    }
                }
            }

            if(newY != 0)
            {
                SetBaseAreaSize(new UIScale(_baseAreaSize.X, newY / WindowConstants.ScreenUnits.Y));
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
        }
    }

    public class Scrollbar : UIBlock 
    {
        public ScrollInfo Bounds = new ScrollInfo();

        public Action<float> OnScrollAction = null;

        private float H;
        public Scrollbar(Vector3 position, UIScale size = default, ScrollInfo bounds = default, float h = 0) 
            : base(position, size)
        {
            Name = "Scrollbar";
            H = h;

            SetScrollBounds(bounds);

            Draggable = true;
            Clickable = true;
        }

        public void SetScrollBounds(ScrollInfo bounds) 
        {
            Bounds = bounds;
        }

        public override void DragEvent(Vector3 position, Vector3 mouseCoord, Vector3 deltaDrag)
        {
            UIAnchorPosition anchor = UIAnchorPosition.Center;
            if (Bounds.ScrollX)
            {
                //position.X = position.X < XBounds[0] ? XBounds[0] : position.X > XBounds[1] ? XBounds[1] : position.X;
            }
            else 
            {
                position.X = Position.X;
            }

            if (Bounds.ScrollY)
            {
                Vector3 topRight = GetAnchorPosition(UIAnchorPosition.TopRight, position);
                Vector3 bottomRight = GetAnchorPosition(UIAnchorPosition.BottomRight, position);

                if (topRight.Y < Bounds.Max) 
                {
                    anchor = UIAnchorPosition.TopCenter;
                    position.Y = Bounds.Max;
                }
                else if (bottomRight.Y > (Bounds.Min))
                {
                    anchor = UIAnchorPosition.BottomCenter;
                    position.Y = (Bounds.Min);
                }
            }
            else 
            {
                position.Y = Position.Y;
            }

            OnScrollAction?.Invoke(GetScrollPercentage());

            SetPositionFromAnchor(position, anchor);
        }

        public void ScrollByPercentage(float percent) 
        {
            Vector3 pos = Position;

            float T = Bounds.Max + H / 2;
            float B = Bounds.Min - H / 2;

            pos.Y = T - (T - B) * percent;

            SetPositionFromAnchor(pos);
            OnScrollAction?.Invoke(GetScrollPercentage());
        }

        public float GetScrollPercentage() 
        {
            float percentage = 0;

            float T = Bounds.Max + H / 2;
            float B = Bounds.Min - H / 2;

            float Y = Position.Y;

            percentage = (T - Y) / (T - B);

            return percentage;
        }

        public class ScrollInfo 
        {
            public float Min = 0;
            public float Max = 0;

            public bool ScrollX = false;
            public bool ScrollY = false;

            public ScrollInfo() { }

            public ScrollInfo(Vector2 bounds, bool scrollX = false)
            {
                Min = bounds[0];
                Max = bounds[1];

                ScrollX = scrollX;
                ScrollY = !ScrollX;
            }
        }
    }
}
