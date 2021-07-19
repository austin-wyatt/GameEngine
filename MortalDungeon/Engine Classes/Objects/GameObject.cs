using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.UI;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

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

        private Vector3 _velocity = default;
        private int _moveDelay = 1;
        private int _currentDelay = 0;
        private int _totalMoves = 0;
        private float _timeToArrive = WindowConstants.TickDenominator; //move delays it will take to arrive. (1 _moveDelay and 60 _timeToArrive would be 1 second. 2 and 30 would also be 1 second.)
        private bool _moving = false;

        public bool Render = true;
        public bool Clickable = false; //Note: The BaseObject's Clickable property and this property must be true for UI objects

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

        public virtual void Tick() 
        {
            bool shouldMove = _moving && (_moveDelay == _currentDelay);

            BaseObjects.ForEach(obj =>
            {
                obj._currentAnimation.Tick();
                //obj.PropertyAnimations.ForEach(anim =>
                //{
                //    anim.Tick();
                //});

                if (shouldMove) 
                {
                    obj.SetPosition(obj.Position + _velocity);
                }
            });

            PropertyAnimations.ForEach(anim =>
            {
                anim.Tick();
            });

            ParticleGenerators.ForEach(gen =>
            {
                gen.Tick();
                if (shouldMove)
                {
                    gen.SetPosition(gen.Position + _velocity);
                }
            });

            if (shouldMove)
            {
                _currentDelay = 0;
                Position += _velocity;
                _totalMoves++;
            }
            else if (_moving) 
            {
                _currentDelay++;
            }

            if (_totalMoves >= _timeToArrive) 
            {
                _moving = false;
                _totalMoves = 0;
                _velocity.X = 0;
                _velocity.Y = 0;
                _velocity.Z = 0;
            }
        }

        public void GradualMove(Vector3 finalPosition, int moveDelay = 1, float timeToArrive = WindowConstants.TickDenominator) 
        {
            if (_velocity.X != 0 || _velocity.Y != 0 || _velocity.Z != 0) 
            {
                SetPosition(Position + _velocity * (_timeToArrive - _totalMoves) * _moveDelay);
                _totalMoves = 0;
            }
            _timeToArrive = timeToArrive / moveDelay; //contrary to the internal variable, _timeToArrive will always be measured in the ticks the move will take when calling the function
            _velocity = (finalPosition - Position) / _timeToArrive;

            _moving = true;
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

        public PropertyAnimation GetPropertyAnimationByID(int id) 
        {
            return PropertyAnimations.Find(anim => anim.AnimationID == id);
        }

        public virtual void OnClick() { }
        public virtual void OnHover() { }
        public virtual void HoverEnd() { }
        public virtual void OnMouseDown() { }
        public virtual void OnMouseUp() { }
        public virtual void OnGrab() { }
        public virtual void GrabEnd() { }

    }
}
