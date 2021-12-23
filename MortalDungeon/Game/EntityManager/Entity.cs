using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    public interface ILoadableEntity 
    {
        public void EntityLoad(FeaturePoint position, bool placeOnTileMap = true);
        public void EntityUnload();
    }

    //public class Entity<T> where T : ILoadableEntity
    public class Entity
    {
        public Unit Handle;

        public int EntityID => _entityID;
        protected int _entityID = _currentEntityID++;
        protected static int _currentEntityID = 0;

        public bool Loaded { get; private set; }

        public bool DestroyOnUnload = false;

        public Entity(Unit handle)
        {
            Handle = handle;
            handle.EntityHandle = this;
        }

        public void Load(FeaturePoint position, bool placeOnTileMap = true) 
        {
            if (!Loaded) 
            {
                if (Handle.Scene._tileMapController.IsValidTile(position)) 
                {
                    Loaded = true;
                    Handle.EntityLoad(position, placeOnTileMap);

                    Handle.Scene._units.Add(Handle);
                }
            }
        }
        public void Unload() 
        {
            if (Loaded)
            {
                Loaded = false;
                Handle.EntityUnload();

                if (DestroyOnUnload) 
                {
                    EntityManager.RemoveEntity(this);
                }
            }
        }
        
    }
}
