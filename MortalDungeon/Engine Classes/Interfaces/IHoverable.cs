using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public interface IHoverable
    {
        public void OnTimedHover();
        public void OnHover();
        public void OnHoverEnd();
    }
}
