using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class ScrollableArea : UIObject
    {
        public UIBlock VisibleArea;
        public Scrollbar Scrollbar;

        private UIScale _baseAreaSize;
        private float _scrollPercent = 0;
        public ScrollableArea(Vector3 position, UIScale visibleAreaSize, Vector3 baseAreaPosition, UIScale baseAreaSize) 
        {
            Size = visibleAreaSize;
            Position = position;
            Name = "ScrollableArea";
            Anchor = UIAnchorPosition.TopLeft;

            Focusable = true;

            _baseAreaSize = baseAreaSize;

            VisibleArea = new UIBlock(Position, Size, default, 71, true);
            VisibleArea.Name = "VisibleArea";

            UIBlock scrollableArea = new UIBlock(baseAreaPosition, baseAreaSize, default, 71, true);
            scrollableArea.MultiTextureData.MixTexture = false;
            scrollableArea._baseObject.OutlineParameters.SetAllInline(0);
            scrollableArea.Name = "ScrollableAreaMainComp";
            scrollableArea.SetColor(new Vector4(0, 0, 0, 0));

            BaseComponent = scrollableArea;

            SetVisibleAreaPosition(Position);

            scrollableArea.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft),UIAnchorPosition.TopLeft);

            InitializeScrollbar();

            AddChild(scrollableArea);
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            SetVisibleAreaPosition(position, Anchor);
            InitializeScrollbar();
        }

        public void SetVisibleAreaPosition(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center) 
        {
            VisibleArea.SetPositionFromAnchor(position, anchor);

            Vector3 globalCoord = WindowConstants.ConvertScreenSpaceToGlobalCoordinates(VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomLeft));
            Vector3 globalCoordTopRight = WindowConstants.ConvertScreenSpaceToGlobalCoordinates(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight));
            BaseComponent.ScissorData = new ScissorData()
            {
                Scissor = true,
                X = (int)globalCoord.X,
                Y = (int)(WindowConstants.ClientSize.Y - globalCoord.Y),
                Width = (int)(globalCoordTopRight.X - globalCoord.X),
                Height = (int)(globalCoord.Y - globalCoordTopRight.Y),
                Depth = 1000
            };

            UpdateScrollableAreaBounds();
        }

        public void SetVisibleAreaSize(UIScale size) 
        {
            VisibleArea.SetSize(size);
            Size = size;

            SetVisibleAreaPosition(Position, UIAnchorPosition.TopLeft);
            InitializeScrollbar();
            SetScrollbarPosition();
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
            Scrollbar.SetPositionFromAnchor(VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopLeft);
        }

        private void InitializeScrollbar() 
        {
            Texture tex = null;
            if (Scrollbar != null) 
            {
                tex = Scrollbar.GetDisplay().TextureReference;
                RemoveChild(Scrollbar.ObjectID);
            }

            Vector3 visibleTopRight = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight);
            Vector3 visibleBottomRight = VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomRight);

            UIScale scrollbarScale = new UIScale(0.1f, Size.Y / _baseAreaSize.Y);
            Scrollbar = new Scrollbar(new Vector3(), scrollbarScale, new Scrollbar.ScrollInfo(new Vector2(visibleBottomRight.Y, visibleTopRight.Y)));

            SetScrollbarPosition();

            Scrollbar.OnScrollAction = OnScroll;

            AddChild(Scrollbar, 10000);

            if (tex != null) 
            {
                Scrollbar.GetDisplay().TextureReference = tex;
            }
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

            //OnScroll(_scrollPercent);
        }

        public void OnScroll(float percent) 
        {
            Vector3 scrollableRange = new Vector3();

            Vector3 pos = VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft);

            if (Scrollbar.Bounds.ScrollY)
            {
                scrollableRange = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) - BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft);
                scrollableRange.Y *= percent;
            }

            _scrollPercent = percent;

            BaseComponent.SetPositionFromAnchor(pos + scrollableRange, UIAnchorPosition.TopLeft);
        }

        public override void OnResize() 
        {
            SetVisibleAreaSize(Size);
        }
    }

    public class Scrollbar : UIBlock 
    {
        public ScrollInfo Bounds = new ScrollInfo();

        public Action<float> OnScrollAction = null;

        private float _sizeCorrection = 0;

        public Scrollbar(Vector3 position, UIScale size = default, ScrollInfo bounds = default) 
            : base(position, size)
        {
            Name = "Scrollbar";

            SetScrollBounds(bounds);

            Draggable = true;
        }

        public void SetScrollBounds(ScrollInfo bounds) 
        {
            Bounds = bounds;

            UIDimensions offset = GetAnchorOffset(UIAnchorPosition.TopRight);

            if (Bounds.ScrollX)
            {
                _sizeCorrection = offset.X;
            }
            else 
            {
                _sizeCorrection = offset.Y;
            }
        }

        public override void SetDragPosition(Vector3 position)
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
                else if (bottomRight.Y > Bounds.Min)
                {
                    anchor = UIAnchorPosition.BottomCenter;
                    position.Y = Bounds.Min;
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
            UIAnchorPosition anchor = UIAnchorPosition.Center;

            pos.Y = Bounds.Max + percent * (Bounds.Min - Bounds.Max) - _sizeCorrection;

            Vector3 topRight = GetAnchorPosition(UIAnchorPosition.TopRight, pos);
            Vector3 bottomRight = GetAnchorPosition(UIAnchorPosition.BottomRight, pos);

            if (topRight.Y < Bounds.Max)
            {
                anchor = UIAnchorPosition.TopCenter;
                pos.Y = Bounds.Max;
            }
            else if (bottomRight.Y > Bounds.Min)
            {
                anchor = UIAnchorPosition.BottomCenter;
                pos.Y = Bounds.Min;
            }

            SetPositionFromAnchor(pos, anchor);
            OnScrollAction?.Invoke(GetScrollPercentage());
        }

        public float GetScrollPercentage() 
        {
            if (Bounds.ScrollX)
            {
                return (Position.X + _sizeCorrection - Bounds.Max) / (Bounds.Min - Bounds.Max + _sizeCorrection);
            }
            else if (Bounds.ScrollY) 
            {
                return (Position.Y + _sizeCorrection - Bounds.Max) / (Bounds.Min - Bounds.Max);
            }

            return 0;
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
