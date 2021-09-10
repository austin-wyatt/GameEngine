using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ActionQueue
    {
        private QueuedList<Action> queuedActions = new QueuedList<Action>();

        private bool Updating = false;
        private bool ShouldUpdate = false;

        public void AddAction(Action action) 
        {
            queuedActions.Add(action);
        }

        public void AddActionSingle(Action action) 
        {
            if (!queuedActions.HasQueuedItems()) 
            {
                queuedActions.Add(action);
            }
        }

        private void InvokeActions() 
        {
            queuedActions.HandleQueuedItems();

            for (int i = 0; i < queuedActions.Count; i++) 
            {
                queuedActions[i]?.Invoke();
            }

            queuedActions.Clear();
        }

        public bool UpdateInProgress() 
        {
            if (Updating) 
            {
                ShouldUpdate = true;
                return true;
            }

            return false;
        }

        public void StartUpdate() 
        {
            Updating = true;
        }

        public void EndUpdate() 
        {
            Updating = false;

            if (ShouldUpdate) 
            {
                ShouldUpdate = false;
                InvokeActions();
            }
        }
    }
}
