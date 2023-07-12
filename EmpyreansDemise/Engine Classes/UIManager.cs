using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public class UIManager
    {
        public static FontInfo DEFAULT_FONT_INFO_64 = new FontInfo("segoeui.ttf", 64);

        public static FontInfo DEFAULT_FONT_INFO_16 = new FontInfo("segoeui.ttf", 16);
        public static FontInfo DEFAULT_FONT_INFO_8 = new FontInfo("segoeui.ttf", 8);

        public List<UIObject> TopLevelObjects = new List<UIObject>();
        public object _UILock = new object();

        //common events that benefit from storing all objects that are relevant.
        //add or remove objects to this while building the reverse trees
        public List<UIObject> ClickableObjects = new List<UIObject>();
        public HashSet<UIObject> _clickableObjects = new HashSet<UIObject>();

        public List<UIObject> HoverableObjects = new List<UIObject>();
        public HashSet<UIObject> _hoverableObjects = new HashSet<UIObject>();

        public List<UIObject> FocusableObjects = new List<UIObject>();
        public HashSet<UIObject> _focusableObjects = new HashSet<UIObject>();

        public List<UIObject> KeyDownObjects = new List<UIObject>();
        public HashSet<UIObject> _keyDownObjects = new HashSet<UIObject>();

        public List<UIObject> KeyUpObjects = new List<UIObject>();
        public HashSet<UIObject> _keyUpObjects = new HashSet<UIObject>();

        public List<UIObject> ScrollableObjects = new List<UIObject>();
        public HashSet<UIObject> _scrollableObjects = new HashSet<UIObject>();

        public object _clickableObjectLock = new object();
        public object _hoverableObjectLock = new object();
        public object _focusableObjectLock = new object();
        public object _keyDownObjectLock = new object();
        public object _keyUpObjectLock = new object();
        public object _scrollableObjectLock = new object();


        public HashSet<UIObject> ExclusiveFocusSet = new HashSet<UIObject>();
        public object _exclusiveFocusLock = new object();

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
                obj.CleanUp();
            }
        }

        public void SortUIObjects()
        {
            lock (_UILock) 
            {
                TopLevelObjects.Sort();

                //float baseZVal = 0;

                foreach (UIObject obj in TopLevelObjects)
                {
                    //obj.GenerateReverseTree(this);

                    //obj.GenerateZPositions(baseZVal);

                    //baseZVal += 0.001f;

                    GenerateReverseTree(obj, generateRenderData: false);
                }
            }

            RegenerateRenderData();
        }

        public void GenerateReverseTree(UIObject obj, bool generateRenderData = true)
        {
            int index = TopLevelObjects.IndexOf(obj);

            obj.GenerateReverseTree(this);

            const float BASE_VALUE = 0.001f;
            if (index != -1)
            {
                //obj.GenerateZPositions(0.0001f * index);
                //obj.GenerateZPositions(-1f + BASE_VALUE * index);
                obj.GenerateZPositions(BASE_VALUE * index);
                //obj.GenerateZPositions(1 * index);
            }

            obj.Update();
        }

        public void UpdateTopLevelObject(UIObject obj)
        {
            //lock (_renderGroupLock)
            //{
            //    var group = UIRenderGroups.Find(g => g.Root.ObjectID == obj.ObjectID);

            //    if (group != null)
            //    {
            //        group.GenerateGroups();
            //    }
            //    else
            //    {
            //        UIRenderGroups.Add(new UIRenderGroup(obj));
            //    }
            //}
        }

        public void RegenerateRenderData()
        {
            //lock (_renderGroupLock)
            //{
            //    UIRenderGroups.Clear();

            //    foreach (var obj in TopLevelObjects)
            //    {
            //        UpdateTopLevelObject(obj);
            //    }
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
                    obj.OnHoverEnd();

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

        #region focusable objects
        private object _focusableSetLock = new object();
        public void AddFocusableObject(UIObject obj)
        {
            lock (_focusableSetLock)
            {
                if (_focusableObjects.Add(obj))
                {
                    RemakeFocusableObjectList();
                }
            }
        }

        public void RemoveFocusableObject(UIObject obj)
        {
            lock (_focusableSetLock)
            {
                if (_focusableObjects.Remove(obj))
                {
                    obj.OnFocusEnd();

                    RemakeFocusableObjectList();
                }
            }
        }

        public void RemakeFocusableObjectList()
        {
            lock (_focusableObjectLock)
            {
                FocusableObjects.Clear();

                SortedDictionary<int, List<UIObject>> focusableObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_focusableSetLock)
                {
                    foreach (UIObject obj in _focusableObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!focusableObjs.ContainsKey(index))
                        {
                            focusableObjs[index] = new List<UIObject>();
                        }

                        focusableObjs[index].Add(obj);
                    }
                }

                foreach (var kvp in focusableObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach (var item in kvp.Value)
                    {
                        FocusableObjects.Add(item);
                    }
                }

                //FocusableObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion

        #region key down objects
        private object _keyDownSetLock = new object();
        public void AddKeyDownObject(UIObject obj)
        {
            lock (_keyDownSetLock)
            {
                if (_keyDownObjects.Add(obj))
                {
                    RemakeKeyDownObjectList();
                }
            }
        }

        public void RemoveKeyDownObject(UIObject obj)
        {
            lock (_keyDownSetLock)
            {
                if (_keyDownObjects.Remove(obj))
                {
                    RemakeKeyDownObjectList();
                }
            }
        }

        public void RemakeKeyDownObjectList()
        {
            lock (_keyDownObjectLock)
            {
                KeyDownObjects.Clear();

                SortedDictionary<int, List<UIObject>> keyDownObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_keyDownSetLock)
                {
                    foreach (UIObject obj in _keyDownObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!keyDownObjs.ContainsKey(index))
                        {
                            keyDownObjs[index] = new List<UIObject>();
                        }

                        keyDownObjs[index].Add(obj);
                    }
                }

                foreach (var kvp in keyDownObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach (var item in kvp.Value)
                    {
                        KeyDownObjects.Add(item);
                    }
                }

                //KeyDownObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion

        #region key up objects
        private object _keyUpSetLock = new object();
        public void AddKeyUpObject(UIObject obj)
        {
            lock (_keyUpSetLock)
            {
                if (_keyUpObjects.Add(obj))
                {
                    RemakeKeyUpObjectList();
                }
            }
        }

        public void RemoveKeyUpObject(UIObject obj)
        {
            lock (_keyUpSetLock)
            {
                if (_keyUpObjects.Remove(obj))
                {
                    RemakeKeyUpObjectList();
                }
            }
        }

        public void RemakeKeyUpObjectList()
        {
            lock (_keyUpObjectLock)
            {
                KeyUpObjects.Clear();

                SortedDictionary<int, List<UIObject>> keyUpObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_keyUpSetLock)
                {
                    foreach (UIObject obj in _keyUpObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!keyUpObjs.ContainsKey(index))
                        {
                            keyUpObjs[index] = new List<UIObject>();
                        }

                        keyUpObjs[index].Add(obj);
                    }
                }

                foreach (var kvp in keyUpObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach (var item in kvp.Value)
                    {
                        KeyUpObjects.Add(item);
                    }
                }

                //KeyUpObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion

        #region scrollable objects
        private object _scrollableSetLock = new object();
        public void AddScrollableObject(UIObject obj)
        {
            lock (_scrollableSetLock)
            {
                if (_scrollableObjects.Add(obj))
                {
                    RemakeScrollableObjectList();
                }
            }
        }

        public void RemoveScrollableObject(UIObject obj)
        {
            lock (_scrollableSetLock)
            {
                if (_scrollableObjects.Remove(obj))
                {
                    RemakeScrollableObjectList();
                }
            }
        }

        public void RemakeScrollableObjectList()
        {
            lock (_scrollableObjectLock)
            {
                ScrollableObjects.Clear();

                SortedDictionary<int, List<UIObject>> scrollableObjs = new SortedDictionary<int, List<UIObject>>();
                lock (_scrollableSetLock)
                {
                    foreach (UIObject obj in _scrollableObjects)
                    {
                        int index = (int)(obj.Position.Z * 1000);

                        if (!scrollableObjs.ContainsKey(index))
                        {
                            scrollableObjs[index] = new List<UIObject>();
                        }

                        scrollableObjs[index].Add(obj);
                    }
                }

                foreach (var kvp in scrollableObjs)
                {
                    kvp.Value.Sort((a, b) => a.Position.Z.CompareTo(b.Position.Z));

                    foreach (var item in kvp.Value)
                    {
                        ScrollableObjects.Add(item);
                    }
                }

                //ScrollableObjects.Sort((a, b) => b.Position.Z.CompareTo(a.Position.Z));
            }
        }
        #endregion

        #region exclusive focus
        public void ExclusiveFocusObject(UIObject obj)
        {
            void walkTree(UIObject parentObject)
            {
                foreach(var item in parentObject.Children)
                {
                    walkTree(item);
                }

                ExclusiveFocusSet.Add(parentObject);
            }

            lock (_exclusiveFocusLock)
            {
                walkTree(obj);
            }
        }

        public void ClearExclusiveFocus()
        {
            lock (_exclusiveFocusLock)
            {
                ExclusiveFocusSet.Clear();
            }
        }

        public bool ExclusiveFocusCheckObject(UIObject obj)
        {
            return ExclusiveFocusSet.Count == 0 || ExclusiveFocusSet.Contains(obj);
        }
        #endregion
    }
}
