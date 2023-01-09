using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Engine_Classes
{
    public enum UISheetIcons 
    {
        Shield = 27,
        BrokenShield,

        Chevron = 47,
        Minimize,
        PartyIcon,

        RefreshIcon = 57,
        Fire = 59
    }

    public enum ColorOverride
    {
        None,
        Hover,
        Select,
        Disabled
    }

    public class UIObject : GameObject, IComparable<UIObject>
    {
        public List<UIObject> Children = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        public List<_Text> TextObjects = new List<_Text>();
        public Vector3 Origin = default; //this will be the top left of the UIBlock
        public UIScale Size = new UIScale(1, 1);
        public bool CameraPerspective = false;

        public new ObjectType ObjectType = ObjectType.UI;

        public new BaseObject BaseObject => BaseObjects.Count > 0 ? BaseObjects[0] : BaseComponent != null ? BaseComponent.BaseObject : null;

        public UIObject BaseComponent;
        public BaseObject _baseObject;

        public UIAnchorPosition Anchor = UIAnchorPosition.Center;
        public UIDimensions _anchorOffset = new UIDimensions();

        public float ZIndex = 0; //higher values get rendered in front

        #region relevant event flags
        private bool _focusable = false;
        public bool Focusable 
        {
            get => _focusable;
            set
            {
                _focusable = value;
                if(ManagerHandle != null)
                {
                    if (_focusable)
                    {
                        ManagerHandle.AddFocusableObject(this);
                    }
                    else
                    {
                        ManagerHandle.RemoveFocusableObject(this);
                    }
                }
            }
        }

        private bool _scrollable = false;
        public bool Scrollable
        {
            get => _scrollable;
            set
            {
                _scrollable = value;
                if (ManagerHandle != null)
                {
                    if (_scrollable)
                    {
                        ManagerHandle.AddScrollableObject(this);
                    }
                    else
                    {
                        ManagerHandle.RemoveScrollableObject(this);
                    }
                }
            }
        }

        /// <summary>
        /// Whether to check for key up/down events for this object
        /// </summary>
        private bool _typeable = false;
        public bool Typeable
        {
            get => _typeable;
            set
            {
                _typeable = value;
                if (ManagerHandle != null)
                {
                    if (_typeable)
                    {
                        ManagerHandle.AddKeyDownObject(this);
                        ManagerHandle.AddKeyUpObject(this);
                    }
                    else
                    {
                        ManagerHandle.RemoveKeyDownObject(this);
                        ManagerHandle.RemoveKeyUpObject(this);
                    }
                }
            }
        }


        private bool _clickable = false;
        public override bool Clickable
        {
            get => _clickable;
            set
            {
                _clickable = value;
                if (ManagerHandle != null)
                {
                    if (_clickable)
                    {
                        ManagerHandle.AddClickableObject(this);
                    }
                    else
                    {
                        ManagerHandle.RemoveClickableObject(this);
                    }
                }
            }
        }

        private bool _hoverable = false;
        public override bool Hoverable
        {
            get => _hoverable;
            set
            {
                _hoverable = value;
                if (ManagerHandle != null)
                {
                    if (_hoverable)
                    {
                        ManagerHandle.AddHoverableObject(this);
                    }
                    else
                    {
                        ManagerHandle.RemoveHoverableObject(this);
                    }
                }
            }
        }
        #endregion


        public bool Disabled = false;
        public bool Selected = false;
        public bool Focused = false; //determines whether this object should be taking key presses

        public UIObject Parent = null;
        public UIObject RootHandle = null;

        protected Vector3 _originOffset = default;
        public bool _scaleAspectRatio = true;

        private object _reverseTreeLock = new object();
        public List<UIObject> ReverseTree = null; //must be generated for all top level UIObjects

        public BoundingArea ScissorBounds = new BoundingArea();
        public Bounds AdditionalBounds = null;

        public bool RenderAfterParent = false;

        public Vector4 DefaultColor = new Vector4(1, 1, 1, 1);
        public Vector4 HoverColor;
        public Vector4 DisabledColor;
        public Vector4 SelectedColor;

        public int _depth = 0;
        public int _childIndex = 1;
        public float ZPos = 0;

        public UIManager ManagerHandle = null;

        /// <summary>
        /// If this object is focusable, the scene's focused object will be made the FocusHandle instead
        /// </summary>
        public UIObject FocusHandle = null;

        public UIObject() { }

        public void SetOrigin(float aspectRatio, UIScale ScaleFactor) 
        {
            Origin = new Vector3(Position.X - Position.X * aspectRatio * ScaleFactor.X / 2, Position.Y - Position.Y * ScaleFactor.Y / 2, Position.Z);
            _originOffset.X = Position.X - Origin.X;
            _originOffset.Y = Position.Y - Origin.Y;
            _originOffset.Z = Position.Z - Origin.Z;
        }

        public override void Tick()
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

        public virtual void SetSize(UIScale size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            GetBaseObject(this).BaseFrame.SetScaleAll(1);

            if(aspectRatio != 1)
                GetBaseObject(this).BaseFrame.ScaleX(aspectRatio);

            GetBaseObject(this).BaseFrame.ScaleX(ScaleFactor.X);
            GetBaseObject(this).BaseFrame.ScaleY(ScaleFactor.Y);

            if (BaseObjects.Count > 0) 
            {
                BaseObjects.ForEach(obj =>
                {
                    obj.BaseFrame.SetScaleAll(1);

                    if(aspectRatio != 1)
                        obj.BaseFrame.ScaleX(aspectRatio);

                    obj.BaseFrame.ScaleX(ScaleFactor.X);
                    obj.BaseFrame.ScaleY(ScaleFactor.Y);
                });
            }

            Size = size;
            SetOrigin(aspectRatio, Size);

            Update();
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

            Update();
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

            Update();
        }

        public override void AddBaseObject(BaseObject obj)
        {
            obj.EnableLighting = false;

            BaseObjects.Add(obj);

            Update();
        }

        public virtual void SetInlineColor(Vector4 color) 
        {
            GetBaseObject().OutlineParameters.InlineColor = color;
        }

        public virtual void SetAllInline(int num) 
        {
            GetBaseObject().OutlineParameters.SetAllInline(num);
        }


        public void BoundsCheck(Vector2 MouseCoordinates, Camera camera, Action<UIObject> optionalAction = null, UIEventType type = UIEventType.Click)
        {
            if (IsValidForBoundsType(this, type))
            {
                if (ManagerHandle.ExclusiveFocusCheckObject(this) && InsideBounds(MouseCoordinates, camera))
                {
                    optionalAction?.Invoke(this);

                    switch (type)
                    {
                        case UIEventType.Click:
                            Task.Run(OnMouseUp);
                            return;
                        case UIEventType.RightClick:
                            Task.Run(OnRightClick);
                            return;
                        case UIEventType.MouseDown:
                            Task.Run(OnMouseDown);
                            return;
                        case UIEventType.Grab:
                            Task.Run(() =>
                            {
                                OnGrab(MouseCoordinates, this);
                                optionalAction?.Invoke(this);
                            });

                            return;
                        case UIEventType.Focus:
                            Task.Run(() =>
                            {
                                if (!Focused)
                                {
                                    if (FocusHandle == null)
                                        OnFocus();
                                    else
                                        FocusHandle.OnFocus();
                                }

                                optionalAction?.Invoke(FocusHandle == null ? this : FocusHandle);
                            });
                            return;
                        //case UIEventType.Scroll:
                        //    Task.Run(() =>
                        //    {
                        //        optionalAction?.Invoke(this);
                        //    });
                        //    return;
                    }
                }
            }
        }

        /// <summary>
        /// Handles bounds checking for hovers since repeatedly passing an action as a parameter was causing
        /// ballooning memory issues.
        /// </summary>
        public bool HoverBoundsCheck(Vector2 MouseCoordinates, Camera camera, UIEventType type = UIEventType.Click)
        {
            if (IsValidForBoundsType(this, type))
            {
                if (type == UIEventType.HoverEnd)
                {
                    OnHoverEnd();
                }
                else if (ManagerHandle.ExclusiveFocusCheckObject(this) && InsideBounds(MouseCoordinates, camera))
                {
                    switch (type)
                    {
                        case UIEventType.Hover:
                            OnHover();
                            return true;
                    }
                }
                else if (type == UIEventType.Hover)
                {
                    OnHoverEnd();
                }
            }

            return false;
        }

        private bool InsideBounds(Vector2 point, Camera camera = null)
        {
            return GetBaseObject().Bounds.Contains(point.X, point.Y, camera);
        }


        public static bool IsValidForBoundsType(UIObject obj, UIEventType type) 
        {
            if (!IsRendered(obj))
                return false;

            return type switch
            {
                UIEventType.Click => obj.Clickable,
                UIEventType.RightClick => obj.Clickable,
                UIEventType.Hover => obj.Hoverable,
                UIEventType.TimedHover => obj.HasTimedHoverEffect,
                UIEventType.MouseDown => obj.Clickable,
                UIEventType.Grab => obj.Draggable && !obj.Disabled,
                UIEventType.KeyDown => obj.Focused && !obj.Disabled,
                UIEventType.Focus => obj.Focusable && !obj.Disabled,
                UIEventType.HoverEnd => obj.Hoverable,
                UIEventType.Scroll => obj.Scrollable,
                _ => false,
            };
        }

        /// <summary>
        /// Checks all the way up the tree for the passed UIObject to ascertain whether the ui element is being displayed
        /// </summary>
        public static bool IsRendered(UIObject obj) 
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


        public override void SetPosition(Vector3 position)
        {
            if (!CameraPerspective)
            {
                position.Z = Position.Z;
            }

            Vector3 deltaPos = Position - position;
            //base.SetPosition(new Vector3(position.X, position.Y, ZPos));
            base.SetPosition(position);

            deltaPos.Z = 0;

            for (int i = 0; i < Children.Count; i++) 
            {
                Vector3 childPos = Children[i].Position;

                Children[i].SetPosition(childPos - deltaPos);
            }

            Update();
        }

        public void SetZPosition(float zPos)
        {
            ZPos = zPos;

            base.SetPosition(new Vector3(Position.X, Position.Y, ZPos));

            if (ManagerHandle != null)
            {
                if (Clickable)
                {
                    ManagerHandle.AddClickableObject(this);
                }

                if (Hoverable)
                {
                    ManagerHandle.AddHoverableObject(this);
                }

                if (Focusable)
                {
                    ManagerHandle.AddFocusableObject(this);
                }

                if (Typeable)
                {
                    ManagerHandle.AddKeyDownObject(this);
                    ManagerHandle.AddKeyUpObject(this);
                }

                if (Scrollable)
                {
                    ManagerHandle.AddScrollableObject(this);
                }
            }

            foreach (var text in TextObjects)
            {
                Vector3 pos = text.Position;
                pos.Z = zPos;
                text.SetPosition(pos);
            }
        }


        public virtual void SetPositionFromAnchor(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center) 
        {
            if (anchor == UIAnchorPosition.Center)
                anchor = Anchor;

            UIDimensions anchorOffset = GetAnchorOffset(anchor);

            _anchorOffset = anchorOffset;



             SetPosition(position - anchorOffset);
        }

        /// <summary>
        /// Shorthand for SetPositionFromAnchor
        /// </summary>
        public void SAP(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center)
        {
            SetPositionFromAnchor(position, anchor);
        }

        /// <summary>
        /// Shorthand for GetAnchorPosition
        /// </summary>
        public Vector3 GAP(UIAnchorPosition anchorPosition)
        {
            return GetAnchorPosition(anchorPosition);
        }

        /// <summary>
        /// Shorthand for GetAnchorPosition
        /// </summary>
        public Vector3 GAP(UIAnchorPosition anchorPosition, Vector3 position)
        {
            return GetAnchorPosition(anchorPosition, position);
        }

        public Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition) 
        {
            return GetAnchorPosition(anchorPosition, Position);
        }
        public virtual Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
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
        public virtual UIDimensions GetAnchorOffset(UIAnchorPosition anchorPosition)
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

        public void SetCameraPerspective(bool camPerspective)
        {
            foreach(var child in Children)
            {
                child.SetCameraPerspective(camPerspective);
            }

            if(_baseObject != null)
            {
                _baseObject.BaseFrame.CameraPerspective = camPerspective;
            }
        }

        /// <summary>
        /// Actually a preorder search now but I'm leaving the breadth first search code commented out
        /// </summary>
        public List<UIObject> BreadthFirstSearch(UIManager handle) 
        {
            List<UIObject> tree = new List<UIObject>();
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

            UIObject root = this;


            void queueChildren(UIObject parentObject, int depth, int childIndex) 
            {
                tree.Add(parentObject);

                parentObject.ManagerHandle = handle;

                parentObject._depth = depth;
                parentObject._childIndex = childIndex;

                parentObject.RootHandle = root;

                //float zPos = parentObject.ZPos;

                //if (parentObject._depth > 1)
                //{
                //    zPos = parentObject.Parent.Position.Z - 1 / (float)Math.Pow(10, parentObject._depth) * parentObject._childIndex;
                //}

                //parentObject.ZPos = zPos;

                //parentObject.SetZPosition(zPos);

                bool exclusiveFocus = ManagerHandle.ExclusiveFocusSet.Contains(parentObject);

                for (int i = parentObject.Children.Count - 1; i >= 0; i--) 
                {
                    queueChildren(parentObject.Children[i], depth + 1, i + 1);

                    if (exclusiveFocus)
                    {
                        lock(ManagerHandle._exclusiveFocusLock)
                        {
                            ManagerHandle.ExclusiveFocusSet.Add(parentObject.Children[i]);
                        }
                    }
                }
            }

            queueChildren(this, 1, 1);

            return tree;
        }

        public static BaseObject GetBaseObject(UIObject obj) 
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

        public void GenerateReverseTree(UIManager handle) 
        {
            ManagerHandle = handle;

            lock (_reverseTreeLock)
            {
                ReverseTree = BreadthFirstSearch(handle);
                //ReverseTree.Reverse();
            }
        }

        public void ForceTreeRegeneration() 
        {
            UIObject parent = this;
            while (true) 
            {
                if(parent.Parent != null)
                    parent = parent.Parent;

                if (parent.Parent == null && ManagerHandle != null)
                {
                    ManagerHandle.GenerateReverseTree(parent);
                    return;
                }
                else if(parent.Parent == null)
                {
                    return;
                }
            }
        }

        public void GenerateZPositions(float baseZVal)
        {
            if (ReverseTree == null)
                return;

            float currVal = baseZVal;

            lock (_reverseTreeLock)
            {
                for(int i = 0; i < ReverseTree.Count; i++)
                {
                    ReverseTree[i].SetZPosition(currVal);

                    currVal -= 0.00000015f;
                }

                //for(int i = ReverseTree.Count - 1; i >= 0; i--)
                //{
                //    ReverseTree[i].SetZPosition(currVal);

                //    //currVal += 0.000000001f;
                //    //currVal += 0.00000015f;
                //    currVal -= 0.00000015f;
                //    //currVal += 0.00001f;
                //}
            }
        }

        public override void CleanUp()
        {
            RootHandle = null;

            Children.ForEach(child =>
            {
                child.CleanUp();
            });

            base.CleanUp();

            if (ManagerHandle != null)
            {
                ManagerHandle.RemoveClickableObject(this);
                ManagerHandle.RemoveHoverableObject(this);
                ManagerHandle.RemoveFocusableObject(this);
                ManagerHandle.RemoveKeyDownObject(this);
                ManagerHandle.RemoveKeyUpObject(this);
                ManagerHandle.RemoveScrollableObject(this);
            }
        }

        public virtual void AddChild(UIObject uiObj, int zIndex = -1) 
        {
            object lockObj = new object();

            if(ManagerHandle != null)
            {
                lockObj = ManagerHandle._UILock;
            }

            lock (lockObj)
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
                    uiObj.ZIndex = 1;
                }

                Children.Sort();


                //if (Parent == null)
                //{
                //    GenerateReverseTree(ManagerHandle);
                //}
                //else 
                //{
                //    ForceTreeRegeneration();
                //}

                ForceTreeRegeneration();

                LoadTexture(uiObj);
            }
        }

        public void RemoveChild(UIObject obj) 
        {
            object lockObj = new object();

            if (ManagerHandle != null)
            {
                lockObj = ManagerHandle._UILock;
            }

            lock (lockObj)
            {
                if (obj != null)
                {
                    obj.CleanUp();

                    Children.Remove(obj);

                    obj.Parent = null;

                    //if (Parent == null)
                    //{
                    //    GenerateReverseTree(ManagerHandle);
                    //}
                    //else 
                    //{
                    //    ForceTreeRegeneration();
                    //}

                    ForceTreeRegeneration();
                }
            }

            ForceTreeRegeneration();
        }

        public void RemoveChildren(IEnumerable<UIObject> children)
        {
            object lockObj = new object();

            if (ManagerHandle != null)
            {
                lockObj = ManagerHandle._UILock;
            }

            lock (lockObj)
            {
                foreach (UIObject child in children)
                {
                    child.CleanUp();
                    child.Parent = null;
                    Children.Remove(child);
                }

                ForceTreeRegeneration();
            }
        }

        public void RemoveChildren()
        {
            object lockObj = new object();

            if (ManagerHandle != null)
            {
                lockObj = ManagerHandle._UILock;
            }

            lock (lockObj)
            {
                //for (int i = Children.Count - 1; i >= 0; i--)
                //{
                //    Children[i].Parent = null;
                //    Children.RemoveAt(i);
                //}

                foreach(var child in Children)
                {
                    child.CleanUp();
                    child.Parent = null;
                }

                Children.Clear();

                //if (Parent == null)
                //{
                //    GenerateReverseTree(ManagerHandle);
                //}
                //else
                //{
                //    ForceTreeRegeneration();
                //}

                ForceTreeRegeneration();
            }
        }

        private void UpdateInstancedRenderData()
        {
            //Window.RenderEnd -= UpdateInstancedRenderData;
            //ManagerHandle.UpdateTopLevelObject(this);
        }

        public void Update()
        {
            //if (RootHandle == null)
            //    return;

            //if (RootHandle == this)
            //{
            //    if (ManagerHandle != null)
            //    {
            //        Window.RenderEnd -= UpdateInstancedRenderData;
            //        Window.RenderEnd += UpdateInstancedRenderData;
            //    }
            //}
            //else
            //{
            //    RootHandle.Update();
            //}
        }

        public void SetDisabled(bool disable) 
        {
            ForEach(obj => obj.OnDisabled(disable));
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


        public virtual new UIDimensions GetDimensions() 
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

        public BaseObject GetBaseObject() 
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

        public override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            if (flag == SetColorFlag.Base)
                DefaultColor = color;

            base.SetColor(color);

            Update();
        }

        public override void SetRender(bool render)
        {
            base.SetRender(render);

            Update();
        }

        public override void SetTextureLoaded(bool textureLoaded)
        {
            base.SetTextureLoaded(textureLoaded);

            Update();
        }


        public event EventHandler Focus;
        public event EventHandler FocusEnd;

        public delegate void KeyboardEventHandler(object? sender, KeyboardKeyEventArgs e);

        public event KeyboardEventHandler KeyUp;
        public event KeyboardEventHandler KeyDown;

        public delegate void MouseEventHandler(object? sender, MouseState mouseState);

        public event MouseEventHandler Scroll;

        public override void OnMouseUp()
        {
            base.OnMouseUp();
            OnClick(); //Default OnMouseUp behavior is a click for UIObjects
        }

        public override void OnMouseDown()
        {
            base.OnMouseDown();
        }

        public virtual void OnFocus() 
        {
            if (Focusable && !Focused)
            {
                Focused = true;

                Focus?.Invoke(this, null);
            }
        }

        public virtual void OnFocusEnd() 
        {
            if (Focused) 
            {
                Focused = false;

                FocusEnd?.Invoke(this, null);
            }
        }

        public override void OnHover() 
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

        public override void OnHoverEnd()
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

        public virtual void OnDisabled(bool disable) 
        {
            Disabled = disable;

            if (DisabledColor != default)
            {
                EvaluateColor();
            }
        }

        public virtual void OnGrab(Vector2 MouseCoordinates, UIObject grabbedObject) 
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

        public virtual void OnSelect(bool select) 
        {
            Selected = select;

            if (SelectedColor != default)
            {
                EvaluateColor();
            }
        }

        public virtual void OnKeyDown(KeyboardKeyEventArgs e) 
        {
            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(KeyboardKeyEventArgs e) 
        {
            KeyUp?.Invoke(this, e);
        }

        public virtual void OnType(KeyboardKeyEventArgs e) 
        {
            switch (e.Key)
            {
                case Keys.Escape:
                    OnFocusEnd();
                    break;
            }
        }

        public void OnScroll(MouseState mouseState)
        {
            Scroll?.Invoke(this, mouseState);
        }

        public virtual void OnUpdate(MouseState mouseState) 
        {

        }

        public virtual void OnResize() 
        {
            SetSize(Size);
        }

        public virtual void OnCameraMove() { }


        public ColorOverride _colorOverride = ColorOverride.None; 
        public void EvaluateColor()
        {
            Vector4 color = DefaultColor;
            SetColorFlag reason = SetColorFlag.Base;

            // these cases should be laid out from least important to most important
            // so that the later ones overwrite the former

            if (HoverColor != default && (Hovered || _colorOverride == ColorOverride.Hover))
            {
                color = HoverColor;
                reason = SetColorFlag.Hover;
            }

            if (DisabledColor != default && (Disabled || _colorOverride == ColorOverride.Disabled))
            {
                color = DisabledColor; 
                reason = SetColorFlag.Disabled;
            }

            if (SelectedColor != default && (Selected || _colorOverride == ColorOverride.Disabled))
            {
                color = SelectedColor;
                reason = SetColorFlag.Selected;
            }

            SetColor(color, reason);
        }

        public void SetColorOverride(ColorOverride colOverride)
        {
            _colorOverride = colOverride;
            EvaluateColor();
        }

        public virtual void UpdateScissorBounds()
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

        public void LoadTexture(UIObject obj)
        {
            void loadTexture()
            {
                Renderer.LoadTextureFromUIObject(obj);
            }

            Window.QueueToRenderCycle(loadTexture);
        }

        public void LoadTexture()
        {
            void loadTexture()
            {
                Renderer.LoadTextureFromUIObject(this);
            }

            Window.QueueToRenderCycle(loadTexture);
        }
        protected static void ValidateObject(UIObject obj) 
        {
            if (obj.BaseComponent == null && obj._baseObject == null)
                throw new Exception("Invalid base fields for UIObject " + obj.ObjectID);
            else
                obj.LoadTexture();
        }

        public override bool Equals(object obj)
        {
            return obj is UIObject @object &&
                   base.Equals(obj) &&
                   _objectID == @object._objectID;
        }
    }


    //public class UITreeNode 
    //{
    //    public UIObject UIObject;
    //    public int Depth = 0;
    //    public BaseObject BoundingObject;

    //    public UITreeNode(UIObject obj, int depth, BaseObject baseObject) 
    //    {
    //        UIObject = obj;
    //        Depth = depth;
    //        BoundingObject = baseObject;
    //    }

    //    public bool InsideBounds(Vector2 point, Camera camera = null)
    //    {
    //        if (UIObject.AdditionalBounds == null)
    //        {
    //            return BoundingObject.Bounds.Contains(point, camera);
    //        }
    //        else 
    //        {
    //            return BoundingObject.Bounds.Contains(point, camera) && UIObject.AdditionalBounds.Contains(point, camera);
    //        }
    //    }
    //}
}
