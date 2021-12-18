using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class ActionQueue
    {
        private QueuedList<Action> queuedActions = new QueuedList<Action>();

        private bool Updating = false;
        private bool ShouldUpdate = false;

        internal void AddAction(Action action) 
        {
            queuedActions.Add(action);
        }

        internal void AddActionSingle(Action action) 
        {
            if (!queuedActions.HasQueuedItems()) 
            {
                queuedActions.Add(action);
            }
        }

        private bool _invokeInProgress = false;
        private void InvokeActions() 
        {
            if (_invokeInProgress) return;

            _invokeInProgress = true;
            queuedActions.Clear();

            queuedActions.HandleQueuedItems();

            int count = queuedActions.Count - 1;
            for (int i = count; i >= 0; i--) 
            {
                queuedActions[i]?.Invoke();
            }

            _invokeInProgress = false;
        }

        internal bool UpdateInProgress() 
        {
            if (Updating) 
            {
                ShouldUpdate = true;
                return true;
            }

            return false;
        }

        internal void StartUpdate() 
        {
            Updating = true;
        }

        internal void EndUpdate() 
        {
            if (ShouldUpdate)
            {
                InvokeActions();
                ShouldUpdate = false;
            }

            Updating = false;
        }
    }
}
