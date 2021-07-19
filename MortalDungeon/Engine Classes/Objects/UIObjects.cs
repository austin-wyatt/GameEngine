using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using static MortalDungeon.Game.UI.UIHelpers;

namespace MortalDungeon.Game.UI
{
    public static class UIHelpers
    {
        public struct UIBorders
        {
            public bool Left;
            public bool Right;
            public bool Top;
            public bool Bottom;

            public static bool operator ==(UIBorders operand, UIBorders operand2)
            {
                return operand.Left == operand2.Left && operand.Right == operand2.Right && operand.Top == operand2.Top && operand.Bottom == operand2.Bottom;
            }
            public static bool operator !=(UIBorders operand, UIBorders operand2)
            {
                return !(operand == operand2);
            }
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

        public enum BoundsCheckType 
        {
            None,
            MouseUp,
            Hover,
            MouseDown,
            Grab
        }
    }

    public class UIObject : GameObject
    {
        public List<UIObject> Children = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        public List<Text> TextObjects = new List<Text>();
        public Vector3 Origin = default; //this will be the top left of the UIBlock
        public Vector2 Size = new Vector2(1, 1);
        public bool CameraPerspective = false;

        public bool Grabbed = false;
        public bool Draggable = false;
        public bool Hoverable = false;
        public bool Hovered = false;

        public bool Disabled = false;

        public bool Selected = false; //when selected, SetColor behaviour changes

        public UIObject Parent = null;

        public Vector3 _grabbedDeltaPos = default;
        protected Vector3 _originOffset = default;

        public Action OnClickAction = null;

        public UIObject() { }

        public void SetOrigin(float aspectRatio, Vector2 ScaleFactor) 
        {
            Origin = new Vector3(Position.X - Position.X * aspectRatio * ScaleFactor.X / 2, Position.Y - Position.Y * ScaleFactor.Y / 2, Position.Z);
            _originOffset.X = Position.X - Origin.X;
            _originOffset.Y = Position.Y - Origin.Y;
            _originOffset.Z = Position.Z - Origin.Z;
        }

        //public virtual void UpdateBorders(UIBorders borders) { }
        public override void Tick()
        {
            base.Tick();

            Children.ForEach((child) => 
            {
                child.Tick();
            });
        }

        public virtual RenderableObject GetDisplay(UIObject uiObj = null) 
        {
            UIObject obj = uiObj == null ? this : uiObj;

            RenderableObject display;
          
            if (obj.BaseObjects.Count > 0)
            {
                display = obj.BaseObjects[0].GetDisplay();
            }
            else 
            {
                display = GetDisplay(obj.Children[0]); //this assumes that a UIObject will always have either a nested object or a BaseObject
            }

            return display;
        }

        public override void ScaleAll(float f)
        {
            ScaleAllRecursive(this, f);

            Scale *= f;
        }
        private void ScaleAllRecursive(UIObject uiObj, float f) 
        {
            uiObj.BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAll(f);
            });

            uiObj.Children.ForEach(obj =>
            {
                obj.ScaleAllRecursive(obj, f);
            });
        }

        public override void ScaleAddition(float f)
        {
            ScaleAdditionRecursive(this, f);

            Scale.X += f;
            Scale.Y += f;
            Scale.Z += f;
        }
        private void ScaleAdditionRecursive(UIObject uiObj, float f)
        {
            uiObj.BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAddition(f);
            });

            uiObj.Children.ForEach(obj =>
            {
                obj.ScaleAdditionRecursive(obj, f);
            });
        }

        public void BoundsCheck(Vector2 MouseCoordinates, Camera camera, Action<UIObject> optionalAction = null, BoundsCheckType type = BoundsCheckType.MouseUp)
        {
            if(Children.Count > 0) //evaluate all children of the UIObject
            {
                ForEach(uiObj =>
                {
                    uiObj.BaseObjects.ForEach(obj =>
                    {
                        if (Clickable && obj.Clickable)
                        {
                            if (obj.Bounds.Contains(MouseCoordinates, camera))
                            {
                                UIObject validObj = GetFirstValidParent(uiObj, type);
                                switch (type)
                                {
                                    case BoundsCheckType.MouseUp:
                                        validObj.OnMouseUp();
                                        break;
                                    case BoundsCheckType.Hover:
                                        validObj.OnHover();
                                        break;
                                    case BoundsCheckType.MouseDown:
                                        validObj.OnMouseDown();
                                        break;
                                    case BoundsCheckType.Grab:
                                        validObj.OnGrab(MouseCoordinates, validObj);
                                        break;
                                }

                                optionalAction?.Invoke(validObj);
                            }
                            else if (type == BoundsCheckType.Hover && Hovered)
                            {
                                HoverEnd();
                            }
                        }
                    });
                });
            }

            if (BaseObjects.Count > 0) //evaluate all BaseObjects of the UIObject
            {
                BaseObjects.ForEach(obj =>
                {
                    if (Clickable && obj.Clickable)
                    {
                        if (obj.Bounds.Contains(MouseCoordinates, camera))
                        {
                            UIObject validObj = GetFirstValidParent(this, type);
                            switch (type)
                            {
                                case BoundsCheckType.MouseUp:
                                    validObj.OnMouseUp();
                                    break;
                                case BoundsCheckType.Hover:
                                    validObj.OnHover();
                                    break;
                                case BoundsCheckType.MouseDown:
                                    validObj.OnMouseDown();
                                    break;
                                case BoundsCheckType.Grab:
                                    validObj.OnGrab(MouseCoordinates, validObj);
                                    break;
                            }

                            optionalAction?.Invoke(validObj);
                        }
                        else if (type == BoundsCheckType.Hover && Hovered)
                        {
                            HoverEnd();
                        }
                    }
                });
            }
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            Vector3 basePosition = default;

            if(Children.Count > 0)
                basePosition = Children[0].Position;

            int count = 0;
            Children.ForEach(uiObj =>
            {
                if (count == 0) //base component of the UIObject
                {
                    uiObj.SetPosition(position);
                }
                else 
                {
                    uiObj.SetPosition(position, basePosition);
                }
                count++;
            });
        }

        public void SetPosition(Vector3 position, Vector3 basePosition)
        {
            base.SetPosition(position);

            Children.ForEach(uiObj =>
            {
                Vector3 offsetPosition = uiObj.Position - basePosition;
                uiObj.SetPosition(position + offsetPosition);
            });
        }

        public void SetPositionConditional(Vector3 position, Func<UIObject, bool> conditionalCheck, int objectCount = -1) 
        {
            int count = 0;
            if (conditionalCheck(this)) 
            {
                SetPosition(position);
                count++;
            }
            
            for (int i = 0; i < Children.Count && objectCount != count; i++) 
            {
                if (conditionalCheck(Children[i])) 
                {
                    Children[i].SetPosition(position);
                    count++;
                }
            }
        }

        public void ForEach(Action<UIObject> objAction, UIObject uiObj = null) 
        {
            if (uiObj == null)
            {
                objAction(this);
                Children.ForEach(obj =>
                {
                    ForEach(objAction, obj);
                });
            }
            else 
            {
                objAction(uiObj);
                uiObj.Children.ForEach(obj =>
                {
                    ForEach(objAction, obj);
                });
            }
        }

        public void AddChild(UIObject uiObj) 
        {
            uiObj.Parent = this;
            Children.Add(uiObj);
        }

        public void ClearChildren() 
        {
            for (int i = Children.Count - 1; i > 0; i--) 
            {
                Children[i].Parent = null;
                Children.RemoveAt(i);
            }
        }

        public UIObject GetFirstValidParent(UIObject uiObj, BoundsCheckType type) //gets the deepest UIObject in the tree that the type applies to
        {
            UIObject currObj = uiObj;

            while (currObj.Parent != null) 
            {
                switch (type) 
                {
                    case BoundsCheckType.Grab:
                        if (currObj.Draggable)
                        {
                            return currObj;
                        }

                        if (currObj.Parent.Draggable)
                        {
                            return currObj.Parent;
                        }
                        break;
                    case BoundsCheckType.MouseUp:
                        if (currObj.Clickable)
                        {
                            return currObj;
                        }

                        if (currObj.Parent.Clickable)
                        {
                            return currObj.Parent;
                        }
                        break;
                    case BoundsCheckType.Hover:
                        if (currObj.Hoverable)
                        {
                            return currObj;
                        }

                        if (currObj.Parent.Hoverable)
                        {
                            return currObj.Parent;
                        }
                        break;
                    case BoundsCheckType.MouseDown:
                        if (currObj.Clickable)
                        {
                            return currObj;
                        }

                        if (currObj.Parent.Clickable)
                        {
                            return currObj.Parent;
                        }
                        break;
                }
                currObj = currObj.Parent;
            }

            return currObj;
        }

        public Vector3 GetDimensions() 
        {
            Vector3 dimensions = default;

            if (Children.Count > 0)
            {
                return Children[0].GetDimensions();
            }
            else if (BaseObjects.Count > 0) 
            {
                return BaseObjects[0].Dimensions;
            }

            return dimensions;
        }

        public override void OnMouseUp()
        {
            base.OnMouseUp();
            OnClick(); //Default OnMouseUp behavior is a click for UIObjects
        }
        public override void OnClick()
        {
            base.OnClick();
            OnClickAction?.Invoke();
        }
        public override void OnMouseDown()
        {
            base.OnMouseDown();
        }
        public override void OnHover()
        {
            base.OnHover();
            if (Hoverable && !Hovered)
            {
                Hovered = true;
            }
        }
        public override void HoverEnd()
        {
            base.HoverEnd();
            Hovered = false;
        }
        public void OnGrab(Vector2 MouseCoordinates, UIObject grabbedObject) 
        {
            base.OnGrab();

            if (Draggable && !Grabbed)
            {
                Grabbed = true;

                Vector3 screenCoord = WindowConstants.ConvertLocalToScreenSpaceCoordinates(MouseCoordinates);
                if (grabbedObject.Children.Count > 0)
                {
                    _grabbedDeltaPos = screenCoord - grabbedObject.Children[0].Position; //Hack. Might need to be fixed later
                }
                else
                {
                    _grabbedDeltaPos = screenCoord - grabbedObject.Position;
                }
            }
        }
        public override void GrabEnd()
        {
            base.GrabEnd();

            if (Grabbed) 
            {
                Grabbed = false;
                _grabbedDeltaPos = default;
            }
        }
    }

    public class TextBox : UIObject
    {
        public float TextScale = 1f;
        //public float TitleScale = 1.5f;
        public Vector3 TextOffset = new Vector3(20, 30, 0);
        public bool CenterText = false;

        private UIBlock _mainBlock;

        public Text TextField;

        public TextBox(Vector3 position, Vector2 size, string text, float textScale = 1, bool centerText = false, bool cameraPerspective = false)
        {
            TextScale = textScale;
            Size = size;
            Position = position;
            Name = "TextBox";
            CenterText = centerText;
            CameraPerspective = cameraPerspective;

            UIBlock block = new UIBlock(Position, new Vector2(Size.X / WindowConstants.ScreenUnits.X, Size.Y / WindowConstants.ScreenUnits.Y), default, 90, true, cameraPerspective);
            block.SetColor(new Vector4(0.2f, 0.2f, 0.2f, 1));

            Text textObj;
            if (CenterText) 
            {
                textObj = new Text(text, block.Position, cameraPerspective);
            }
            else 
            {
                textObj = new Text(text, block.Origin + TextOffset, cameraPerspective);
            }
            
            textObj.SetScale(textScale);

            if (CenterText) 
            {
                Vector2 textDimensions = textObj.GetTextDimensions();
                textObj.SetPosition(new Vector3(block.Position.X - textDimensions.X / 2, block.Position.Y, block.Position.Z));
            }

            TextObjects.Add(textObj);

            TextField = textObj;

            AddChild(block);
            _mainBlock = block;

            block.OnClickAction = () =>
            {
                //Console.WriteLine(block.Origin);
            };
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);
            TextObjects.ForEach(obj => 
            {
                if (CenterText)
                {
                    Vector2 textDimensions = obj.GetTextDimensions();
                    obj.SetPosition(new Vector3(_mainBlock.Position.X - textDimensions.X / 2, _mainBlock.Position.Y, _mainBlock.Position.Z));
                }
                else 
                {
                    obj.SetPosition(_mainBlock.Origin + TextOffset);
                }
            });
        }
        public override void SetColor(Vector4 color) 
        {
            _mainBlock.SetColor(color);
        }

        public void SetTextColor(Vector4 color) 
        {
            TextObjects.ForEach(obj => obj.SetColor(color));
        }
    }

    public class Footer : UIObject
    {
        public Footer(float height = 100)
        {
            Position = new Vector3(WindowConstants.ScreenUnits.X / 2, WindowConstants.ScreenUnits.Y - height / 4 + height / 200, 0);
            Name = "Footer";

            Clickable = true;

            UIBlock window = new UIBlock(Position, new Vector2(2, height / WindowConstants.ScreenUnits.Y), default, 90, false);
            AddChild(window);


            Button testButton = new Button(window.Origin + new Vector3(140, height / 2, 0), new Vector2(500, 150), "Move", 0.75f);
            AddChild(testButton);

            Button button2 = new Button(window.Origin + new Vector3(290, height / 2, 0), new Vector2(500, 150), "Melee", 0.75f);
            AddChild(button2);

            Button button3 = new Button(window.Origin + new Vector3(440, height / 2, 0), new Vector2(500, 150), "Range", 0.75f);
            AddChild(button3);
        }
    }

    public class SideBar : UIObject
    {
        public SideBar()
        {

        }
    }

    public class Button : UIObject
    {
        private TextBox _mainObject;
        public Vector4 BaseColor = new Vector4(0.78f, 0.60f, 0.34f, 1);

        public Button(Vector3 position, Vector2 size, string text = "", float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, bool cameraPerspective = false)
        {
            Position = position;
            Size = size;
            CameraPerspective = cameraPerspective;

            Clickable = true;
            //Draggable = true;
            Hoverable = true;

            TextBox textBox = new TextBox(position, size, text, textScale, true, CameraPerspective);
            _mainObject = textBox;

            AddChild(textBox);

            if (boxColor != default)
            {
                BaseColor = boxColor;
                textBox.SetColor(boxColor);
            }
            else 
            {
                textBox.SetColor(BaseColor);
            }
            if (textColor != default) 
            {
                textBox.SetTextColor(textColor);
            }
        }

        public Button() { }

        public override void OnHover()
        {
            if (!Hovered) 
            {
                Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

                SetColor(hoveredColor);
            }

            base.OnHover();
        }

        public override void HoverEnd()
        {
            if (Hovered) 
            {
                SetColor(BaseColor);
            }

            base.HoverEnd();
        }

        public override void OnMouseDown()
        {
            base.OnMouseDown();
            Vector4 mouseDownColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);

            SetColor(mouseDownColor);
        }
        public override void OnMouseUp()
        {
            base.OnMouseUp();
            Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

            SetColor(hoveredColor);
        }

        public override void SetColor(Vector4 color)
        {
            if(!Selected)
                _mainObject.SetColor(color);
        }
    }

    //public class Block : UIObject
    //{
    //    public UIBorders Borders = new UIBorders();
    //    private BaseObject _borderLeft;
    //    private BaseObject _borderRight;
    //    private BaseObject _borderTop;
    //    private BaseObject _borderBottom;
    //    public Block(Vector2i clientSize, Vector3 position, UIBorders borders = default)
    //    {
    //        ClientSize = clientSize;
    //        Name = "UIBlock";
    //        Position = position;

    //        Borders.Left = borders.Left;
    //        Borders.Right = borders.Right;
    //        Borders.Top = borders.Top;
    //        Borders.Bottom = borders.Bottom;

    //        CreateBorders();

    //        Animation tempAnimation;
    //        float aspectRatio = (float)ClientSize.Y / ClientSize.X;

    //        RenderableObject block = new RenderableObject(new SpritesheetObject(0, Spritesheets.UISheet, 3, 3).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
    //        block.ScaleX(aspectRatio);
    //        block.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
    //        tempAnimation = new Animation() 
    //        { 
    //            Frames = new List<RenderableObject>() { block },
    //            Frequency = 0,
    //            Repeats = 0
    //        };

    //        BaseObject blockObj = new BaseObject(ClientSize, new List<Animation>() { tempAnimation }, 0, "UIBlock", position, EnvironmentObjects.BASE_TILE.Bounds);
    //        BaseObjects.Add(blockObj);
    //    }

    //    private void CreateBorders() //there really isn't a less ugly way to do this
    //    {
    //        Animation tempAnimation;
    //        float aspectRatio = (float)ClientSize.Y / ClientSize.X;

    //        RenderableObject borderLeft = new RenderableObject(new SpritesheetObject(3, Spritesheets.UISheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
    //        borderLeft.ScaleX(aspectRatio);
    //        borderLeft.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);

    //        tempAnimation = new Animation()
    //        {
    //            Frames = new List<RenderableObject>() { borderLeft },
    //            Frequency = 0,
    //            Repeats = 0
    //        };
    //        BaseObject borderLeftObj = new BaseObject(ClientSize, new List<Animation>() { tempAnimation }, 0, "LeftBorder", Position, EnvironmentObjects.BASE_TILE.Bounds);
    //        borderLeftObj.Render = Borders.Left;
    //        BaseObjects.Add(borderLeftObj);
    //        _borderLeft = borderLeftObj;


    //        RenderableObject borderRight = new RenderableObject(new SpritesheetObject(3, Spritesheets.UISheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
    //        borderRight.ScaleX(aspectRatio);
    //        borderRight.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);

    //        tempAnimation = new Animation()
    //        {
    //            Frames = new List<RenderableObject>() { borderRight },
    //            Frequency = 0,
    //            Repeats = 0
    //        };
    //        BaseObject borderRightObj = new BaseObject(ClientSize, new List<Animation>() { tempAnimation }, 0, "RightBorder", Position + Vector3.UnitX * BLOCK_WIDTH * aspectRatio, EnvironmentObjects.BASE_TILE.Bounds);
    //        borderRightObj.Render = Borders.Right;
    //        BaseObjects.Add(borderRightObj);
    //        _borderRight = borderRightObj;


    //        RenderableObject borderTop = new RenderableObject(new SpritesheetObject(3, Spritesheets.UISheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
    //        borderTop.ScaleX(aspectRatio);
    //        borderTop.RotateZ(90);
    //        borderTop.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);

    //        tempAnimation = new Animation()
    //        {
    //            Frames = new List<RenderableObject>() { borderTop },
    //            Frequency = 0,
    //            Repeats = 0
    //        };
    //        BaseObject borderTopObj = new BaseObject(ClientSize, new List<Animation>() { tempAnimation }, 0, "TopBorder", Position + new Vector3(0, -BLOCK_HEIGHT * 0.97f, 0), EnvironmentObjects.BASE_TILE.Bounds);
    //        borderTopObj.Render = Borders.Top;
    //        BaseObjects.Add(borderTopObj);
    //        _borderTop = borderTopObj;


    //        RenderableObject borderBottom = new RenderableObject(new SpritesheetObject(3, Spritesheets.UISheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
    //        borderBottom.ScaleX(aspectRatio);
    //        borderBottom.RotateZ(90);
    //        borderBottom.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);

    //        tempAnimation = new Animation()
    //        {
    //            Frames = new List<RenderableObject>() { borderBottom },
    //            Frequency = 0,
    //            Repeats = 0
    //        };
    //        BaseObject borderBottomObj = new BaseObject(ClientSize, new List<Animation>() { tempAnimation }, 0, "BottomBorder", Position, EnvironmentObjects.BASE_TILE.Bounds);
    //        borderBottomObj.Render = Borders.Bottom;
    //        BaseObjects.Add(borderBottomObj);
    //        _borderBottom = borderBottomObj;
    //    }
    //    public override void UpdateBorders(UIBorders borders) 
    //    {
    //        _borderBottom.Render = borders.Bottom;
    //        _borderLeft.Render = borders.Left;
    //        _borderRight.Render = borders.Right;
    //        _borderTop.Render = borders.Top;
    //    }
    //}

    //public class UIWindow : UIObject 
    //{
    //    public Vector2i Size = new Vector2i(1, 1);

    //    private List<Block> Blocks = new List<Block>();

    //    public UIWindow(Vector2i clientSize, Vector3 position, Vector2i size = default) 
    //    {
    //        ClientSize = clientSize;
    //        Position = position;
    //        Size = size.X == 0 ? Size : size;

    //        FillBlocks();
    //    }

    //    public void FillBlocks() 
    //    {
    //        float aspectRatio = (float)ClientSize.Y / ClientSize.X;

    //        for (int i = 0; i < Size.X; i++)
    //        {
    //            for (int j = 0; j < Size.Y; j++)
    //            {
    //                UIBorders border = Borders.None;

    //                if (j == 0)
    //                {
    //                    border.Top = true;
    //                }
    //                if (i == 0)
    //                {
    //                    border.Left = true;
    //                }

    //                if (j == Size.Y - 1)
    //                {
    //                    border.Bottom = true;
    //                }

    //                if (i == Size.X - 1)
    //                {
    //                    border.Right = true;
    //                }

    //                Block blk = new Block(ClientSize, Position + new Vector3(BLOCK_WIDTH * aspectRatio * i, BLOCK_WIDTH * j, 0), border);

    //                NestedObjects.Add(blk);
    //                Blocks.Add(blk);
    //            }
    //        }
    //    }

    //    public override void ScaleAddition(float f)
    //    {
    //        base.ScaleAddition(f);

    //        //RecalculateBlockPosition();
    //    }

    //    public override void ScaleAll(float f)
    //    {
    //        base.ScaleAll(f);

    //        //RecalculateBlockPosition();
    //    }

    //    private void RecalculateBlockPosition()
    //    {
    //        float aspectRatio = (float)ClientSize.Y / ClientSize.X;

    //        for (int i = 0; i < Size.X; i++)
    //        {
    //            for (int j = 0; j < Size.Y; j++)
    //            {
    //                Blocks[i * Size.X + j].SetPosition(Position + new Vector3(BLOCK_WIDTH * aspectRatio * i * Scale.X, BLOCK_HEIGHT * j * Scale.Y, 0));
    //            }
    //        }
    //    }
    //}

    public class UIBlock : UIObject
    {
        public Vector2 Size = new Vector2(1, 1);
        private bool _scaleAspectRatio = true;
        protected BaseObject _window;
        protected BaseObject _backdrop;

        public Action OnClickAction;

        public UIBlock(Vector3 position, Vector2 size = default, Vector2i spritesheetDimensions = default, int spritesheetPosition = 90, bool scaleAspectRatio = true, bool cameraPerspective = false)
        {
            Position = position;
            Size = size.X == 0 ? Size : size;
            _scaleAspectRatio = scaleAspectRatio;
            Name = "UIBlock";
            CameraPerspective = cameraPerspective;


            Vector2i SpritesheetDimensions = spritesheetDimensions.X == 0 ? new Vector2i(1, 1) : spritesheetDimensions;

            Animation tempAnimation;
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(Size.X, Size.Y);

            RenderableObject window = new RenderableObject(new SpritesheetObject(spritesheetPosition, Spritesheets.UISheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            window.ScaleX(aspectRatio);
            window.ScaleX(ScaleFactor.X);
            window.ScaleY(ScaleFactor.Y);
            window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject windowObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindow", position, EnvironmentObjects.UIBlockBounds);
            windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            BaseObjects.Add(windowObj);
            _window = windowObj;

            RenderableObject backdrop = new RenderableObject(new SpritesheetObject(spritesheetPosition, Spritesheets.UISheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
            backdrop.ScaleX(aspectRatio);
            backdrop.ScaleX(ScaleFactor.X);
            backdrop.ScaleY(ScaleFactor.Y);
            backdrop.ScaleAddition(0.01f);
            backdrop.Color = new Vector4(0.1f, 0.1f, 0.1f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { backdrop },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject backdropObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindowBackdrop", position, EnvironmentObjects.BASE_TILE.Bounds) { Clickable = false };
            backdropObj.BaseFrame.CameraPerspective = CameraPerspective;

            BaseObjects.Add(backdropObj);
            _backdrop = backdropObj;

            SetOrigin(aspectRatio, ScaleFactor);
        }

        public override void SetColor(Vector4 color)
        {
            _window.BaseFrame.Color = color;
        }
        public void SetBackdropColor(Vector4 color)
        {
            _backdrop.BaseFrame.Color = color;
        }

        public override void ScaleAddition(float f)
        {
            base.ScaleAddition(f);
        }

        public override void ScaleAll(float f)
        {
            base.ScaleAll(f);
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;
            Origin = new Vector3(Position.X - _originOffset.X, Position.Y - _originOffset.Y, Position.Z - _originOffset.Z);
        }
    }

    /// <summary>
    /// Functionally similar to the UIBlock class but only contains the backdrop as opposed to the backdrop + primary window
    /// </summary>
    public class Backdrop : UIObject 
    {
        public Vector2 Size = new Vector2(1, 1);
        private Vector3 _originOffset = default;
        private bool _scaleAspectRatio = true;
        private BaseObject _backdrop;

        public Action _onClick;

        public Backdrop(Vector3 position, Vector2 size = default, Vector2i spritesheetDimensions = default, int spritesheetPosition = 90, bool scaleAspectRatio = true, bool cameraPerspective = false)
        {
            Position = position;
            Size = size.X == 0 ? Size : size;
            _scaleAspectRatio = scaleAspectRatio;
            Name = "Backdrop";
            CameraPerspective = cameraPerspective;


            Vector2i SpritesheetDimensions = spritesheetDimensions.X == 0 ? new Vector2i(1, 1) : spritesheetDimensions;

            Animation tempAnimation;
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(Size.X, Size.Y);

            RenderableObject window = new RenderableObject(new SpritesheetObject(spritesheetPosition, Spritesheets.UISheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            window.ScaleX(aspectRatio);
            window.ScaleX(ScaleFactor.X);
            window.ScaleY(ScaleFactor.Y);
            window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject backdropObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindow", position, EnvironmentObjects.UIBlockBounds);
            backdropObj.BaseFrame.CameraPerspective = CameraPerspective;

            BaseObjects.Add(backdropObj);
            _backdrop = backdropObj;

            SetOrigin(aspectRatio, ScaleFactor);
        }

        public override void SetColor(Vector4 color)
        {
            _backdrop.BaseFrame.Color = color;
        }

        public override void ScaleAddition(float f)
        {
            base.ScaleAddition(f);
        }

        public override void ScaleAll(float f)
        {
            base.ScaleAll(f);
        }

        public override void OnClick()
        {
            _onClick?.Invoke();
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;
            Origin = new Vector3(Position.X - _originOffset.X, Position.Y - _originOffset.Y, Position.Z - _originOffset.Z);
        }
    }
}
