using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes
{
    public class AsyncSignal
    {
        private ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);

        public void Set()
        {
            _waitHandle.Set();
        }

        public void Dispose()
        {
            _waitHandle.Dispose();
        }

        public void Wait()
        {
            _waitHandle.Wait();
        }

        public void WaitAndDispose()
        {
            _waitHandle.Wait();
            _waitHandle.Dispose();
        }

        public void Reset()
        {
            _waitHandle.Reset();
        }
    }
}
