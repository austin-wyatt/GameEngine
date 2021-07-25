using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
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

        public new ObjectType ObjectType = ObjectType.UI;

        //public new bool Grabbed = false;
        //public new bool Draggable = false;
        //public new bool Hoverable = false;
        //public new bool Hovered = false;

        public bool Disabled = false;

        public bool Selected = false; //when selected, SetColor behaviour changes

        public UIObject Parent = null;

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
                            else if (type == BoundsCheckType.Hover)
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
        public void OnGrab(Vector2 MouseCoordinates, UIObject grabbedObject) 
        {
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
    }
    
}
