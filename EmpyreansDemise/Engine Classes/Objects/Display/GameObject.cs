using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace Empyrean.Engine_Classes
{
    public enum SetColorFlag 
    {
        Base,
        Hover,
        Disabled,
        Selected
    }

    public class GameObject : ITickable, IHoverable, IHasPosition
    {
        public string Name = "";

        public Vector3 _position;
        public Vector3 Position { get => _position; set => _position = value; }

        public List<BaseObject> BaseObjects = new List<BaseObject>();

        public List<ParticleGenerator> ParticleGenerators = new List<ParticleGenerator>();

        public Vector3 Scale = new Vector3(1, 1, 1);

        public BaseObject BaseObject => BaseObjects.Count > 0 ? BaseObjects[0] : null;

        public List<PropertyAnimation> PropertyAnimations = new List<PropertyAnimation>();

        public bool Cull = false; //whether the object was determined to be outside of the camera's view and should be culled
        public bool Render = true;
        public virtual bool Clickable { get; set; } //Note: The BaseObject's Clickable property and this property must be true for UI objects
        public virtual bool Hoverable { get; set; }

        public bool Draggable = false;
        public bool HasTimedHoverEffect = false;

        public bool Hovered = false;
        public bool Grabbed = false;

        public bool HasContextMenu = false;

        public bool TextureLoaded = false;
        public bool _canLoadTexture = true;

        public int ObjectID => _objectID;
        protected int _objectID = currentObjectID++;
        protected static int currentObjectID = 0;

        public MultiTextureData MultiTextureData = new MultiTextureData();

        public ScissorData ScissorData = ScissorData.Empty;

        public ObjectType ObjectType = ObjectType.GenericObject;

        public Vector3 _grabbedDeltaPos = default;

        //public Stats Stats; //contains game parameters for the object
        public GameObject() { }

        public GameObject(Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) 
        {
            RenderableObject renderableObj = new RenderableObject(new SpritesheetObject(spritesheetPos, spritesheet).CreateObjectDefinition(ObjectIDs.Unknown, EnvironmentObjects.QuadBounds, true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            Animation anim = new Animation()
            {
                Frames = new List<RenderableObject>() { renderableObj },
                Frequency = 0,
                Repeats = 0,
                GenericType = 0
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { anim }, ObjectID, "Game object " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            baseObj.BaseFrame.CameraPerspective = true;

            AddBaseObject(baseObj);

            SetPosition(position);

            LoadTexture(this);
        }

        public BaseObject CreateBaseObjectFromSpritesheet(Spritesheet spritesheet, int spritesheetPos, float[] bounds = null) 
        {
            RenderableObject renderableObj;

            if (bounds != null) 
            {
                renderableObj = new RenderableObject(new SpritesheetObject(spritesheetPos, spritesheet).CreateObjectDefinition(ObjectIDs.Unknown, bounds, true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            }
            else
            {
                renderableObj = new RenderableObject(new SpritesheetObject(spritesheetPos, spritesheet).CreateObjectDefinition(ObjectIDs.Unknown, EnvironmentObjects.BaseTileBounds, true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            }
            

            Animation anim = new Animation()
            {
                Frames = new List<RenderableObject>() { renderableObj },
                Frequency = 0,
                Repeats = 0,
                GenericType = 0
            };

            BaseObject baseObj;

            if (bounds != null)
            {
                baseObj = new BaseObject(new List<Animation>() { anim }, ObjectID, "Game object " + ObjectID, default, bounds);
            }
            else
            {
                baseObj = new BaseObject(new List<Animation>() { anim }, ObjectID, "Game object " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            }

                
            baseObj.BaseFrame.CameraPerspective = true;

            return baseObj;
        }

        public virtual void SetName(string name) 
        {
            Name = name;
        }

        public virtual void SetPosition(float x, float y, float z)
        {
            for (int i = 0; i < BaseObjects.Count; i++)
            {
                BaseObjects[i].SetPosition(x - (_position.X - BaseObjects[i].Position.X), 
                    y - (_position.Y - BaseObjects[i].Position.Y),
                    z - (_position.Z - BaseObjects[i].Position.Z));
            }

            for (int i = 0; i < ParticleGenerators.Count; i++)
            {
                Vector3 deltaPos = Position - ParticleGenerators[i].Position;

                ParticleGenerators[i].SetPosition(x - (_position.X - ParticleGenerators[i].Position.X),
                    y - (_position.Y - ParticleGenerators[i].Position.Y),
                    z - (_position.Z - ParticleGenerators[i].Position.Z));
            }

            _position.X = x;
            _position.Y = y;
            _position.Z = z;

        }

        public virtual void SetPosition(Vector3 position) 
        {
            for(int i = 0; i < BaseObjects.Count; i++)
            {
                Vector3 deltaPos = Position - BaseObjects[i].Position;

                BaseObjects[i].SetPosition(position - deltaPos);
            }

            for(int i = 0; i < ParticleGenerators.Count; i++)
            {
                Vector3 deltaPos = Position - ParticleGenerators[i].Position;

                ParticleGenerators[i].SetPosition(position - deltaPos);
            }

            _position = position;
        }


        public delegate void DragEventHandler(object sender, Vector3 mouseCoordinates, Vector3 position, Vector3 deltaDrag);
        public DragEventHandler Drag;

        public delegate bool DragPreviewHandler(object sender, Vector3 mouseCoordinates, Vector3 position, Vector3 deltaDrag);
        public DragPreviewHandler PreviewDrag;

        public virtual void DragEvent(Vector3 position, Vector3 mouseCoord, Vector3 deltaDrag)
        {
            if (PreviewDrag == null || PreviewDrag.Invoke(this, mouseCoord, position, deltaDrag))
            {
                SetPosition(position);

                Drag?.Invoke(this, mouseCoord, position, deltaDrag);
            }
        }

        public virtual void AddBaseObject(BaseObject obj) 
        {
            BaseObjects.Add(obj);
        }

        public virtual void RemoveBaseObject(BaseObject obj) 
        {
            BaseObjects.Remove(obj);
        }

        public virtual void Tick() 
        {
            int i;
            for(i = 0; i < BaseObjects.Count; i++)
            {
                BaseObjects[i]._currentAnimation.Tick();
            }

            if (_properyAnimationsToDestroy.Count > 0)
            {
                DestroyQueuedPropertyAnimations();
            }

            if (_properyAnimationsToAdd.Count > 0) 
            {
                AddQueuedPropertyAnimations();
            }

            for(i = 0; i < PropertyAnimations.Count; i++)
            {
                PropertyAnimations[i].Tick();
            }

            for (i = 0; i < ParticleGenerators.Count; i++)
            {
                ParticleGenerators[i].Tick();
            }
        }

        public virtual void ScaleAll(float f) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAll(f);
            });

            Scale *= f;
        }

        public virtual void ScaleXY(float x, float y)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleX(x);
                obj.BaseFrame.ScaleY(y);
            });

            Scale.X *= x;
            Scale.Y *= y;
        }

        public virtual void SetScale(float f) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetScaleAll(f);
            });

            Scale = new Vector3(f, f, f);
        }

        public virtual void SetScale(float x, float y, float z)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetScale(x, y, z);
            });

            Scale = new Vector3(x, y, z);
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

        public virtual void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetBaseColor(color);
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
        private readonly List<int> _properyAnimationsToDestroy = new List<int>();
        public void RemovePropertyAnimation(int animationID) 
        {
            int animIndex = PropertyAnimations.FindIndex(p => p.AnimationID == animationID);

            if (animIndex != -1) 
            {
                _properyAnimationsToDestroy.Add(animIndex);
            }
        }

        public void RemovePropertyAnimation(PropertyAnimation animation)
        {
            int animIndex = PropertyAnimations.FindIndex(p => p.AnimationID == animation.AnimationID);

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

        private readonly List<PropertyAnimation> _properyAnimationsToAdd = new List<PropertyAnimation>();
        public void AddPropertyAnimation(PropertyAnimation animation) 
        {
            //PropertyAnimations.Add(animation);
            _properyAnimationsToAdd.Add(animation);
        }

        private void AddQueuedPropertyAnimations() 
        {
            _properyAnimationsToAdd.ForEach(p =>
            {
                PropertyAnimations.Add(p);
            });

            _properyAnimationsToAdd.Clear();
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

        public event GameObjectEventHandler TextureLoad;
        public virtual void SetTextureLoaded(bool textureLoaded)
        {
            TextureLoaded = textureLoaded;

            if (TextureLoaded)
            {
                TextureLoad?.Invoke(this);
            }
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


        public event EventHandler Click;
        public event EventHandler RightClick;
        public event EventHandler MouseDown;

        public virtual void OnClick() 
        {
            Click?.Invoke(this, null);
        }
        public virtual void OnRightClick() 
        {
            RightClick?.Invoke(this, null);
        }
        public virtual void OnHover() 
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                HoverEvent(this);
            }
        }
        public virtual void OnHoverEnd() 
        {
            if (Hovered) 
            {
                Hovered = false;

                HoverEndEvent(this);
            }
        }

        public virtual void OnTimedHover() 
        {
            TimedHoverEvent(this);
        }

        public virtual void OnTimedHover(GameObject obj)
        {
            TimedHoverEvent(obj);
        }

        public virtual void OnMouseDown() 
        {
            MouseDown?.Invoke(this, null);
        }
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

        public virtual void OnCull() { }

        public virtual Tooltip CreateContextMenu() 
        {
            return null;
        }

        public virtual void CleanUp() 
        {
            OnCleanUp?.Invoke(this);

            TimedHover = null;
            Hover = null;

            OnHoverEnd();
            HoverEnd = null;
        }

        #region Event actions
        public delegate void GameObjectEventHandler(GameObject obj);

        public event GameObjectEventHandler OnCleanUp;
        public event GameObjectEventHandler HoverEnd;
        public event GameObjectEventHandler Hover;
        public event GameObjectEventHandler TimedHover;

        protected void HoverEndEvent(GameObject obj) 
        {
            HoverEnd?.Invoke(obj);
        }

        protected void CleanUpEvent(GameObject obj)
        {
            OnCleanUp?.Invoke(obj);
        }

        protected void HoverEvent(GameObject obj)
        {
            Hover?.Invoke(obj);
        }

        protected void TimedHoverEvent(GameObject obj)
        {
            TimedHover?.Invoke(obj);
        }
        #endregion


        public static void LoadTexture<T>(T obj) where T : GameObject 
        {
            //void loadTex()
            //{
            //    Renderer.LoadTextureFromGameObj(obj);
            //    Renderer.OnRender -= loadTex;
            //};

            //Renderer.OnRender += loadTex;

            TextureLoadBatcher.LoadTexture(obj);
        }

        public static void LoadTextures<T>(IEnumerable<T> obj, bool nearest = true, bool generateMipMaps = true) where T : GameObject
        {
            void loadTex()
            {
                lock (obj) 
                {
                    foreach(var item in obj)
                    {
                        Renderer.LoadTextureFromGameObj(item, nearest, generateMipMaps);
                    }
                }
            };

            Window.QueueToRenderCycle(loadTex);
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

        public object this[string propertyName]
        {
            get
            {
                Type type = typeof(GameObject);
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                return propertyInfo.GetValue(this, null);
            }
            set
            {
                Type type = typeof(GameObject);
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                propertyInfo.SetValue(this, value, null);
            }
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
        public TransformableMesh ScissoredArea;

        public ScissorData()
        {
            ScissoredArea = new TransformableMesh(StaticObjects.QUAD_VERTICES, null);
        }

        private ScissorData(int _) { }

        public bool Scissor = false;

        public bool _scissorFlag = false;

        public static ScissorData Empty = new ScissorData(0);
    }
}