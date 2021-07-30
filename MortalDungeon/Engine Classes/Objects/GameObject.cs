using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace MortalDungeon.Engine_Classes
{
    public class GameObject
    {
        public string Name = "";
        public Vector3 Position = new Vector3();
        public List<BaseObject> BaseObjects = new List<BaseObject>();
        public List<ParticleGenerator> ParticleGenerators = new List<ParticleGenerator>();
        public Vector3 PositionalOffset = new Vector3();
        public Vector3 Scale = new Vector3(1, 1, 1);

        public List<PropertyAnimation> PropertyAnimations = new List<PropertyAnimation>();

        public bool Cull = false; //whether the object was determined to be outside of the camera's view and should be culled
        public bool Render = true;
        public bool Clickable = false; //Note: The BaseObject's Clickable property and this property must be true for UI objects
        public bool Hoverable = false;
        public bool Draggable = false;

        public bool Hovered = false;
        public bool Grabbed = false;

        public int ObjectID => _objectID;
        protected int _objectID = currentObjectID++;
        protected static int currentObjectID = 0;


        public MultiTextureData MultiTextureData = new MultiTextureData();

        public ScissorData ScissorData = new ScissorData();


        public ObjectType ObjectType = ObjectType.GenericObject;

        public Vector3 _grabbedDeltaPos = default;

        //public Stats Stats; //contains game parameters for the object
        public GameObject() { }

        public virtual void SetPosition(Vector3 position) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.SetPosition(position + PositionalOffset);
            });

            ParticleGenerators.ForEach(particleGen =>
            {
                particleGen.SetPosition(position + PositionalOffset);
            });

            Position = position;
        }

        public virtual void SetDragPosition(Vector3 position)
        {
            SetPosition(position);
        }

        public virtual void Tick() 
        {
            BaseObjects.ForEach(obj =>
            {
                obj._currentAnimation.Tick();
            });

            PropertyAnimations.ForEach(anim =>
            {
                anim.Tick();
            });

            if (_properyAnimationsToDestroy.Count > 0) 
            {
                DestroyQueuedPropertyAnimations();
            }

            ParticleGenerators.ForEach(gen =>
            {
                gen.Tick();
            });
        }

        public virtual void ScaleAll(float f) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAll(f);
            });

            Scale *= f;
        }

        public virtual void ScaleAddition(float f)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAddition(f);
            });

            Scale.X += f;
            Scale.Y += f;
            Scale.Z += f;
        }

        public virtual void SetColor(Vector4 color) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.Color = color;
            });
        }

        public virtual RenderableObject GetDisplay()
        {
            RenderableObject display;

            if (BaseObjects.Count > 0)
            {
                display = BaseObjects[0].BaseFrame;
            }
            else if (ParticleGenerators.Count > 0)
            {
                display = ParticleGenerators[0].ParticleDisplay;
            }
            else 
            {
                throw new Exception("Attemped to retrieve the display of an empty GameObject.");
            }

            return display;
        }

        public Vector3 GetDimensions()
        {
            Vector3 dimensions = default;
            if (BaseObjects.Count > 0) 
            {
                dimensions = BaseObjects[0].Dimensions;
            }

            return dimensions;
        }

        public PropertyAnimation GetPropertyAnimationByID(int id) 
        {
            return PropertyAnimations.Find(anim => anim.AnimationID == id);
        }
        private List<int> _properyAnimationsToDestroy = new List<int>();
        public void RemovePropertyAnimation(int animationID) 
        {
            int animIndex = PropertyAnimations.FindIndex(p => p.AnimationID == animationID);

            if (animIndex != -1) 
            {
                _properyAnimationsToDestroy.Add(animIndex);
            }
        }
        private void DestroyQueuedPropertyAnimations() 
        {
            _properyAnimationsToDestroy.Sort((x,y) => y - x);

            _properyAnimationsToDestroy.ForEach(i =>
            {
                PropertyAnimations.RemoveAt(i);
            });

            _properyAnimationsToDestroy.Clear();
        }

        public void AddPropertyAnimation(PropertyAnimation animation) 
        {
            PropertyAnimations.Add(animation);
        }

        public void AddSingleUsePropertyAnimation(PropertyAnimation animation) 
        {
            animation.OnFinish = () =>
            {
                animation.Reset();
                RemovePropertyAnimation(animation.AnimationID);
            };

            PropertyAnimations.Add(animation);
        }

        public virtual void SetRender(bool render) 
        {
            Render = render;
        }


        private int _animationID = 0;
        public int NextAnimationID 
        {
            get 
            {
                return _animationID++;
            }
            private set 
            {
                NextAnimationID = value;
            }
        }

        public virtual void OnClick() { }
        public virtual void OnHover() 
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;
            }
        }
        public virtual void HoverEnd() 
        {
            if (Hovered) 
            {
                Hovered = false;
            }
        }
        public virtual void OnMouseDown() { }
        public virtual void OnMouseUp() { }
        public virtual void OnGrab(Vector2 MouseCoordinates) 
        {
            if (Draggable && !Grabbed)
            {
                Grabbed = true;

                Vector3 screenCoord = WindowConstants.ConvertLocalToScreenSpaceCoordinates(MouseCoordinates);

                _grabbedDeltaPos = screenCoord - Position;
            }
        }
        public virtual void GrabEnd() 
        {
            if (Grabbed)
            {
                Grabbed = false;
                _grabbedDeltaPos = default;
            }
            
        }

        public override bool Equals(object obj)
        {
            return obj is GameObject @object &&
                   ObjectID == @object.ObjectID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ObjectID);
        }
    }

    public class MultiTextureData
    {
        public bool MixTexture = false;
        public TextureUnit MixedTextureLocation = TextureUnit.Texture1;
        public float MixPercent = 0f;
        public Texture MixedTexture = null;
        public TextureName MixedTextureName = TextureName.Unknown;
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