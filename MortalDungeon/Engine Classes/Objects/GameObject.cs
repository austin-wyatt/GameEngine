using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace MortalDungeon.Engine_Classes
{
    internal class GameObject : ITickable
    {
        internal string Name = "";
        internal Vector3 Position = new Vector3();
        internal List<BaseObject> BaseObjects = new List<BaseObject>();
        internal List<ParticleGenerator> ParticleGenerators = new List<ParticleGenerator>();
        internal Vector3 Scale = new Vector3(1, 1, 1);
        internal BaseObject BaseObject => BaseObjects.Count > 0 ? BaseObjects[0] : null;

        internal List<PropertyAnimation> PropertyAnimations = new List<PropertyAnimation>();

        internal bool Cull = false; //whether the object was determined to be outside of the camera's view and should be culled
        internal bool Render = true;
        internal bool Clickable = false; //Note: The BaseObject's Clickable property and this property must be true for UI objects
        internal bool Hoverable = false;
        internal bool Draggable = false;
        internal bool HasTimedHoverEffect = false;

        internal bool Hovered = false;
        internal bool Grabbed = false;

        internal bool HasContextMenu = false;

        internal bool TextureLoaded = false;

        internal int ObjectID => _objectID;
        protected int _objectID = currentObjectID++;
        protected static int currentObjectID = 0;


        internal MultiTextureData MultiTextureData = new MultiTextureData();

        internal ScissorData ScissorData = new ScissorData();


        internal ObjectType ObjectType = ObjectType.GenericObject;


        internal Vector3 _grabbedDeltaPos = default;

        //internal Stats Stats; //contains game parameters for the object
        internal GameObject() { }

        internal GameObject(Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) 
        {
            RenderableObject renderableObj = new RenderableObject(new SpritesheetObject(spritesheetPos, spritesheet).CreateObjectDefinition(ObjectIDs.Unknown, EnvironmentObjects.BaseTileBounds, true, false), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

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

        internal BaseObject CreateBaseObjectFromSpritesheet(Spritesheet spritesheet, int spritesheetPos) 
        {
            RenderableObject renderableObj = new RenderableObject(new SpritesheetObject(spritesheetPos, spritesheet).CreateObjectDefinition(ObjectIDs.Unknown, EnvironmentObjects.BaseTileBounds, true, false), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            Animation anim = new Animation()
            {
                Frames = new List<RenderableObject>() { renderableObj },
                Frequency = 0,
                Repeats = 0,
                GenericType = 0
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { anim }, ObjectID, "Game object " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            baseObj.BaseFrame.CameraPerspective = true;

            return baseObj;
        }

        internal virtual void SetName(string name) 
        {
            Name = name;
        }

        internal virtual void SetPosition(Vector3 position) 
        {
            BaseObjects.ForEach(obj =>
            {
                Vector3 deltaPos = Position - obj.Position;

                obj.SetPosition(position - deltaPos);
            });

            ParticleGenerators.ForEach(particleGen =>
            {
                Vector3 deltaPos = Position - particleGen.Position;

                particleGen.SetPosition(position - deltaPos);
            });

            Position = position;
        }

        internal virtual void SetDragPosition(Vector3 position)
        {
            SetPosition(position);
        }

        internal virtual void AddBaseObject(BaseObject obj) 
        {
            BaseObjects.Add(obj);
        }

        internal virtual void RemoveBaseObject(BaseObject obj) 
        {
            BaseObjects.Remove(obj);
        }

        internal virtual void Tick() 
        {
            BaseObjects.ForEach(obj =>
            {
                obj._currentAnimation.Tick();
            });

            if (_properyAnimationsToDestroy.Count > 0)
            {
                DestroyQueuedPropertyAnimations();
            }

            if (_properyAnimationsToAdd.Count > 0) 
            {
                AddQueuedPropertyAnimations();
            }

            PropertyAnimations.ForEach(anim =>
            {
                anim.Tick();
            });


            ParticleGenerators.ForEach(gen =>
            {
                gen.Tick();
            });
        }

        internal virtual void ScaleAll(float f) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAll(f);
            });

            Scale *= f;
        }

        internal virtual void SetScale(float f) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetScaleAll(f);
            });

            Scale = new Vector3(f, f, f);
        }

        internal virtual void SetScale(float x, float y, float z)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetScale(x, y, z);
            });

            Scale = new Vector3(x, y, z);
        }

        internal virtual void ScaleAddition(float f)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.ScaleAddition(f);
            });

            Scale.X += f;
            Scale.Y += f;
            Scale.Z += f;
        }

        internal virtual void SetColor(Vector4 color) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.BaseFrame.SetBaseColor(color);
            });
        }

        internal virtual RenderableObject GetDisplay()
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

        internal Vector3 GetDimensions()
        {
            Vector3 dimensions = default;
            if (BaseObjects.Count > 0) 
            {
                dimensions = BaseObjects[0].Dimensions;
            }

            return dimensions;
        }

        internal PropertyAnimation GetPropertyAnimationByID(int id) 
        {
            return PropertyAnimations.Find(anim => anim.AnimationID == id);
        }
        private readonly List<int> _properyAnimationsToDestroy = new List<int>();
        internal void RemovePropertyAnimation(int animationID) 
        {
            int animIndex = PropertyAnimations.FindIndex(p => p.AnimationID == animationID);

            if (animIndex != -1) 
            {
                _properyAnimationsToDestroy.Add(animIndex);
            }
        }

        internal void RemovePropertyAnimation(PropertyAnimation animation)
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
        internal void AddPropertyAnimation(PropertyAnimation animation) 
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



        internal void AddSingleUsePropertyAnimation(PropertyAnimation animation) 
        {
            animation.OnFinish = () =>
            {
                animation.Reset();
                RemovePropertyAnimation(animation.AnimationID);
            };

            PropertyAnimations.Add(animation);
        }

        internal virtual void SetRender(bool render) 
        {
            Render = render;
        }


        private int _animationID = 0;
        internal int NextAnimationID 
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

        internal virtual void OnClick() { }
        internal virtual void OnRightClick() { }
        internal virtual void OnHover() 
        {
            if (Hoverable && !Hovered)
            {
                Hovered = true;

                HoverEvent(this);
            }
        }
        internal virtual void OnHoverEnd() 
        {
            if (Hovered) 
            {
                Hovered = false;

                HoverEndEvent(this);
            }
        }

        internal virtual void OnTimedHover() 
        {
            TimedHoverEvent(this);
        }

        internal virtual void OnMouseDown() { }
        internal virtual void OnMouseUp() { }
        internal virtual void OnGrab(Vector2 MouseCoordinates) 
        {
            if (Draggable && !Grabbed)
            {
                Grabbed = true;

                Vector3 screenCoord = WindowConstants.ConvertLocalToScreenSpaceCoordinates(MouseCoordinates);

                _grabbedDeltaPos = screenCoord - Position;
            }
        }
        internal virtual void GrabEnd() 
        {
            if (Grabbed)
            {
                Grabbed = false;
                _grabbedDeltaPos = default;
            }
            
        }

        internal virtual void OnCull() { }

        internal virtual Tooltip CreateContextMenu() 
        {
            return null;
        }

        internal virtual void CleanUp() 
        {
            OnCleanUp?.Invoke(this);
        }

        #region Event actions
        internal delegate void GameObjectEventHandler(GameObject obj);

        internal event GameObjectEventHandler OnCleanUp;
        internal event GameObjectEventHandler OnHoverEndEvent;
        internal event GameObjectEventHandler OnHoverEvent;
        internal event GameObjectEventHandler OnTimedHoverEvent;

        protected void HoverEndEvent(GameObject obj) 
        {
            OnHoverEndEvent?.Invoke(obj);
        }

        protected void CleanUpEvent(GameObject obj)
        {
            OnCleanUp?.Invoke(obj);
        }

        protected void HoverEvent(GameObject obj)
        {
            OnHoverEvent?.Invoke(obj);
        }

        protected void TimedHoverEvent(GameObject obj)
        {
            OnTimedHoverEvent?.Invoke(obj);
        }

        #endregion


        internal static void LoadTexture<T>(T obj) where T : GameObject 
        {
            void loadTex()
            {
                Renderer.LoadTextureFromGameObj(obj);
                Renderer.OnRender -= loadTex;
            };

            Renderer.OnRender += loadTex;
        }

        internal static void LoadTextures<T>(List<T> obj) where T : GameObject
        {
            void loadTex()
            {
                lock (obj) 
                {
                    obj.ForEach(item =>
                    {
                        Renderer.LoadTextureFromGameObj(item);
                    });

                    Renderer.OnRender -= loadTex;
                }
            };

            Renderer.OnRender += loadTex;
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

    internal class MultiTextureData
    {
        internal bool MixTexture = false;
        internal TextureUnit MixedTextureLocation = TextureUnit.Texture1;
        internal float MixPercent = 0f;
        internal Texture MixedTexture = null;
        internal TextureName MixedTextureName = TextureName.Unknown;
    }

    internal class ScissorData
    {
        internal int X = 0;
        internal int Y = 0;
        internal int Width = 0;
        internal int Height = 0;
        internal int Depth = 0;

        internal bool Scissor = false;

        internal bool _scissorFlag = false;
        internal int _startingDepth = 0;
    }
}