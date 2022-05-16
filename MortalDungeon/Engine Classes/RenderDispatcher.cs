using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes
{
    public class RenderDispatcher
    {
        private Dictionary<object, Action> _actionsToDispatch = new Dictionary<object, Action>();
        private object _dispatchLock = new object();
        bool _batched = false;

        public void DispatchAction(object source, Action action)
        {
            if(Thread.CurrentThread.ManagedThreadId != WindowConstants.MainThreadId)
            {
                lock (_dispatchLock)
                {
                    if (_actionsToDispatch.TryAdd(source, action) && !_batched)
                    {
                        Window.QueueToRenderCycle(BatchActions);
                        _batched = true;
                    }
                }
            }
            else
            {
                if (_actionsToDispatch.TryAdd(source, action) && !_batched)
                {
                    Window.QueueToRenderCycle(BatchActions);
                    _batched = true;
                }
            }
        }

        private void BatchActions()
        {
            lock (_dispatchLock)
            {
                _batched = false;
                foreach (var item in _actionsToDispatch)
                {
                    item.Value.Invoke();
                }

                _actionsToDispatch.Clear();
            }
        }
    }
}
