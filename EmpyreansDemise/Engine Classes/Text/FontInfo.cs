using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class FontInfo
    {
        public string FontName;
        public string FontPath;
        public int FontSize;
        public bool IsLocalPath = false;

        public string GetName()
        {
            return FontName + "_" + FontSize;
        }
    }
}
