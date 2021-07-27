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

        public enum EventType 
        {
            None,
            MouseUp,
            Hover,
            MouseDown,
            Grab,
            KeyDown
        }
    }

    public class UIObject : GameObject, IComparable<UIObject>
    {
        public List<UIObject> Children = new List<UIObject>(); //nested objects will be placed based off of their positional offset from the parent
        public List<Text> TextObjects = new List<Text>();
        public Vector3 Origin = default; //this will be the top left of the UIBlock
        public Vector2 Size = new Vector2(1, 1);
        public bool CameraPerspective = false;

        public new ObjectType ObjectType = ObjectType.UI;

        public UIObject BaseComponent;
        public BaseObject _baseObject;

        public float ZIndex = 0; //higher values get rendered in front


        public bool Disabled = false;

        public bool Selected = false;

        public bool Focused = false; //determines whether this object should be taking key presses

        public UIObject Parent = null;

        protected Vector3 _originOffset = default;

        protected bool _scaleAspectRatio = true;

        public Action OnClickAction = null;

        public List<UITreeNode> ReverseTree = null; //must be generated for all top level UIObjects

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
                display = GetDisplay(BaseComponent); //this assumes that a UIObject will always have either a nested object or a BaseObject
            }

            return display;
        }

        public virtual void SetSize(Vector2 size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            _baseObject.BaseFrame.SetScaleAll(1);

            _baseObject.BaseFrame.ScaleX(aspectRatio);
            _baseObject.BaseFrame.ScaleX(ScaleFactor.X);
            _baseObject.BaseFrame.ScaleY(ScaleFactor.Y);

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

       

        public void BoundsCheck(Vector2 MouseCoordinates, Camera camera, Action<UIObject> optionalAction = null, EventType type = EventType.MouseUp)
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
                            case EventType.MouseUp:
                                ReverseTree[i].UIObject.OnMouseUp();
                                break;
                            case EventType.Hover:
                                ReverseTree[i].UIObject.OnHover();
                                break;
                            case EventType.MouseDown:
                                ReverseTree[i].UIObject.OnMouseDown();
                                break;
                            case EventType.Grab:
                                ReverseTree[i].UIObject.OnGrab(MouseCoordinates, ReverseTree[i].UIObject);
                                optionalAction?.Invoke(ReverseTree[i].UIObject);
                                return;
                        }

                        optionalAction?.Invoke(ReverseTree[i].UIObject);
                    }
                    else if (type == EventType.Hover)
                    {
                        ReverseTree[i].UIObject.HoverEnd();
                    }
                }
            }
        }

        public static bool IsValidForBoundsType(UIObject obj, EventType type) 
        {
            if (!obj.Render || obj.Disabled)
                return false;

            switch (type)
            {
                case EventType.MouseUp:
                    return obj.Clickable;
                case EventType.Hover:
                    return obj.Hoverable;
                case EventType.MouseDown:
                    return obj.Clickable;
                case EventType.Grab:
                    return obj.Draggable;
                case EventType.KeyDown:
                    return obj.Focused;
                default:
                    return false;
            }
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            Vector3 basePosition = default;

            if(BaseComponent != null)
                basePosition = BaseComponent.Position;

            Children.ForEach(uiObj =>
            {
                if (uiObj.ObjectID == BaseComponent.ObjectID) //base component of the UIObject
                {
                    uiObj.SetPosition(position);
                }
                else 
                {
                    uiObj.SetPosition(position, basePosition);
                }
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


        public Vector3 GetDimensions() 
        {
            Vector3 dimensions = default;
            if (BaseComponent != null) 
            {
                return BaseComponent.GetDimensions();
            }
            else if (_baseObject != null)
            {
                return _baseObject.Dimensions;
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

        public virtual void OnKeyUp(KeyboardKeyEventArgs e) 
        {
            if (ReverseTree == null)
                return;

            int count = ReverseTree.Count;

            for (int i = 0; i < count; i++)
            {
                if (IsValidForBoundsType(ReverseTree[i].UIObject, EventType.KeyDown))
                {
                    OnType(e);
                }
            }
        }

        public virtual void OnType(KeyboardKeyEventArgs e) 
        {
            Console.WriteLine(Name + "   " + e.Key);
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
}
