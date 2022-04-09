using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public interface IHoverable
    {
        public void OnTimedHover();
        public void OnHover();
        public void OnHoverEnd();
    }
}
