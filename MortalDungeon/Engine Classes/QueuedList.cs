using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class QueuedObjectList<T> : QueuedList<T> where T : GameObject
    {
        public QueuedObjectList()
        {

        }

        public new void Add(T item)
        {
            _itemsToAdd.Add(item);

            Rendering.Renderer.LoadTextureFromGameObj(item);
        }
    }

    public class QueuedUIList<T> : QueuedList<T> where T : UIObject
    {
        public QueuedUIList()
        {

        }

        public new void HandleQueuedItems()
        {

            AddQueuedItems();
            ClearQueuedItems();

            //ForEach(item =>
            //{
            //    bool hasChanges = item.Children.HasQueuedItems();

            //    item.Children.HandleQueuedItems();

            //    if (hasChanges)
            //    {
            //        item.ForceTreeRegeneration();
            //    }
            //});
        }
    }

    public class QueuedList<T> : List<T>
    {
        protected List<T> _itemsToAdd = new List<T>();
        protected List<T> _itemsToRemove = new List<T>();

        public QueuedList()
        {

        }

        public QueuedList(List<T> list)
        {
            Clear();
            _itemsToAdd.Clear();
            _itemsToRemove.Clear();

            list.ForEach(i => AddImmediate(i));
        }

        public QueuedList(IOrderedEnumerable<T> list)
        {
            Clear();
            _itemsToAdd.Clear();
            _itemsToRemove.Clear();

            foreach(T i in list) 
            {
                AddImmediate(i);
            }
        }

        public static QueuedList<T> FromEnumerable(IOrderedEnumerable<T> list) 
        {
            return new QueuedList<T>(list);
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
        }

        public void AddQueuedItems()
        {
            _itemsToAdd.ForEach(i =>
            {
                base.Add(i);
            });

            _itemsToAdd.Clear();
        }

        public void RemoveImmediate(T item)
        {
            base.Remove(item);
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

        public bool HasQueuedItems() 
        {
            return _itemsToAdd.Count > 0 || _itemsToRemove.Count > 0;
        }
    }
}
