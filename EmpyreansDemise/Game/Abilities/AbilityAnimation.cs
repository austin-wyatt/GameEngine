using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    /// <summary>
    /// Includes any visual effects/sounds inherent to an ability
    /// </summary>
    public class AbilityAnimation
    {
        public TaskCompletionSource<bool> TaskHandle;

        /// <summary>
        /// TaskHandle must be resolved from this action.
        /// </summary>
        public Action AnimAction;

        public async Task PlayAnimation()
        {
            TaskHandle = new TaskCompletionSource<bool>();

            if(AnimAction == null)
            {
                return;
            }
            else
            {
                AnimAction.Invoke();
            }

            await TaskHandle.Task;
        }
    }
}
