using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Engine_Classes
{
    public static class CalculationThread
    {
        private static Queue<Action> _calculationActions = new Queue<Action>();
        private static object _calculationLock = new object();

        public static void Initialize()
        {
            Task.Run(CalculationLoop);
        }

        public static void AddCalculation(Action action)
        {
            lock (_calculationLock)
            {
                _calculationActions.Enqueue(action);
            }
        }

        private static void CalculationLoop()
        {
            while (true)
            {
                lock (_calculationLock)
                {
                    while (_calculationActions.Count > 0)
                    {
                        _calculationActions.Dequeue().Invoke();
                    }
                }

                Thread.Sleep(2);
            }
        }
    }
}
