using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            return TextCopy.ClipboardService.GetText();
        }

        public static void SetText(string str)
        {
            TextCopy.ClipboardService.SetText(str);
        }
    }
}
