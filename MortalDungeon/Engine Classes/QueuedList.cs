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
            _itemsToAdd[_currentQueue].Add(item);

            Rendering.Renderer.LoadTextureFromGameObj(item);
        }
    }

    public class QueuedUIList<T> : QueuedList<T> where T : UIObject
    {
        public QueuedUIList()
        {

        }
    }

    public class QueuedList<T> : List<T>
    {
        protected List<List<T>> _itemsToAdd = CreateQueue();
        protected List<List<T>> _itemsToRemove = CreateQueue();

        protected int _currentQueue = 0;

        protected const int INTERNAL_QUEUES = 5;

        public int CHANGE_TOKEN { get; private set; }

        public QueuedList()
        {
            CHANGE_TOKEN = 0;
        }

        public QueuedList(List<T> list)
        {
            Clear();

            _itemsToAdd = CreateQueue();
            _itemsToRemove = CreateQueue();

            list.ForEach(i => AddImmediate(i));
        }

        public QueuedList(IOrderedEnumerable<T> list)
        {
            Clear();

            _itemsToAdd = CreateQueue();
            _itemsToRemove = CreateQueue();

            foreach (T i in list) 
            {
                AddImmediate(i);
            }
        }

        protected static List<List<T>> CreateQueue() 
        {
            List<List<T>> temp = new List<List<T>>();

            for (int i = 0; i < INTERNAL_QUEUES; i++) 
            {
                temp.Add(new List<T>());
            }

            return temp;
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
            lock(_itemsToAdd[_currentQueue])
            _itemsToAdd[_currentQueue].Add(item);
        }

        public void AddQueuedItems(int queue)
        {
            lock (_itemsToAdd[_currentQueue])
            {
                for (int i = 0; i < _itemsToAdd[queue].Count; i++)
                {
                    base.Add(_itemsToAdd[queue][i]);
                }

                _itemsToAdd[queue].Clear();
            }
        }

        public void RemoveImmediate(T item)
        {
            base.Remove(item);
        }
        public new void Remove(T item)
        {
            lock (_itemsToRemove[_currentQueue])
            _itemsToRemove[_currentQueue].Add(item);
        }

        public void ClearQueuedItems(int queue)
        {
            lock (_itemsToRemove[_currentQueue])
            {
                for (int i = 0; i < _itemsToRemove[queue].Count; i++)
                {
                    base.Remove(_itemsToRemove[queue][i]);
                }

                _itemsToRemove[queue].Clear();
            }
        }

        public void HandleQueuedItems()
        {
            if (!HasQueuedItems()) return;

            CHANGE_TOKEN++;

            int queue = _currentQueue;
            _currentQueue = (queue + 1) % INTERNAL_QUEUES;

            AddQueuedItems(queue);
            ClearQueuedItems(queue);
        }

        public bool HasQueuedItems() 
        {
            return _itemsToAdd[_currentQueue].Count > 0 || _itemsToRemove[_currentQueue].Count > 0;
        }

        public void ManuallyIncrementChangeToken() 
        {
            CHANGE_TOKEN++;
        }
    }
}
