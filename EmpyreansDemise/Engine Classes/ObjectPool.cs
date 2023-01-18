using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public class ObjectPool<T> where T : new()
    {
        public int Capacity = 100;

        private Stack<T> PooledObjects;

        public int Count { get { return PooledObjects.Count; } }

        private object _lock = new object();
        public ObjectPool()
        {
            PooledObjects = new Stack<T>(Capacity);
        }

        public ObjectPool(int capacity)
        {
            Capacity = capacity;
            PooledObjects = new Stack<T>(Capacity);
        }

        public T GetObject()
        {
            lock (_lock)
            {
                if (PooledObjects.Count == 0)
                {
                    return new T();
                }

                return PooledObjects.Pop();
            }
        }

        public void FreeObject(ref T obj)
        {
            lock (_lock)
            {
                if (PooledObjects.Count < Capacity)
                {
                    PooledObjects.Push(obj);
                }
            }
        }

        /// <summary>
        /// Use this if the object is already a reference type
        /// </summary>
        public void FreeObject(T obj)
        {
            lock (_lock)
            {
                if (PooledObjects.Count < Capacity)
                {
                    PooledObjects.Push(obj);
                }
            }
        }

        public void EmptyPool()
        {
            lock (_lock)
            {
                PooledObjects.Clear();
            }
        }
    }
}
