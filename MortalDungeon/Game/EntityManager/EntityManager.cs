using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    public struct FeatureEntityEntry
    {
        public long Hash;
        public long FeatureId;

        public FeatureEntityEntry(long hash, long id)
        {
            Hash = hash;
            FeatureId = id;
        }
    }

    public static class EntityManager
    {
        public static HashSet<Entity> Entities = new HashSet<Entity>();
        public static HashSet<Entity> LoadedEntities = new HashSet<Entity>();

        private static Dictionary<FeatureEntityEntry, Entity> _featureEntitiesReference = new Dictionary<FeatureEntityEntry, Entity>();


        /// <summary>
        /// Adds the entity to the list of all entities. This doesn't imply anything about whether it is loaded or unloaded.
        /// </summary>
        public static void AddEntity(Entity entity) 
        {
            lock (Entities)
            {
                if (!Entities.Contains(entity))
                {
                    Entities.Add(entity);

                    _featureEntitiesReference.AddOrSet(new FeatureEntityEntry(entity.Handle.ObjectHash, entity.Handle.FeatureID), entity);
                }
            }
        }

        /// <summary>
        /// Removes an entity from the list of all entities. This will effectively destroy the entity.
        /// </summary>
        public static void RemoveEntity(Entity entity) 
        {
            lock (Entities) 
            {
                _featureEntitiesReference.Remove(new FeatureEntityEntry(entity.Handle.ObjectHash, entity.Handle.FeatureID));

                entity.Unload();
                Entities.Remove(entity);
                LoadedEntities.Remove(entity);
            }
        }

        public static void UnloadEntity(Entity entity)
        {
            lock (Entities)
            {
                //when unloading an entity, save their position so that they can potentially be reloaded into the same place.

                entity.Unload();
                LoadedEntities.Remove(entity);
            }
        }

        public static void LoadEntity(Entity entity, FeaturePoint position, bool placeOnTileMap = true)
        {
            lock (Entities)
            {
                entity.Load(position, placeOnTileMap);
                LoadedEntities.Add(entity);
            }
        }

        public static bool FeatureEntityExists(FeatureEntityEntry entityEntry)
        {
            return _featureEntitiesReference.ContainsKey(entityEntry);
        }

        public static Entity GetFeatureEntity(FeatureEntityEntry entityEntry)
        {
            return _featureEntitiesReference[entityEntry];
        }
    }
}
