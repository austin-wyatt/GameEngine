using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class UIManager
    {
        public List<UIObject> TopLevelObjects = new List<UIObject>();
        public object _UILock = new object();

        //common events that benefit from storing all objects that are relevant.
        //add or remove objects to this while building the reverse trees
        public List<UIObject> ClickableObjects = new List<UIObject>();
        public HashSet<UIObject> _clickableObjects = new HashSet<UIObject>();

        public List<UIObject> HoverableObjects = new List<UIObject>();
        public HashSet<UIObject> _hoverableObjects = new HashSet<UIObject>();


        public object _clickableObjectLock = new object();
        public object _hoverableObjectLock = new object();

        public void AddUIObject(UIObject obj, int zIndex)
        {
            obj.ZIndex = zIndex;
            obj.ManagerHandle = this;

            lock (_UILock)
            {
                TopLevelObjects.Add(obj);
            }

            SortUIObjects();
        }

        public void RemoveUIObject(UIObject obj)
        {
            lock (_UILock)
            {
                TopLevelObjects.Remove(obj);
            }
        }

        public void SortUIObjects()
        {
            lock (_UILock) 
            {
                TopLevelObjects.Sort();

                float baseZVal = 0;

                foreach (UIObject obj in TopLevelObjects)
                {
                    obj.GenerateReverseTree(this);

                    obj.GenerateZPositions(baseZVal);

                    baseZVal += 0.0001f;
                }
            }
        }

        public void GenerateReverseTree(UIObject obj)
        {
            int index = TopLevelObjects.IndexOf(obj);

            obj.GenerateReverseTree(this);

            if (index != -1)
            {
                //obj.GenerateZPositions(0.0001f * index);
                obj.GenerateZPositions(0.001f * index);

                Console.WriteLine("Generated Z positions for object: " + obj.Name);
            }

            //float stepAmount = 1 / (float)obj.ReverseTree.Count;
            //int count = 0;
            //foreach(var item in obj.ReverseTree)
            //{
            //    item.UIObject.SetColor(new OpenTK.Mathematics.Vector4(stepAmount * count, 0, 0, 1));
            //    count++;
            //}
        }

        #region clickable objects
        private object _clickableSetLock = new object();
        public void AddClickableObject(UIObject obj)
        {
            lock (_clickableSetLock)
            {
                if (_clickableObjects.Add(obj))
                {
                    RemakeClickableObjectList();
                }
            }
        }

        public void RemoveClickableObject(UIObject obj)
        {
            lock (_clickableSetLock)
            {
                if (_clickableObjects.Remove(obj)) 
                {
                    RemakeClickableObjectList();
                }
            }
        }

        public void RemakeClickableObjectList()
        {
            lock (_clickableObjectLock)
            {
                ClickableObjects.Clear();

                SortedDictionary<int, List<UIObject>> clickableObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_clickableSetLock)
                {
                    foreach (UIObject obj in _clickableObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!clickableObjs.ContainsKey(index))
                        {
                            clickableObjs[index] = new List<UIObject>();
                        }

                        clickableObjs[index].Add(obj);
                    }
                }

                foreach (var kvp in clickableObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach (var item in kvp.Value)
                    {
                        ClickableObjects.Add(item);
                    }
                }

                //ClickableObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion

        #region hoverable objects
        private object _hoverableSetLock = new object();
        public void AddHoverableObject(UIObject obj)
        {
            lock (_hoverableSetLock)
            {
                if (_hoverableObjects.Add(obj))
                {
                    RemakeHoverableObjectList();
                }
            }
        }

        public void RemoveHoverableObject(UIObject obj)
        {
            lock (_hoverableSetLock)
            {
                if (_hoverableObjects.Remove(obj))
                {
                    RemakeHoverableObjectList();
                }
            }
        }

        public void RemakeHoverableObjectList()
        {
            lock (_hoverableObjectLock)
            {
                HoverableObjects.Clear();

                SortedDictionary<int, List<UIObject>> hoverableObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_hoverableSetLock)
                {
                    foreach (UIObject obj in _hoverableObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!hoverableObjs.ContainsKey(index))
                        {
                            hoverableObjs[index] = new List<UIObject>();
                        }

                        hoverableObjs[index].Add(obj);
                    }
                }

                foreach(var kvp in hoverableObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach(var item in kvp.Value)
                    {
                        HoverableObjects.Add(item);
                    }
                }
                //HoverableObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion
    }
}
