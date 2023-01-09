using System;
using System.Collections.Generic;
using System.Text;
using DataObjects;

namespace Empyrean.Game.Logger
{
    public static class LoggerHub
    {
        public static void ProcessPacket(LoggerPacket packet)
        {
            Window.QueueToRenderCycle(() => _processPacket(packet));
        }

        private static void _processPacket(LoggerPacket packet)
        {
            if (!packet.TryGetValue("id", out object id))
                return;

            string packetType;

            if(id.GetType() == typeof(LoggerEventType))
            {
                LoggerEventType standardType = (LoggerEventType)id;

                //update standard :user values based packet information
                switch (standardType)
                {
                    case LoggerEventType.UnitKilled:

                    default:
                        break;
                }

                packetType = standardType.ToString();
            }
            else
            {
                packetType = (string)id;
            }

            LoggerActionManager.ProcessLoggerActions(packet, packetType);
        }


        public static object GetKey(string keyPath) 
        {
            DataSearchRequest search = new DataSearchRequest(keyPath);

            search.Search(out object key);
            return key;
        }
        public static bool GetKeyExists(string keyPath) 
        {
            DataSearchRequest search = new DataSearchRequest(keyPath);

            return search.Search(out object _);
        }
        public static bool TryGetKey(string keyPath, out object key) 
        {
            DataSearchRequest search = new DataSearchRequest(keyPath);

            return search.Search(out key);
        }

        public static void SetKey(string keyPath, object value) 
        {
            if (TryGetEntry(keyPath, out DataObjectEntry entry))
            {
                entry.SetValue(value);
            }
        }

        public static bool TrySetKey(string keyPath, object value) 
        {
            DataSearchRequest search = new DataSearchRequest(keyPath);

            if (TryGetEntry(keyPath, out DataObjectEntry entry))
            {
                entry.SetValue(value);
                return true;
            }
            else
            {
                return search.GetOrCreateEntry(out _, value);
            }
        }

        public static bool TryGetEntry(string keyPath, out DataObjectEntry entry) 
        {
            DataSearchRequest search = new DataSearchRequest(keyPath);

            return search.GetEntry(out entry);
        }
    }
}
