using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal enum UISheetIcons 
    {
        Shield = 27,
        BrokenShield,

        Chevron = 47,
        Minimize,
        PartyIcon,

        Fire = 59
    }

    internal class UIObject : GameObject, IComparable<UIObject>
    {
        internal List<UIObject> Children = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        internal List<Text> TextObjects = new List<Text>();
        internal Vector3 Origin = default; //this will be the top left of the UIBlock
        internal UIScale Size = new UIScale(1, 1);
        internal bool CameraPerspective = false;

        internal new ObjectType ObjectType = ObjectType.UI;

        internal UIObject BaseComponent;
        internal BaseObject _baseObject;

        internal UIAnchorPosition Anchor = UIAnchorPosition.Center;
        internal UIDimensions _anchorOffset = new UIDimensions();

        internal float ZIndex = 0; //higher values get rendered in front

        internal bool Focusable = false;

        internal bool Disabled = false;
        internal bool Selected = false;
        internal bool Focused = false; //determines whether this object should be taking key presses

        internal UIObject Parent = null;

        protected Vector3 _originOffset = default;
        protected bool _scaleAspectRatio = true;

        internal Action OnClickAction = null;

        internal List<UITreeNode> ReverseTree = null; //must be generated for all top level UIObjects
        internal BoundingArea ScissorBounds = new BoundingArea();
        internal Bounds AdditionalBounds = null;

        internal bool RenderAfterParent = false;

        public Vector4 DefaultColor = new Vector4(1, 1, 1, 1);
        public Vector4 HoverColor;
        public Vector4 DisabledColor;
        public Vector4 SelectedColor;

        internal UIObject() { }

        internal void SetOrigin(float aspectRatio, UIScale ScaleFactor) 
        {
            Origin = new Vector3(Position.X - Position.X * aspectRatio * ScaleFactor.X / 2, Position.Y - Position.Y * ScaleFactor.Y / 2, Position.Z);
            _originOffset.X = Position.X - Origin.X;
            _originOffset.Y = Position.Y - Origin.Y;
            _originOffset.Z = Position.Z - Origin.Z;
        }

        internal override void Tick()
        {
            if (Render) 
            {
                base.Tick();

                lock(Children)
                for (int i = 0; i < Children.Count; i++) 
                {
                    Children[i].Tick();
                }
            }
        }

        internal virtual void SetSize(UIScale size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            GetBaseObject(this).BaseFrame.SetScaleAll(1);

            GetBaseObject(this).BaseFrame.ScaleX(aspectRatio);
            GetBaseObject(this).BaseFrame.ScaleX(ScaleFactor.X);
            GetBaseObject(this).BaseFrame.ScaleY(ScaleFactor.Y);

            if (BaseObjects.Count > 0) 
            {
                BaseObjects.ForEach(obj =>
                {
                    obj.BaseFrame.SetScaleAll(1);

                    obj.BaseFrame.ScaleX(aspectRatio);
                    obj.BaseFrame.ScaleX(ScaleFactor.X);
                    obj.BaseFrame.ScaleY(ScaleFactor.Y);
                });
            }

            Size = size;
            SetOrigin(aspectRatio, Size);
        }

        internal override void ScaleAll(float f)
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

        internal override void ScaleAddition(float f)
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

        internal override void AddBaseObject(BaseObject obj)
        {
            obj.EnableLighting = false;

            BaseObjects.Add(obj);
        }

        internal virtual void SetInlineColor(Vector4 color) 
        {
            GetBaseObject().OutlineParameters.InlineColor = color;
        }

        internal virtual void SetAllInline(int num) 
        {
            GetBaseObject().OutlineParameters.SetAllInline(num);
        }

        internal void BoundsCheck(Vector2 MouseCoordinates, Camera camera, Action<UIObject> optionalAction = null, UIEventType type = UIEventType.Click)
        {
            if (ReverseTree == null)
                return;

            int count = ReverseTree.Count;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (count != ReverseTree.Count)
                    {
                        Console.WriteLine("ReverseTree modified during BoundsCheck");
                        return;
                    }

                    if (IsValidForBoundsType(ReverseTree[i].UIObject, type))
                    {
                        if (type == UIEventType.HoverEnd)
                        {
                            ReverseTree[i].UIObject.OnHoverEnd();
                        }
                        else if (ReverseTree[i].InsideBounds(MouseCoordinates, camera))
                        {
                            optionalAction?.Invoke(ReverseTree[i].UIObject);

                            switch (type)
                            {
                                case UIEventType.Click:
                                    ReverseTree[i].UIObject.OnMouseUp();
                                    return;
                                case UIEventType.RightClick:
                                    ReverseTree[i].UIObject.OnRightClick();
                                    return;
                                case UIEventType.Hover:
                                    ReverseTree[i].UIObject.OnHover();

                                    type = UIEventType.HoverEnd; //This ensures only 1 object can be hovered in any given reverse tree
                                    break;
                                case UIEventType.TimedHover:
                                    optionalAction?.Invoke(ReverseTree[i].UIObject);

                                    type = UIEventType.HoverEnd; //This ensures only 1 object can be hovered in any given reverse tree
                                    break;
                                case UIEventType.MouseDown:
                                    ReverseTree[i].UIObject.OnMouseDown();
                                    return;
                                case UIEventType.Grab:
                                    ReverseTree[i].UIObject.OnGrab(MouseCoordinates, ReverseTree[i].UIObject);
                                    optionalAction?.Invoke(ReverseTree[i].UIObject);
                                    return;
                                case UIEventType.Focus:
                                    if (!ReverseTree[i].UIObject.Focused)
                                    {
                                        ReverseTree[i].UIObject.OnFocus();
                                    }
                                    optionalAction?.Invoke(ReverseTree[i].UIObject);
                                    return;
                            }
                        }
                        else if (type == UIEventType.Hover)
                        {
                            ReverseTree[i].UIObject.OnHoverEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error caught in BoundsCheck: " + e.Message);
            }
        }
        

        internal static bool IsValidForBoundsType(UIObject obj, UIEventType type) 
        {
            if (!IsRendered(obj))
                return false;

            return type switch
            {
                UIEventType.Click => obj.Clickable && !obj.Disabled,
                UIEventType.RightClick => obj.Clickable && !obj.Disabled,
                UIEventType.Hover => obj.Hoverable,
                UIEventType.TimedHover => obj.HasTimedHoverEffect,
                UIEventType.MouseDown => obj.Clickable && !obj.Disabled,
                UIEventType.Grab => obj.Draggable && !obj.Disabled,
                UIEventType.KeyDown => obj.Focused && !obj.Disabled,
                UIEventType.Focus => obj.Focusable && !obj.Disabled,
                UIEventType.HoverEnd => obj.Hoverable,
                _ => false,
            };
        }

        /// <summary>
        /// Checks all the way up the tree for the passed UIObject to ascertain whether the ui element is being displayed
        /// </summary>
        internal static bool IsRendered(UIObject obj) 
        {
            UIObject parent = obj;

            while (parent != null) 
            {
                if (!parent.Render)
                    return false;

                parent = parent.Parent;
            }

            return true;
        }


        internal override void SetPosition(Vector3 position)
        {
            Vector3 deltaPos = Position - position;
            base.SetPosition(position);

            for (int i = 0; i < Children.Count; i++) 
            {
                Children[i].SetPosition(Children[i].Position - deltaPos);
            }
        }

        

        internal virtual void SetPositionFromAnchor(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center) 
        {
            if (anchor == UIAnchorPosition.Center)
                anchor = Anchor;

            UIDimensions anchorOffset = GetAnchorOffset(anchor);

            _anchorOffset = anchorOffset;

             SetPosition(position - anchorOffset);
        }


        internal Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition) 
        {
            return GetAnchorPosition(anchorPosition, Position);
        }
        internal virtual Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
        {
            UIDimensions dimensions = GetDimensions();
            Vector3 anchorPos = new Vector3(position);

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    anchorPos.Y -= dimensions.Y / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    anchorPos.Y -= dimensions.Y / 2;
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    anchorPos.Y -= dimensions.Y / 2;
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.LeftCenter:
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.RightCenter:
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomCenter:
                    anchorPos.Y += dimensions.Y / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    anchorPos.Y += dimensions.Y / 2;
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomRight:
                    anchorPos.Y += dimensions.Y / 2;
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.Center:
                default:
                    break;
            }

            return anchorPos;
        }
        internal virtual UIDimensions GetAnchorOffset(UIAnchorPosition anchorPosition)
        {
            UIDimensions dimensions = GetDimensions();
            UIDimensions returnDim = new UIDimensions();

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    returnDim.Y -= dimensions.Y / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.LeftCenter:
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.RightCenter:
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomCenter:
                    returnDim.Y += dimensions.Y / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    returnDim.Y += dimensions.Y / 2;
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomRight:
                    returnDim.Y += dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.Center:
                default:
                    break;
            }

            return returnDim;
        }

        /// <summary>
        /// Actually a preorder search now but I'm leaving the breadth first search code commented out
        /// </summary>
        internal List<UITreeNode> BreadthFirstSearch() 
        {
            List<UITreeNode> tree = new List<UITreeNode>();
            //List<UIObject> nodesToTraverse = new List<UIObject>();

            //List<UIObject> temp = new List<UIObject>();

            //tree.Add(new UITreeNode(this, 0, GetBaseObject(this))); //root node
            //nodesToTraverse.Add(this);

            //int depth = 0;
            //for (int i = 0; i < nodesToTraverse.Count; i++) 
            //{
            //    nodesToTraverse[i].Children.ForEach(c =>
            //    {
            //        tree.Add(new UITreeNode(c, depth, GetBaseObject(c)));
            //        temp.Add(c);
            //    });

            //    if (i == nodesToTraverse.Count - 1) 
            //    {
            //        temp.Reverse();
            //        nodesToTraverse = new List<UIObject>(temp);
            //        temp.Clear();
            //        i = -1;

            //        depth++;
            //    }
            //}

            void queueChildren(UIObject parentObject) 
            {
                tree.Add(new UITreeNode(parentObject, 0, GetBaseObject(parentObject)));

                for (int i = parentObject.Children.Count - 1; i >= 0; i--) 
                {
                    queueChildren(parentObject.Children[i]);
                }
            }

            queueChildren(this);

            return tree;
        }

        internal static BaseObject GetBaseObject(UIObject obj) 
        {
            if (obj._baseObject != null)
                return obj._baseObject;

            BaseObject returnObj = null;

            UIObject nextObj = obj;

            while (returnObj == null) 
            {
                nextObj = nextObj.BaseComponent;

                if (nextObj != null && nextObj._baseObject != null)
                {
                    return nextObj._baseObject;
                }
                else if (nextObj == null)
                {
                    throw new Exception("UIObject contains no base object.");
                }
            }

            return returnObj;
        }

        internal void GenerateReverseTree() 
        {
            ReverseTree = BreadthFirstSearch();
            ReverseTree.Reverse();
        }

        internal void ForceTreeRegeneration() 
        {
            UIObject parent = this;
            while (true) 
            {
                if(parent.Parent != null)
                    parent = parent.Parent;

                if (parent.Parent == null)
                {
                    parent.GenerateReverseTree();
                    return;
                }
            }
        }

        internal override void CleanUp()
        {
            Children.ForEach(child =>
            {
                child.CleanUp();
            });

            base.CleanUp();
        }

        internal virtual void AddChild(UIObject uiObj, int zIndex = -1) 
        {
            uiObj.ReverseTree = null;
            uiObj.Parent = this;
            Children.Add(uiObj);

            if (zIndex != -1)
            {
                uiObj.ZIndex = zIndex;
            }
            else 
            {
                uiObj.ZIndex = 0;
            }

            Children.Sort();

            if (Parent == null)
            {
                GenerateReverseTree();
            }
            else 
            {
                ForceTreeRegeneration();
            }

            LoadTexture(uiObj);
        }

        internal void RemoveChild(int objectID) 
        {
            UIObject child = Children.Find(c => c.ObjectID == objectID);

            if (child != null) 
            {
                child.CleanUp();

                Children.Remove(child);

                child.Parent = null;

                if (Parent == null)
                {
                    GenerateReverseTree();
                }
                else 
                {
                    ForceTreeRegeneration();
                }
            }
        }

        internal void RemoveChild(UIObject obj) 
        {
            RemoveChild(obj.ObjectID);
        }

        internal void RemoveChildren()
        {
            //for (int i = Children.Count - 1; i >= 0; i--)
            //{
            //    Children[i].Parent = null;
            //    Children.RemoveAt(i);
            //}

            Children.Clear();

            if (Parent == null)
            {
                GenerateReverseTree();
            }
            else
            {
                ForceTreeRegeneration();
            }
        }

        internal void RemoveChildren(List<int> objectIDs) 
        {
            objectIDs.ForEach(id => RemoveChild(id));
        }

        internal void SetDisabled(bool disable) 
        {
            ForEach(obj => obj.OnDisabled(disable));
        }

        internal void ForEach(Action<UIObject> objAction, UIObject uiObj = null)
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


        internal virtual new UIDimensions GetDimensions() 
        {
            UIDimensions dimensions = default;
            if (BaseComponent != null) 
            {
                return BaseComponent.GetDimensions();
            }
            else if (_baseObject != null)
            {
                return new UIDimensions(_baseObject.Dimensions);
            }

            return dimensions;
        }

        internal BaseObject GetBaseObject() 
        {
            if (_baseObject != null) 
            {
                return _baseObject;
            }
            else if (BaseComponent != null) 
            {
                return BaseComponent.GetBaseObject();
            }

            return null;
        }

        internal override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            if (flag == SetColorFlag.Base)
                DefaultColor = color;

            base.SetColor(color);
        }

        internal override void OnMouseUp()
        {
            base.OnMouseUp();
            OnClick(); //Default OnMouseUp behavior is a click for UIObjects
        }
        internal override void OnClick()
        {
            base.OnClick();

            OnClickAction?.Invoke();
        }
        internal override void OnMouseDown()
        {
            base.OnMouseDown();
        }

        internal virtual void OnFocus() 
        {
            if (Focusable && !Focused)
            {
                Focused = true;
            }
        }

        internal virtual void FocusEnd() 
        {
            if (Focused) 
            {
                Focused = false;
            }
        }

        internal override void OnHover() 
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                HoverEvent(this);

                if(HoverColor != default) 
                {
                    EvaluateColor();
                }
            }
        }

        internal override void OnHoverEnd()
        {
            if (Hovered)
            {
                Hovered = false;

                HoverEndEvent(this);

                if (HoverColor != default)
                {
                    EvaluateColor();
                }
            }
        }

        internal virtual void OnDisabled(bool disable) 
        {
            Disabled = disable;

            if (DisabledColor != default)
            {
                EvaluateColor();
            }
        }

        internal virtual void OnGrab(Vector2 MouseCoordinates, UIObject grabbedObject) 
        {
            if (Draggable && !Grabbed)
            {
                Grabbed = true;

                Vector3 screenCoord = WindowConstants.ConvertLocalToScreenSpaceCoordinates(MouseCoordinates);
                if (grabbedObject.BaseComponent != null)
                {
                    _grabbedDeltaPos = screenCoord - grabbedObject.BaseComponent.Position;
                }
                else
                {
                    _grabbedDeltaPos = screenCoord - grabbedObject.Position;
                }
            }
        }

        internal virtual void OnSelect(bool select) 
        {
            Selected = select;

            if (SelectedColor != default)
            {
                EvaluateColor();
            }
        }

        internal virtual void OnKeyDown(KeyboardKeyEventArgs e) 
        {
            if (ReverseTree == null)
                return;

            int count = ReverseTree.Count;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (IsValidForBoundsType(ReverseTree[i].UIObject, UIEventType.KeyDown))
                    {
                        ReverseTree[i].UIObject.OnType(e);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in UIObject.OnKeyDown: " + ex.Message);
            }
        }

        internal virtual void OnKeyUp(KeyboardKeyEventArgs e) { }

        internal virtual void OnType(KeyboardKeyEventArgs e) 
        {
            switch (e.Key)
            {
                case Keys.Escape:
                    FocusEnd();
                    break;
            }
        }

        internal virtual void OnUpdate(MouseState mouseState) { }

        internal virtual void OnResize() { }

        internal virtual void OnCameraMove() { }

        internal void EvaluateColor()
        {
            Vector4 color = DefaultColor;
            SetColorFlag reason = SetColorFlag.Base;

            // these cases should be laid out from least important to most important
            // so that the later ones overwrite the former

            if (HoverColor != default && Hovered)
            {
                color = HoverColor;
                reason = SetColorFlag.Hover;
            }

            if (DisabledColor != default && Disabled)
            {
                color = DisabledColor; 
                reason = SetColorFlag.Disabled;
            }

            if (SelectedColor != default && Selected)
            {
                color = SelectedColor;
                reason = SetColorFlag.Selected;
            }

            SetColor(color, reason);
        }

        internal virtual void UpdateScissorBounds()
        {
            Vector3 botLeft = BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft);
            Vector3 topRight = BaseComponent.GetAnchorPosition(UIAnchorPosition.TopRight);

            ScissorBounds.UpdateBoundingArea(botLeft.X, topRight.X, botLeft.Y, topRight.Y);
        }


        public int CompareTo([AllowNull] UIObject other)
        {
            if (other == null)
                return 1;

            return -ZIndex.CompareTo(other.ZIndex);
        }

        internal void LoadTexture(UIObject obj)
        {
            Renderer.LoadTextureFromUIObject(obj);
        }

        internal void LoadTexture()
        {
            Renderer.LoadTextureFromUIObject(this);
        }
        protected static void ValidateObject(UIObject obj) 
        {
            if (obj.BaseComponent == null && obj._baseObject == null)
                throw new Exception("Invalid base fields for UIObject " + obj.ObjectID);
        }
    }


    internal class UITreeNode 
    {
        internal UIObject UIObject;
        internal int Depth = 0;
        internal BaseObject BoundingObject;

        internal UITreeNode(UIObject obj, int depth, BaseObject baseObject) 
        {
            UIObject = obj;
            Depth = depth;
            BoundingObject = baseObject;
        }

        internal bool InsideBounds(Vector2 point, Camera camera = null)
        {
            if (UIObject.AdditionalBounds == null)
            {
                return BoundingObject.Bounds.Contains(point, camera);
            }
            else 
            {
                return BoundingObject.Bounds.Contains(point, camera) && UIObject.AdditionalBounds.Contains(point, camera);
            }
        }
    }
}
