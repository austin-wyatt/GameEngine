using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class QueuedObjectList<T> : List<T> where T : GameObject
    {
        private List<T> _itemsToAdd = new List<T>();
        private List<T> _itemsToRemove = new List<T>();

        public QueuedObjectList()
        {

        }

        /// <summary>
        /// Immediately adds the item to the list instead of queueing it for the next tick.
        /// </summary>
        public void AddImmediate(T item)
        {
            base.Add(item);
        }

        public new void Add(T item)
        {
            _itemsToAdd.Add(item);

            Rendering.Renderer.LoadTextureFromGameObj(item);
        }

        public void AddQueuedItems()
        {
            _itemsToAdd.ForEach(i =>
            {
                base.Add(i);
            });

            _itemsToAdd.Clear();
        }

        public new void Remove(T item)
        {
            _itemsToRemove.Add(item);
        }

        public void ClearQueuedItems()
        {
            _itemsToRemove.ForEach(i =>
            {
                base.Remove(i);
            });

            _itemsToRemove.Clear();
        }

        public void HandleQueuedItems() 
        {
            AddQueuedItems();
            ClearQueuedItems();
        }
    }
}
