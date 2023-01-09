using Empyrean.Engine_Classes.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Engine_Classes
{
    public static class TextureLoadBatcher
    {
        private static List<GameObject> ObjectsToLoad = new List<GameObject>(100);
        private static List<SimpleTexture> SimpleTexturesToLoad = new List<SimpleTexture>(100);

        private static List<AsyncSignal> _waitHandles = new List<AsyncSignal>();

        public static AsyncSignal ProcessWaitHandle = new AsyncSignal();

        private static bool _processQueued = false;

        private static object _loadLock = new object();
        public static void LoadTexture(GameObject obj, AsyncSignal signal = null)
        {
            lock (_loadLock)
            {
                ObjectsToLoad.Add(obj);

                if (!_processQueued)
                {
                    QueueProcessing();
                }

                if(signal != null)
                {
                    _waitHandles.Add(signal);
                }
            }
        }

        public static void LoadTexture(SimpleTexture obj, AsyncSignal signal = null)
        {
            if (WindowConstants.InMainThread(Thread.CurrentThread))
            {
                Renderer.LoadTextureFromSimple(obj);
                signal?.Set();
                return;
            }

            lock (_loadLock)
            {
                SimpleTexturesToLoad.Add(obj);

                if (!_processQueued)
                {
                    QueueProcessing();
                }

                if (signal != null)
                {
                    _waitHandles.Add(signal);
                }
            }
        }

        private static void QueueProcessing()
        {
            _processQueued = true;
            Window.QueueToRenderCycle(ProcessObjects);
        }


        private static void ProcessObjects()
        {
            lock (_loadLock)
            {
                _processQueued = false;

                for (int i = 0; i < ObjectsToLoad.Count; i++)
                {
                    Renderer.LoadTextureFromGameObj(ObjectsToLoad[i]);
                }

                for(int i = 0; i < SimpleTexturesToLoad.Count; i++)
                {
                    Renderer.LoadTextureFromSimple(SimpleTexturesToLoad[i]);
                }

                for (int i = 0; i < _waitHandles.Count; i++)
                {
                    _waitHandles[i].Set();
                }
                ProcessWaitHandle.Set();

                ObjectsToLoad.Clear();
                SimpleTexturesToLoad.Clear();
                _waitHandles.Clear();

                ProcessWaitHandle.Reset();
            }
        }
    }
}
