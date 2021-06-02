using MortalDungeon.Game.GameObjects;
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
        public Vector2i ClientSize = new Vector2i();

        private Vector3 _velocity = default;
        private int _moveDelay = 1;
        private int _currentDelay = 0;
        private int _totalMoves = 0;
        private float _timeToArrive = 60; //move delays it will take to arrive. (1 _moveDelay and 60 _timeToArrive would be 1 second. 2 and 30 would also be 1 second.)
        private bool _moving = false;

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
                obj.CurrentAnimation.Tick();
                if (shouldMove) 
                {
                    obj.SetPosition(obj.Position + _velocity);
                }
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

        public void GradualMove(Vector3 finalPosition, int moveDelay = 1, float timeToArrive = 60) 
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
    }

    public class Unit : GameObject //main unit class. Tracks position on tilemap
    {
        public bool Render = true;
        public int TileMapPosition = 0; //can be anything from -1 to infinity. If the value is below 0 then it is not being positioned on the tilemap
        public Unit() { }
        public Unit(Vector2i clientSize, Vector3 position, int tileMapPosition = 0, int id = 0, string name = "Unit")
        {
            ClientSize = clientSize;
            Position = position;
            Name = name;
        }
    }

    public class TileMap : GameObject //grid of tiles
    {
        public bool Render = true;
        public int Width = 30;
        public int Height = 30;

        public List<BaseTile> Tiles = new List<BaseTile>();
        public TileMap(Vector2i clientSize, Vector3 position, int id = 0, string name = "TileMap")
        {
            ClientSize = clientSize;
            Position = position; //position of the first tile
            Name = name;
        }

        public void PopulateTileMap()
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(Position);

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    baseTile = new BaseTile(ClientSize, tilePosition, i * Width + o);

                    Tiles.Add(baseTile);

                    tilePosition.Y += baseTile.BaseObjects[0].Dimensions.Y;
                }
                tilePosition.X = (i + 1) * baseTile.BaseObjects[0].Dimensions.X / 1.29f;
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : baseTile.BaseObjects[0].Dimensions.Y / -2);
                tilePosition.Z += 0.0001f;
            }
        }

        public Vector3 GetPositionOfTile(int index)
        {
            Vector3 position = new Vector3();
            if (index < Width * Height && index >= 0)
            {
                position = Tiles[index].Position;
            }
            else if (index < 0 && Tiles.Count > 0)
            {
                position = Tiles[0].Position;
            }
            else if (index >= Tiles.Count)
            {
                position = Tiles[Tiles.Count - 1].Position;
            }

            return position;
        }

        public override void Tick()
        {
            base.Tick();

            Tiles.ForEach(tile =>
            {
                tile.Tick();
            });
        }
    }
}
