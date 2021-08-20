using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public enum UIEventType
    {
        None,
        Click,
        RightClick,
        Hover,
        MouseDown,
        Grab,
        KeyDown,
        Focus,
        TimedHover
    }

    public enum UIAnchorPosition
    {
        Center,
        TopCenter,
        BottomCenter,
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
        LeftCenter,
        RightCenter
    }

    public static class UIHelpers
    {
        public struct UIBorders
        {
            public bool Left;
            public bool Right;
            public bool Top;
            public bool Bottom;
        }

        public const int BLOCK_WIDTH = 500;
        public const int BLOCK_HEIGHT = 500;

        public static class Borders
        {
            public static readonly UIBorders None = new UIBorders();
            public static readonly UIBorders All = new UIBorders { Bottom = true, Top = true, Left = true, Right = true };
            public static readonly UIBorders LeftOnly = new UIBorders { Left = true };
            public static readonly UIBorders RightOnly = new UIBorders { Right = true };
            public static readonly UIBorders TopOnly = new UIBorders { Top = true };
            public static readonly UIBorders BottomOnly = new UIBorders { Bottom = true };
            public static readonly UIBorders TopLeft = new UIBorders { Top = true, Left = true };
            public static readonly UIBorders BottomLeft = new UIBorders { Bottom = true, Left = true };
            public static readonly UIBorders TopRight = new UIBorders { Top = true, Right = true };
            public static readonly UIBorders BottomRight = new UIBorders { Bottom = true, Right = true };
            public static readonly UIBorders TopBottom = new UIBorders { Top = true, Bottom = true };

            public static readonly UIBorders OpenTop = new UIBorders { Bottom = true, Left = true, Right = true };
            public static readonly UIBorders OpenLeft = new UIBorders { Bottom = true, Top = true, Right = true };
            public static readonly UIBorders OpenBottom = new UIBorders { Top = true, Left = true, Right = true };
            public static readonly UIBorders OpenRight = new UIBorders { Bottom = true, Top = true, Left = true };
        }

        public static readonly Texture UI_BACKGROUND = Texture.LoadFromFile("Resources/FogTexture.png");

        public static readonly Vector3 BaseMargin = new Vector3(10, 10, 0);
        public static readonly Vector3 BaseVerticalMargin = new Vector3(0, 10, 0);
        public static readonly Vector3 BaseHorizontalMargin = new Vector3(10, 0, 0);

        public static void AddAbilityIconHoverEffect(UIObject obj, CombatScene scene, Ability ability)
        {
            void onHover()
            {
                obj.SetColor(Colors.IconHover);
            }

            void hoverEnd()
            {
                if (scene._selectedAbility != null && scene._selectedAbility.AbilityID == ability.AbilityID)
                {
                    obj.SetColor(Colors.IconSelected);
                }
                else
                {
                    obj.SetColor(Colors.White);
                }
            }

            obj._onHoverActions.Add(onHover);
            obj._onHoverEndActions.Add(hoverEnd);
        }

        public struct StringTooltipParameters 
        {
            public CombatScene Scene;
            public string Text;
            public GameObject HoverParent;
            public UIObject TooltipParent;
            public GeneralContextFlags TooltipFlag;
            public Vector3 Position;
            public UIAnchorPosition Anchor;
            public Vector4 BackgroundColor;
            public float TextScale;

            public StringTooltipParameters(CombatScene scene, string text, GameObject hoverParent, UIObject baseObject) 
            {
                Scene = scene;
                Text = text;
                HoverParent = hoverParent;
                TooltipParent = baseObject;

                TooltipFlag = GeneralContextFlags.UITooltipOpen;
                Position = default;
                Anchor = UIAnchorPosition.BottomLeft;
                BackgroundColor = Colors.UILightGray;
                TextScale = 0.05f;
            }
        }
        public static void CreateToolTip(StringTooltipParameters param)
        {
            if (param.Scene.ContextManager.GetFlag(param.TooltipFlag))
                return;

            param.Scene.ContextManager.SetFlag(param.TooltipFlag, true);

            Vector3 backgroundOffset = new Vector3(-5, -10, 0);

            TextComponent tooltip = new TextComponent();
            tooltip.SetColor(Colors.UITextBlack);
            tooltip.SetText(param.Text);
            tooltip.SetTextScale(param.TextScale);
            tooltip.Hoverable = true;

            if (param.Position == default)
            {
                tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(param.Scene._cursorObject.Position + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);
            }
            else
            {
                if (param.Anchor == UIAnchorPosition.TopRight)
                {
                    tooltip.SetPositionFromAnchor(param.Position - new Vector3(-backgroundOffset.X, backgroundOffset.Y, 0), param.Anchor);
                }
                else 
                {
                    tooltip.SetPositionFromAnchor(param.Position - backgroundOffset, param.Anchor);
                }
            }



            UIBlock tooltipBackground = new UIBlock();
            tooltipBackground.SetColor(param.BackgroundColor);
            tooltipBackground.MultiTextureData.MixTexture = false;

            UIScale tooltipScale = tooltip.GetScale();
            tooltipScale.Y += 0.05f;
            tooltipScale.X += 0.05f;

            tooltipBackground.SetSize(tooltipScale);
            tooltipBackground.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft) + backgroundOffset, UIAnchorPosition.TopLeft);

            param.TooltipParent.AddChild(tooltip, 100000);
            param.TooltipParent.AddChild(tooltipBackground, 99999);

            void temp()
            {
                param.TooltipParent.RemoveChild(tooltip.ObjectID);
                param.TooltipParent.RemoveChild(tooltipBackground.ObjectID);
                param.HoverParent._onHoverEndActions.Remove(temp);
                param.Scene.ContextManager.SetFlag(param.TooltipFlag, false);
            }

            param.HoverParent._onHoverEndActions.Add(temp);
        }


        public static void CreateToolTip(CombatScene scene, Tooltip tooltip, UIObject tooltipParent, UIObject baseObject, GeneralContextFlags tooltipFlag = GeneralContextFlags.UITooltipOpen)
        {
            if (scene.ContextManager.GetFlag(tooltipFlag))
                return;

            scene.ContextManager.SetFlag(tooltipFlag, true);

            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(scene._cursorObject.Position + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);

            baseObject.AddChild(tooltip, 100000);

            void temp()
            {
                baseObject.RemoveChild(tooltip.ObjectID);
                tooltipParent._onHoverEndActions.Remove(temp);
                scene.ContextManager.SetFlag(tooltipFlag, false);
            }

            tooltipParent._onHoverEndActions.Add(temp);
        }

        /// <summary>
        /// Creates a context menu based on the passed Tooltip object. Returns an action that will delete the context menu when invoked (or null if a context menu is open already)
        /// </summary>
        public static void CreateContextMenu(CombatScene scene, Tooltip tooltip, UIObject baseObject, GeneralContextFlags contextFlag = GeneralContextFlags.ContextMenuOpen)
        {
            if (scene.ContextManager.GetFlag(contextFlag))
                return;

            if (tooltip == null)
                return;

            scene.ContextManager.SetFlag(contextFlag, true);


            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(scene._cursorObject.Position), UIAnchorPosition.BottomLeft);

            baseObject.AddChild(tooltip, 100000);

            void temp()
            {
                baseObject.RemoveChild(tooltip.ObjectID);
                scene.ContextManager.SetFlag(contextFlag, false);
            }

            scene._closeContextMenu = temp;
        }

        public static (Tooltip tooltip, UIList itemList) GenerateContextMenuWithList(string headerText)
        {
            Tooltip menu = new Tooltip();

            TextComponent header = new TextComponent();
            header.SetTextScale(0.05f);
            header.SetColor(Colors.UITextBlack);
            header.SetText(headerText);

            menu.AddChild(header);

            float textScale = 0.04f;
            UIList list = new UIList(default, Text.GetTextDimensions(20, 1, textScale).ToScale() + new UIScale(0, 0.05f), textScale, default, Colors.UITextBlack, Colors.UILightGray);


            menu.AddChild(list);


            //initially position the objects so that the tooltip can be fitted
            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            list.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            //menu.Margins = new UIDimensions(0, 60);

            menu.FitContents(false);

            //position the objects again once the menu has been fitted to the correct size
            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            list.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            menu.Clickable = true;

            menu.BaseComponent.BaseObject.OutlineParameters.SetAllInline(0);

            return (menu, list);
        }
    }

    public class UIDimensions
    {
        public Vector2 _dimensions;
        public float X { get { return _dimensions.X; } set { _dimensions.X = value; } }
        public float Y { get { return _dimensions.Y; } set { _dimensions.Y = value; } }

        public static UIDimensions operator +(UIDimensions a, UIDimensions b) => new UIDimensions(a.X + b.X, a.Y + b.Y);
        public static Vector3 operator +(Vector3 b, UIDimensions a) => new Vector3(a.X + b.X, a.Y + b.Y, b.Z);
        public static UIDimensions operator -(UIDimensions a, UIDimensions b) => new UIDimensions(a.X - b.X, a.Y - b.Y);
        public static Vector3 operator -(Vector3 a, UIDimensions b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z);
        public static UIDimensions operator *(UIDimensions a, float f) => new UIDimensions(a.X * f, a.Y * f);
        public static UIDimensions operator /(UIDimensions a, float f) => new UIDimensions(a.X / f, a.Y / f);

        public static implicit operator UIScale(UIDimensions self)
        {
            return self.ToScale();
        }

        public static implicit operator Vector3(UIDimensions self)
        {
            return new Vector3(self.X, self.Y, 0);
        }

        public static implicit operator UIDimensions(Vector3 vec) 
        {
            return new UIDimensions(vec.X, vec.Y);
        }

        public UIDimensions()
        {
            _dimensions = new Vector2();
        }

        public UIDimensions(Vector2 dimensions)
        {
            _dimensions = dimensions;
        }

        public UIDimensions(Vector3 dimensions)
        {
            _dimensions = new Vector2(dimensions.X, dimensions.Y);
        }

        public UIDimensions(float x, float y)
        {
            _dimensions = new Vector2(x, y);
        }

        public UIScale ToScale()
        {
            return new UIScale(_dimensions.X / WindowConstants.ScreenUnits.X, _dimensions.Y / WindowConstants.ScreenUnits.Y);
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }
    }

    public class UIScale
    {
        public Vector2 _scale;
        public float X { get { return _scale.X; } set { _scale.X = value; } }
        public float Y { get { return _scale.Y; } set { _scale.Y = value; } }


        public static UIScale operator +(UIScale a, UIScale b) => new UIScale(a.X + b.X, a.Y + b.Y);
        public static UIScale operator -(UIScale a, UIScale b) => new UIScale(a.X - b.X, a.Y - b.Y);
        public static UIScale operator *(UIScale a, float f) => new UIScale(a.X * f, a.Y * f);
        public static UIScale operator /(UIScale a, float f) => new UIScale(a.X / f, a.Y / f);

        public static implicit operator UIDimensions(UIScale self)
        {
            return self.ToDimensions();
        }

        public UIScale()
        {
            _scale = new Vector2();
        }

        public UIScale(Vector2 scale)
        {
            _scale = scale;
        }

        public UIScale(UIScale scale)
        {
            _scale = new Vector2(scale.X, scale.Y);
        }

        public UIScale(float x, float y)
        {
            _scale = new Vector2(x, y);
        }

        public UIDimensions ToDimensions()
        {
            return new UIDimensions(_scale.X * WindowConstants.ScreenUnits.X, _scale.Y * WindowConstants.ScreenUnits.Y);
        }

        public static float CoordToScale(float coord)
        {
            return coord / WindowConstants.ScreenUnits.X;
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }
    }

    public class BoundingArea
    {
        public float MinX = 0;
        public float MaxX = 0;
        public float MinY = 0;
        public float MaxY = 0;

        public bool InBoundingArea(Vector3 position) 
        {
            return !(position.X < MinX || position.X > MaxX || position.Y > MinY || position.Y < MaxY);
        }

        public void UpdateBoundingArea(float minX, float maxX, float minY, float maxY) 
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }
    }
}
