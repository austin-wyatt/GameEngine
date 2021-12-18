using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class QueuedObjectList<T> : QueuedList<T> where T : GameObject
    {
        internal QueuedObjectList()
        {

        }

        internal new void Add(T item)
        {
            _itemsToAdd[_currentQueue].Add(item);

            void loadTex()
            {
                Rendering.Renderer.LoadTextureFromGameObj(item);
                Rendering.Renderer.OnRender -= loadTex;
            };

            Rendering.Renderer.OnRender += loadTex;
        }
    }

    internal class QueuedUIList<T> : QueuedList<T> where T : UIObject
    {
        internal QueuedUIList()
        {

        }
    }

    internal class QueuedList<T> : List<T>
    {
        protected List<List<T>> _itemsToAdd = CreateQueue();
        protected List<List<T>> _itemsToRemove = CreateQueue();

        protected int _currentQueue = 0;

        protected const int INTERNAL_QUEUES = 2;

        internal int CHANGE_TOKEN { get; private set; }

        internal QueuedList()
        {
            CHANGE_TOKEN = 0;
        }

        internal QueuedList(List<T> list)
        {
            Clear();

            _itemsToAdd = CreateQueue();
            _itemsToRemove = CreateQueue();

            list.ForEach(i => AddImmediate(i));
        }

        internal QueuedList(IOrderedEnumerable<T> list)
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

        internal static QueuedList<T> FromEnumerable(IOrderedEnumerable<T> list) 
        {
            return new QueuedList<T>(list);
        }

        /// <summary>
        /// Immediately adds the item to the list instead of queueing it for the next tick.
        /// </summary>
        internal void AddImmediate(T item)
        {
            lock (this)
            {
                base.Add(item);
            }
        }

        internal new void Add(T item)
        {
            lock(_itemsToAdd[_currentQueue])
            _itemsToAdd[_currentQueue].Add(item);
        }

        internal void AddQueuedItems(int queue)
        {
            lock (this)
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
        }

        internal void RemoveImmediate(T item)
        {
            lock (this)
            {
                base.Remove(item);
            }
        }
        internal new void Remove(T item)
        {
            lock (_itemsToRemove[_currentQueue])
            _itemsToRemove[_currentQueue].Add(item);
        }

        internal void ClearQueuedItems(int queue)
        {
            lock (this)
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
        }

        internal void HandleQueuedItems()
        {
            if (!HasQueuedItems()) return;

            CHANGE_TOKEN++;

            int queue = _currentQueue;
            _currentQueue = (queue + 1) % INTERNAL_QUEUES;

            AddQueuedItems(queue);
            ClearQueuedItems(queue);
        }

        internal bool HasQueuedItems() 
        {
            return _itemsToAdd[_currentQueue].Count > 0 || _itemsToRemove[_currentQueue].Count > 0;
        }

        internal void ManuallyIncrementChangeToken() 
        {
            CHANGE_TOKEN++;
        }
    }
}
