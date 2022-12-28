using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Serializers
{
    public interface ISerializable
    {
        public void PrepareForSerialization();
        public void CompleteDeserialization();
    }

    public static class SerializerParams
    {
        public static string DATA_BASE_PATH = "Data/";
        public static string SAVE_BASE_PATH = "Save/";
        static SerializerParams()
        {
//#if DEBUG
            DATA_BASE_PATH = "Z:/repos/EngineToolsGUI/EngineToolsGUI/bin/Debug/net6.0-windows/Data/";
//#endif
        }
    }
}
