using Empyrean.Engine_Classes;
using Empyrean.Game.Ledger.Units;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Entities
{

    [Serializable]
    public struct PermanentId
    {
        public int Id;

        public PermanentId(int id)
        {
            Id = id;
        }

        public static PermanentId DEFAULT = new PermanentId(0);

        public override bool Equals(object obj)
        {
            return obj is PermanentId id &&
                   Id == id.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public bool TryGetUnit(out Unit unit)
        {
            if(EntityManager.EntitiesById.TryGetValue(this, out var entity))
            {
                unit = entity.Handle;
                return true;
            }

            unit = null;
            return false;
        }
    }
    public static class EntityManager
    {
        public static HashSet<Entity> Entities = new HashSet<Entity>();
        public static HashSet<Entity> LoadedEntities = new HashSet<Entity>();

        public static Dictionary<PermanentId, Entity> EntitiesById = new Dictionary<PermanentId, Entity>();

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
                    EntitiesById.AddOrSet(entity.Handle.PermanentId, entity);
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
                UnitPositionLedger.LedgerUnit(entity.Handle);

                entity.Unload();
                Entities.Remove(entity);
                LoadedEntities.Remove(entity);
                EntitiesById.Remove(entity.Handle.PermanentId);
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

        public static bool GetEntityByPermanentId(PermanentId id, out Entity entity)
        {
            return EntitiesById.TryGetValue(id, out entity);
        }
    }
}
