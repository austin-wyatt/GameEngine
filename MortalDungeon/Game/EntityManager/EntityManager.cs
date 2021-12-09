using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Entities
{
    public static class EntityManager
    {
        public static List<Entity> Entities = new List<Entity>();

        /// <summary>
        /// Adds the entity to the list of all entities. This doesn't imply anything about whether it is loaded or unloaded.
        /// </summary>
        public static void AddEntity(Entity entity) 
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
        public static void RemoveEntity(Entity entity) 
        {
            lock (Entities) 
            {
                entity.Unload();
                Entities.Remove(entity);
            }
        }
    }
}
