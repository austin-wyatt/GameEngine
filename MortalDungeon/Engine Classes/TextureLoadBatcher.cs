using MortalDungeon.Engine_Classes.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public static class TextureLoadBatcher
    {
        private static List<GameObject> ObjectsToLoad = new List<GameObject>(100);

        private static bool _processQueued = false;

        private static object _loadLock = new object();
        public static void LoadTexture(GameObject obj)
        {
            lock (_loadLock)
            {
                ObjectsToLoad.Add(obj);

                if (!_processQueued)
                {
                    QueueProcessing();
                }
            }
        }

        private static void QueueProcessing()
        {
            _processQueued = true;
            Window.RenderEnd += ProcessObjects;
        }


        private static void ProcessObjects()
        {
            Window.RenderEnd -= ProcessObjects;
            lock (_loadLock)
            {
                _processQueued = false;

                for(int i = 0; i < ObjectsToLoad.Count; i++)
                {
                    Renderer.LoadTextureFromGameObj(ObjectsToLoad[i]);
                }

                ObjectsToLoad.Clear();
            }
        }
    }
}
