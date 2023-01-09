using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public class LockedList<T>
    {
        private object _lockObj;
        private List<T> _list = new List<T>();

        public int Count { get { return _list.Count; } }

        public LockedList(object lockObj)
        {
            _lockObj = lockObj;
        }

        public void Add(T obj)
        {
            lock (_lockObj)
            {
                _list.Add(obj);
            }
        }

        public void Remove(T obj)
        {
            lock (_lockObj)
            {
                _list.Remove(obj);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lockObj)
            {
                _list.RemoveAt(index);
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                _list.Clear();
            }
        }

        public List<T> GetList()
        {
            return _list;
        }
    }
}
