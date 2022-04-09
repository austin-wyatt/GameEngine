using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Ledger.Units;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    

    public static class EntityManager
    {
        public static HashSet<Entity> Entities = new HashSet<Entity>();
        public static HashSet<Entity> LoadedEntities = new HashSet<Entity>();


        public static object _entityLock = new object();

        /// <summary>
        /// Adds the entity to the list of all entities. This doesn't imply anything about whether it is loaded or unloaded.
        /// </summary>
        public static void AddEntity(Entity entity) 
        {
            lock (_entityLock)
            {
                if (!Entities.Contains(entity))
                {
                    Entities.Add(entity);
                }
            }
        }

        /// <summary>
        /// Removes an entity from the list of all entities. This will effectively destroy the entity.
        /// </summary>
        public static void RemoveEntity(Entity entity) 
        {
            lock (_entityLock) 
            {
                UnitLedger.LedgerUnit(entity.Handle);

                entity.Unload();
                Entities.Remove(entity);
                LoadedEntities.Remove(entity);
            }
        }

        public static void UnloadEntity(Entity entity)
        {
            RemoveEntity(entity);

            //lock (_entityLock)
            //{
            //    //when unloading an entity, save their position so that they can potentially be reloaded into the same place.

            //    entity.Unload();
            //    LoadedEntities.Remove(entity);
            //}
        }

        public static void LoadEntity(Entity entity, FeaturePoint position, bool placeOnTileMap = true)
        {
            lock (_entityLock)
            {
                entity.Load(position, placeOnTileMap);
                LoadedEntities.Add(entity);
            }
        }

        
    }
}
