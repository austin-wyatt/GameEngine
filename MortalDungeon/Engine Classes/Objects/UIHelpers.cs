using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Icon = MortalDungeon.Engine_Classes.UIComponents.Icon;
using MortalDungeon.Definitions;

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
        TimedHover,
        HoverEnd,
        Scroll
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

            public bool EnforceScreenBounds;

            public StringTooltipParameters(CombatScene scene, string text, GameObject hoverParent, UIObject baseObject) 
            {
                Scene = scene;
                Text = text;
                HoverParent = hoverParent;
                TooltipParent = baseObject;

                TooltipFlag = GeneralContextFlags.UITooltipOpen;
                Position = default;
                Anchor = UIAnchorPosition.BottomLeft;
                BackgroundColor = _Colors.UILightGray;
                TextScale = 0.1f;

                EnforceScreenBounds = true;
            }
        }
        public static void CreateToolTip(StringTooltipParameters param)
        {
            string tooltipName = "Tooltip" + param.TooltipFlag;

            if (param.Scene.ContextManager.GetFlag(param.TooltipFlag)) 
            {
                for (int i = param.Scene._tooltipBlock.Children.Count - 1; i >= 0; i--)
                {
                    if (param.Scene._tooltipBlock.Children[i].Name == tooltipName) 
                    {
                        param.Scene._tooltipBlock.RemoveChild(param.Scene._tooltipBlock.Children[i]);
                    }
                }
            }
                

            param.Scene.ContextManager.SetFlag(param.TooltipFlag, true);

            Vector3 backgroundOffset = new Vector3(-5, -10, -0.001f);

            Text tooltip = new Text(param.Text, Text.DEFAULT_FONT, 32, Brushes.Black);
            tooltip.SetTextScale(param.TextScale);
            tooltip.Hoverable = true;

            //tooltip.RenderAfterParent = true;

            if (param.Position == default)
            {
                tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(Window._cursorCoords) + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);
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

            UIScale tooltipScale = new UIScale(tooltip.Size);
            //tooltipScale.Y += 0.05f;
            //tooltipScale.X += 0.05f;

            tooltipBackground.SetSize(tooltipScale);
            tooltipBackground.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
            //tooltip.SetPosition(new Vector3(tooltip.Position.X, tooltip.Position.Y, tooltipBackground.Position.Z - 0.001f));
            tooltip.SetPosition(tooltip.Position);

            tooltipBackground.AddChild(tooltip);

            tooltipBackground.Name = tooltipName;

            //param.TooltipParent.AddChild(tooltip, 100000);
            param.TooltipParent.AddChild(tooltipBackground, 99999);


            void tempScene(SceneEventArgs args)
            {
                if (args.EventAction != EventHandlerAction.CloseTooltip) return;

                //param.TooltipParent.RemoveChild(tooltip.ObjectID);
                param.TooltipParent.RemoveChild(tooltipBackground);
                param.HoverParent.HoverEnd -= tempGameObj;
                param.Scene.ContextManager.SetFlag(param.TooltipFlag, false);

                param.Scene.OnUIForceClose -= tempScene;
            }

            void tempGameObj(GameObject obj)
            {
                //param.TooltipParent.RemoveChild(tooltip.ObjectID);
                param.TooltipParent.RemoveChild(tooltipBackground);
                param.HoverParent.HoverEnd -= tempGameObj;
                param.Scene.ContextManager.SetFlag(param.TooltipFlag, false);

                param.Scene.OnUIForceClose -= tempScene;
            }

            param.HoverParent.HoverEnd += tempGameObj;
            param.Scene.OnUIForceClose += tempScene;

            if (param.EnforceScreenBounds) 
            {
                CheckTooltipPlacement(tooltipBackground, param.Scene);
            }
        }


        public static void CreateToolTip(CombatScene scene, Tooltip tooltip, UIObject tooltipParent, UIObject baseObject, GeneralContextFlags tooltipFlag = GeneralContextFlags.UITooltipOpen)
        {
            if (scene.ContextManager.GetFlag(tooltipFlag))
                return;

            scene.ContextManager.SetFlag(tooltipFlag, true);

            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(Window._cursorCoords) + new Vector3(0, -30, 0)), UIAnchorPosition.BottomLeft);

            baseObject.AddChild(tooltip, 100000);

            void tempScene(SceneEventArgs args)
            {
                if (args.EventAction != EventHandlerAction.CloseTooltip) return;

                baseObject.RemoveChild(tooltip);
                tooltipParent.HoverEnd -= tempGameObj;
                scene.ContextManager.SetFlag(tooltipFlag, false);

                scene.OnUIForceClose -= tempScene;
            }

            void tempGameObj(GameObject args)
            {
                baseObject.RemoveChild(tooltip);
                tooltipParent.HoverEnd -= tempGameObj;
                scene.ContextManager.SetFlag(tooltipFlag, false);

                scene.OnUIForceClose -= tempScene;
            }

            tooltipParent.HoverEnd += tempGameObj;
            scene.OnUIForceClose += tempScene;

            CheckTooltipPlacement(tooltip, scene);
        }

        public static void NukeTooltips(GeneralContextFlags tooltipFlag, CombatScene scene) 
        {
            string tooltipName = "Tooltip" + tooltipFlag;

            for (int i = scene._tooltipBlock.Children.Count - 1; i >= 0; i--)
            {
                if (scene._tooltipBlock.Children[i].Name == tooltipName)
                {
                    scene._tooltipBlock.RemoveChild(scene._tooltipBlock.Children[i]);
                }
            }
        }

        public static Tooltip GenerateTooltipWithHeader(string headerText, string bodyText)
        {
            Tooltip tooltip = new Tooltip();

            Text header = new Text(headerText, Text.DEFAULT_FONT, 64, Brushes.Black);
            header.SetTextScale(0.1f);

            Text description = new Text(bodyText, Text.DEFAULT_FONT, 64, Brushes.Black);
            description.SetTextScale(0.075f);
            description.SetColor(_Colors.UITextBlack);

            tooltip.AddChild(header);
            tooltip.AddChild(description);

            header.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            tooltip.Margins = new UIDimensions(0, 30);

            tooltip.FitContents();
            tooltip.BaseComponent.SetPosition(tooltip.Position);

            header.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            return tooltip;
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


            tooltip.SetPositionFromAnchor(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(Window._cursorCoords)) + new Vector3(10, 0, 0), UIAnchorPosition.BottomLeft);

            baseObject.AddChild(tooltip, 100000);

            void temp()
            {
                baseObject.RemoveChild(tooltip);
                scene.ContextManager.SetFlag(contextFlag, false);
            }

            scene._closeContextMenu = temp;

            CheckTooltipPlacement(tooltip, scene);
        }

        public static (Tooltip tooltip, UIList itemList) GenerateContextMenuWithList(string headerText)
        {
            Tooltip menu = new Tooltip();

            Text header = new Text(headerText, Text.DEFAULT_FONT, 64, Brushes.Black);
            header.SetTextScale(0.1f);

            menu.AddChild(header);

            float textScale = 0.075f;
            UIList list = new UIList(default, new UIScale(header.Scale.X > 0.75f ? 0.75f : header.Scale.X, 0.1f), textScale, default, _Colors.UITextBlack, _Colors.UILightGray);


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

        public static void CheckTooltipPlacement(UIObject tooltip, Scene scene) 
        {
            Vector3 topLeft = tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft);
            Vector3 botRight = tooltip.GetAnchorPosition(UIAnchorPosition.BottomRight);

            bool right = false;
            bool top = false;
            bool bot = false;

            if (botRight.X > WindowConstants.ScreenUnits.X) 
            {
                right = true;
            }

            if (botRight.Y > WindowConstants.ScreenUnits.Y)
            {
                bot = true;
            }

            if (topLeft.Y < 0) 
            {
                top = true;
            }

            if (!(right || top || bot))
                return;

            Vector3 mousePos = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(Window._cursorCoords));

            mousePos.Z = tooltip.Position.Z;

            if (right && top)
            {
                tooltip.SetPositionFromAnchor(mousePos, UIAnchorPosition.TopRight);
            }
            else if (right && bot || top)
            {
                tooltip.SetPositionFromAnchor(mousePos, UIAnchorPosition.TopLeft);
            }
            else if (right)
            {
                tooltip.SetPositionFromAnchor(mousePos, UIAnchorPosition.BottomRight);
            }
        }

        public static void AddTimedHoverTooltip(UIObject obj, string text, CombatScene scene)
        {
            obj.HasTimedHoverEffect = true;
            obj.Hoverable = true;

            void timedHover(GameObject obj)
            {
                StringTooltipParameters param = new StringTooltipParameters(scene, "", obj, scene._tooltipBlock)
                {
                    Text = text
                };
                CreateToolTip(param);
            }

            obj.TimedHover += timedHover;

            void onCleanUp(GameObject _) 
            {
                _.OnHoverEnd();
                obj.TimedHover -= timedHover;
            }

            obj.OnCleanUp += onCleanUp;
        }

        public static void AddTimedHoverTooltip(UIObject obj, string header, string body, CombatScene scene)
        {
            obj.HasTimedHoverEffect = true;
            obj.Hoverable = true;

            void timedHover(GameObject _)
            {
                var tooltip = GenerateTooltipWithHeader(header, body);
                CreateToolTip(scene, tooltip, obj, scene._tooltipBlock);
            }

            obj.TimedHover += timedHover;

            void onCleanUp(GameObject _)
            {
                _.OnHoverEnd();
                obj.TimedHover -= timedHover;
            }

            obj.OnCleanUp += onCleanUp;
        }

        public static UIObject CreateWindow(UIScale size, string name, UIObject parent, CombatScene scene, 
            bool enforceUniqueness = false, bool createExitButton = true, Action customExitAction = null) 
        {
            if (enforceUniqueness) 
            {
                for (int i = parent.Children.Count - 1; i >= 0; i--) 
                {
                    if (parent.Children[i].Name == name)
                    {
                        parent.RemoveChild(parent.Children[i]);
                    }
                }
                
            }

            UIBlock window = new UIBlock(new Vector3(500, 500, 0), size);
            window.MultiTextureData.MixTexture = false;
            window.Draggable = true;
            window.Clickable = true;
            window.Hoverable = true;

            window.Name = name;

            if (createExitButton) 
            {
                Icon exit = new Icon(new UIScale(0.075f, 0.075f), UI_1.Exit, Spritesheets.UISpritesheet_1);
                exit.Clickable = true;
                exit.Click += (s, e) =>
                {
                    if(customExitAction != null) 
                        customExitAction?.Invoke();
                    else
                        parent.RemoveChild(window);

                    exit.OnHoverEnd();
                };
                exit.SetPositionFromAnchor(window.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-5, 5, 0), UIAnchorPosition.TopRight);

                AddTimedHoverTooltip(exit, "Exit", scene);

                window.AddChild(exit);
            }
            

            return window;
        }

        public static void CreateIconHoverEffect(Icon icon, CombatScene scene, Vector3 position) 
        {
            icon.SetCameraPerspective(true);

            foreach(var item in icon.BaseObjects)
            {
                item.EnableLighting = true;
            }

            icon.SetPosition(position);

            scene._genericObjects.Add(icon);

            PropertyAnimation anim = new PropertyAnimation(icon.BaseObject.BaseFrame);

            float xMovement = (float)(new ConsistentRandom().NextDouble() - 1) * 10f;

            for (int i = 0; i < 50; i++)
            {
                Keyframe frame = new Keyframe(i * 2)
                {
                    Action = () =>
                    {
                        icon.SetPosition(icon.Position + new Vector3(xMovement, -10, 0.015f));
                        icon.SetColor(icon.BaseObject.BaseFrame.BaseColor - new Vector4(0, 0, 0, 0.02f));
                    }
                };

                anim.Keyframes.Add(frame);
            }

            icon.AddPropertyAnimation(anim);
            anim.Play();

            anim.OnFinish = () =>
            {
                icon.RemovePropertyAnimation(anim.AnimationID);
                scene._genericObjects.Remove(icon);
                scene.Tick -= icon.Tick;
            };

            scene.Tick -= icon.Tick;
            scene.Tick += icon.Tick;
        }

        public enum FocusedPopupOptions
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
        }
        public static void CreateFocusedPopup(string text, CombatScene scene, FocusedPopupOptions options, Action okYes, Action no = null, Action cancel = null)
        {
            Text message = new Text(WrapString(text, 50), Text.DEFAULT_FONT, 32, Brushes.Black);
            message.SetTextScale(0.075f);

            UIBlock window = new UIBlock(default, new UIScale(message.Size.X + 0.1f, message.Size.Y + 0.15f));

            message.SetPositionFromAnchor(window.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);
            window.AddChild(message);

            window.SetPosition(WindowConstants.CenterScreen);

            Button createOkButton()
            {
                Button okButton = new Button(default, new UIScale(0.2f, 0.075f), "Ok", 0.2f);
                okButton.Click += (s, e) =>
                {
                    onClose();
                    okYes?.Invoke();
                };

                okButton.SetPositionFromAnchor(window.GetAnchorPosition(UIAnchorPosition.BottomRight) + new Vector3(-5, -5, 0), UIAnchorPosition.BottomRight);
                window.AddChild(okButton);

                return okButton;
            }

            Button createCancelButton()
            {
                Button cancelButton = new Button(default, new UIScale(0.2f, 0.075f), "Cancel", 0.19f);
                cancelButton.Click += (s, e) =>
                {
                    onClose();
                    cancel?.Invoke();
                };

                window.AddChild(cancelButton);

                return cancelButton;
            }

            switch (options)
            {
                case FocusedPopupOptions.Ok:
                    createOkButton();
                    break;
                case FocusedPopupOptions.OkCancel:
                    var okButton = createOkButton();
                    var cancelButton = createCancelButton();

                    cancelButton.SetPositionFromAnchor(okButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(-5, 0, 0), UIAnchorPosition.BottomRight);
                    break;
            }


            void onClose()
            {
                scene.UIManager.RemoveUIObject(window);
                scene.UIManager.ClearExclusiveFocus();
            }

            scene.UIManager.AddUIObject(window, 99999);
            scene.UIManager.ExclusiveFocusObject(window);
        }


        public static string WrapString(string line, int maxWidth)
        {
            if (line.Length <= maxWidth)
                return line;

            string returnString = "";

            char[] testChars = new char[] { ' ', '"', '@' };

            //search for space
            bool matchFound = false;

            foreach (char c in testChars)
            {
                if (matchFound)
                    break;

                for (int i = maxWidth; i > 1; i--)
                {
                    bool foundChar = false;

                    switch (c)
                    {
                        case ' ':
                            foundChar = line[i] == ' ';
                            break;
                        case '"':
                            foundChar = line[i] == '"' || line[i] == '\'' || line[i] == '.' || line[i] == ',' || line[i] == '!' ||
                                        line[i] == ':' || line[i] == ';' || line[i] == '?';
                            break;
                        case '@':
                            foundChar = line[i] == '@' || line[i] == '#' || line[i] == '$' || line[i] == '%' || line[i] == '^' ||
                                        line[i] == '&' || line[i] == '*' || line[i] == '(' || line[i] == ')' || line[i] == '{' ||
                                        line[i] == '}' || line[i] == '[' || line[i] == ']' || line[i] == '/' || line[i] == '\\' ||
                                        line[i] == '|' || line[i] == '<' || line[i] == '>';
                            break;
                    }

                    if (foundChar)
                    {
                        if (c == ' ')
                        {
                            returnString = line.Substring(0, i) + "\n";
                            line = line.Substring(i + 1);
                        }
                        else
                        {
                            returnString = line.Substring(0, i + 1) + "\n";
                            line = line.Substring(i + 1);
                        }


                        if (line.Length > maxWidth)
                        {
                            returnString += WrapString(line, maxWidth);
                        }
                        else
                        {
                            returnString += line;
                        }
                        matchFound = true;
                        break;
                    }
                }
            }

            if (matchFound == false)
            {
                returnString = line.Substring(0, maxWidth) + "\n";

                line = line.Substring(maxWidth + 1);

                if (line.Length > maxWidth)
                {
                    returnString += WrapString(line, maxWidth);
                }
                else
                {
                    returnString += line;
                }
            }


            return returnString;
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
            return "UIScale {" + X + ", " + Y + "}";
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
