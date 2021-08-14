using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public interface ITickable
    {
        public virtual void Tick() { }
    }
}
