using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public interface ITickable
    {
        public virtual void Tick() { }
    }
}
