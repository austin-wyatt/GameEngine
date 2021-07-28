using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static MortalDungeon.Engine_Classes.UIHelpers;

namespace MortalDungeon.Engine_Classes
{
    public class UIObject : GameObject, IComparable<UIObject>
    {
        public List<UIObject> Children = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        public List<Text> TextObjects = new List<Text>();
        public Vector3 Origin = default; //this will be the top left of the UIBlock
        public UIScale Size = new UIScale(1, 1);
        public bool CameraPerspective = false;

        public new ObjectType ObjectType = ObjectType.UI;

        public UIObject BaseComponent;
        public BaseObject _baseObject;

        public UIAnchorPosition Anchor = UIAnchorPosition.Center;
        public UIDimensions _anchorOffset = new UIDimensions();

        public float ZIndex = 0; //higher values get rendered in front

        public bool Focusable = false;


        public bool Disabled = false;

        public bool Selected = false;

        public bool Focused = false; //determines whether this object should be taking key presses

        public UIObject Parent = null;

        protected Vector3 _originOffset = default;

        protected bool _scaleAspectRatio = true;

        public Action OnClickAction = null;

        public List<UITreeNode> ReverseTree = null; //must be generated for all top level UIObjects

        public UIObject() { }

        public void SetOrigin(float aspectRatio, UIScale ScaleFactor) 
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

        public virtual void SetSize(UIScale size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            GetBaseObject(this).BaseFrame.SetScaleAll(1);

            GetBaseObject(this).BaseFrame.ScaleX(aspectRatio);
            GetBaseObject(this).BaseFrame.ScaleX(ScaleFactor.X);
            GetBaseObject(this).BaseFrame.ScaleY(ScaleFactor.Y);

            Size = size;
            SetOrigin(aspectRatio, Size);
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

       

        public void BoundsCheck(Vector2 MouseCoordinates, Camera camera, Action<UIObject> optionalAction = null, UIEventType type = UIEventType.MouseUp)
        {
            if (ReverseTree == null)
                return;

            int count = ReverseTree.Count;

            for (int i = 0; i < count; i++)
            {
                if (IsValidForBoundsType(ReverseTree[i].UIObject, type))
                {
                    if (ReverseTree[i].BoundingObject.Bounds.Contains(MouseCoordinates, camera))
                    {
                        switch (type)
                        {
                            case UIEventType.MouseUp:
                                ReverseTree[i].UIObject.OnMouseUp();
                                break;
                            case UIEventType.Hover:
                                ReverseTree[i].UIObject.OnHover();
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

                        optionalAction?.Invoke(ReverseTree[i].UIObject);
                    }
                    else if (type == UIEventType.Hover)
                    {
                        ReverseTree[i].UIObject.HoverEnd();
                    }
                }
            }
        }

        public static bool IsValidForBoundsType(UIObject obj, UIEventType type) 
        {
            if (!obj.Render || obj.Disabled)
                return false;

            switch (type)
            {
                case UIEventType.MouseUp:
                    return obj.Clickable;
                case UIEventType.Hover:
                    return obj.Hoverable;
                case UIEventType.MouseDown:
                    return obj.Clickable;
                case UIEventType.Grab:
                    return obj.Draggable;
                case UIEventType.KeyDown:
                    return obj.Focused;
                case UIEventType.Focus:
                    return obj.Focusable;
                default:
                    return false;
            }
        }


        public override void SetPosition(Vector3 position)
        {
            Vector3 deltaPos = Position - position;
            base.SetPosition(position);

            for (int i = 0; i < Children.Count; i++) 
            {
                Children[i].SetPosition(Children[i].Position - deltaPos);
            }
        }

        public void SetPositionFromAnchor(Vector3 position) 
        {
            UIDimensions anchorOffset = GetAnchorOffset(Anchor);

            _anchorOffset = anchorOffset;

             SetPosition(position - anchorOffset);
        }


        public Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition) 
        {
            return GetAnchorPosition(anchorPosition, Position);
        }
        public Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
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
        public UIDimensions GetAnchorOffset(UIAnchorPosition anchorPosition)
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

        public List<UITreeNode> BreadthFirstSearch() 
        {
            List<UITreeNode> tree = new List<UITreeNode>();
            List<UIObject> nodesToTraverse = new List<UIObject>();

            List<UIObject> temp = new List<UIObject>();

            tree.Add(new UITreeNode(this, 0, GetBaseObject(this))); //root node
            nodesToTraverse.Add(this);

            int depth = 0;
            for (int i = 0; i < nodesToTraverse.Count; i++) 
            {
                nodesToTraverse[i].Children.ForEach(c =>
                {
                    tree.Add(new UITreeNode(c, depth, GetBaseObject(c)));
                    temp.Add(c);
                });

                if (i == nodesToTraverse.Count - 1) 
                {
                    temp.Reverse();
                    nodesToTraverse = new List<UIObject>(temp);
                    temp.Clear();
                    i = -1;

                    depth++;
                }
            }

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

        public void GenerateReverseTree() 
        {
            ReverseTree = BreadthFirstSearch();
            ReverseTree.Reverse();
        }

        public void ForceTreeRegeneration() 
        {
            UIObject parent = this;
            while (true) 
            {
                parent = parent.Parent;

                if (parent.Parent == null)
                {
                    parent.GenerateReverseTree();
                    return;
                }
            }
        }

        public void AddChild(UIObject uiObj, int zIndex = -1) 
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
        }

        public void ClearChildren() 
        {
            for (int i = Children.Count - 1; i > 0; i--) 
            {
                Children[i].Parent = null;
                Children.RemoveAt(i);
            }
        }

        public void RemoveChild(int objectID) 
        {
            UIObject child = Children.Find(c => c.ObjectID == objectID);

            if (child != null) 
            {
                Children.Remove(child);
                if (Parent == null)
                {
                    GenerateReverseTree();
                }
            }
        }

        public void RemoveChildren(List<int> objectIDs) 
        {
            objectIDs.ForEach(id => RemoveChild(id));
        }


        public new UIDimensions GetDimensions() 
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

        public virtual void OnFocus() 
        {
            if (Focusable && !Focused)
            {
                Focused = true;
            }
        }

        public virtual void EndFocus() 
        {
            if (Focused) 
            {
                Focused = false;
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

        public virtual void OnKeyDown(KeyboardKeyEventArgs e) 
        {
            if (ReverseTree == null)
                return;

            int count = ReverseTree.Count;

            for (int i = 0; i < count; i++)
            {
                if (IsValidForBoundsType(ReverseTree[i].UIObject, UIEventType.KeyDown))
                {
                    ReverseTree[i].UIObject.OnType(e);
                }
            }
        }

        public virtual void OnKeyUp(KeyboardKeyEventArgs e) { }

        public virtual void OnType(KeyboardKeyEventArgs e) 
        {
            //Console.WriteLine(Name + "   " + TextHelper.KeyStrokeToString(e));
        }

        public int CompareTo([AllowNull] UIObject other)
        {
            if (other == null)
                return 1;

            return -ZIndex.CompareTo(other.ZIndex);
        }


        protected static void ValidateObject(UIObject obj) 
        {
            if (obj.BaseComponent == null && obj._baseObject == null)
                throw new Exception("Invalid base fields for UIObject " + obj.ObjectID);
        }
    }


    public class UITreeNode 
    {
        public UIObject UIObject;
        public int Depth = 0;
        public BaseObject BoundingObject;

        public UITreeNode(UIObject obj, int depth, BaseObject baseObject) 
        {
            UIObject = obj;
            Depth = depth;
            BoundingObject = baseObject;
        }
    }

    public class ScissorData
    {
        public int X = 0;
        public int Y = 0;
        public int Width = 0;
        public int Height = 0;
        public int Depth = 0;

        public bool Scissor = false;

        public bool _scissorFlag = false;
        public int _startingDepth = 0;
    }
}
