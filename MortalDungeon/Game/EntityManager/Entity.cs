using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    internal interface ILoadableEntity 
    {
        public void EntityLoad(FeaturePoint position);
        public void EntityUnload();
    }

    //internal class Entity<T> where T : ILoadableEntity
    internal class Entity
    {
        internal Unit Handle;

        internal int EntityID => _entityID;
        protected int _entityID = _currentEntityID++;
        protected static int _currentEntityID = 0;

        internal bool Loaded { get; private set; }

        internal bool DestroyOnUnload = false;

        internal Entity(Unit handle)
        {
            Handle = handle;
            handle.EntityHandle = this;
        }

        internal void Load(FeaturePoint position) 
        {
            if (!Loaded) 
            {
                if (Handle.Scene._tileMapController.IsValidTile(position)) 
                {
                    Loaded = true;
                    Handle.EntityLoad(position);

                    Handle.Scene._units.Add(Handle);
                }
            }
        }
        internal void Unload() 
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
