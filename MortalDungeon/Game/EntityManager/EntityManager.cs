using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    internal static class EntityManager
    {
        internal static List<Entity> Entities = new List<Entity>();

        /// <summary>
        /// Adds the entity to the list of all entities. This doesn't imply anything about whether it is loaded or unloaded.
        /// </summary>
        internal static void AddEntity(Entity entity) 
        {
            lock(Entities)
            if (!Entities.Contains(entity)) 
            {
                Entities.Add(entity);
            }
        }

        /// <summary>
        /// Removes an entity from the list of all entities. This will effectively destroy the entity.
        /// </summary>
        internal static void RemoveEntity(Entity entity) 
        {
            lock (Entities) 
            {
                entity.Unload();
                Entities.Remove(entity);
            }
        }
    }
}
