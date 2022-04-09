using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class RenderDispatcher
    {
        private Dictionary<object, Action> _actionsToDispatch = new Dictionary<object, Action>();
        private object _dispatchLock = new object();
        public void DispatchAction(object source, Action action)
        {
            lock (_dispatchLock)
            {
                if(_actionsToDispatch.TryAdd(source, action))
                {
                    Window.QueueToRenderCycle(BatchActions);
                }
            }
        }

        private void BatchActions()
        {
            lock (_dispatchLock)
            {
                foreach (var item in _actionsToDispatch)
                {
                    item.Value.Invoke();
                }

                _actionsToDispatch.Clear();
            }
        }
    }
}
