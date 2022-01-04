using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    public static class SerializerParams
    {
        public static string DATA_BASE_PATH = "Data/";
        static SerializerParams()
        {
#if DEBUG
            DATA_BASE_PATH = "Z:/repos/EngineToolsGUI/EngineToolsGUI/bin/Debug/net6.0-windows/Data/";
#endif
        }
    }
}
