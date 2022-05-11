using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Empyrean.Game.Serializers
{
    public static class ID_LEDGER
    {
        private static object _readWriteLock = new object();

        public static Dictionary<string, int> LedgeredIDs = new Dictionary<string, int>();

        private static bool _loaded = false;

        public static void LoadLedger()
        {
            string ledgerText;

            lock (_readWriteLock)
            {
                ledgerText = File.ReadAllText(SerializerParams.DATA_BASE_PATH + "ID_LEDGER");
            }

            ledgerText = ledgerText.Replace("\n", "").Replace("\r", "");

            var ledgerArr = ledgerText.Split(";");

            foreach(var ledger in ledgerArr)
            {
                var kvp = ledger.Split(":");

                if (kvp.Length != 2)
                    continue;

                LedgeredIDs.AddOrSet(kvp[0], int.Parse(kvp[1]));
            }

            _loaded = true;
        }

        private static void WriteDictToLedger()
        {
            string ledgerText = "";

            foreach(var kvp in LedgeredIDs)
            {
                ledgerText += $"{kvp.Key}:{kvp.Value};\n";
            }

            lock (_readWriteLock)
            {
                File.WriteAllText(SerializerParams.DATA_BASE_PATH + "ID_LEDGER", ledgerText);
            }
        }

        public static int GetNextId(string key)
        {
            if (!_loaded)
            {
                LoadLedger();
            }

            int id = 0;

            if(LedgeredIDs.TryGetValue(key, out int value))
            {
                LedgeredIDs[key]++;

                id = value;
            }
            else
            {
                id = 100;

                LedgeredIDs.AddOrSet(key, id + 1);
            }

            WriteDictToLedger();

            return id;
        }
    }
}
