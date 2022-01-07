using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Serializers
{
    public static class TextTableManager
    {
        public static Dictionary<int, TextTable> TextTables = new Dictionary<int, TextTable>();

        static TextTableManager()
        {
            LoadTextTables();
        }

        public static void LoadTextTables()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".T"))
                {
                    filesToLoad.Add(file);
                }
            }

            foreach (string file in filesToLoad)
            {
                var textTable = TextTableSerializer.LoadTextTableFromFile(file);
                TextTables.Add(textTable.TableID, textTable);
            }
        }

        public static string GetTextEntry(int tableID, int textEntryID)
        {
            if (TextTables.TryGetValue(tableID, out var table))
            {
                if (table.TryGetTextEntry(textEntryID, out var entry))
                {
                    return entry.Text;
                }
            }


            return "";
        }

        public static void SetTextEntry(int tableID, int textEntryID, string text)
        {
            if (TextTables.TryGetValue(tableID, out var table))
            {
                if (table.TryGetTextEntry(textEntryID, out var entry))
                {
                    entry.Text = text;
                }
            }
        }

        public static bool AddTextEntry(int tableID, string text, out TextEntry entry, int textEntryID = -1)
        {

            if (TextTables.TryGetValue(tableID, out var table))
            {
                if (textEntryID == -1)
                {
                    entry = table.AddTextEntry(table.GetNextAvailableID(), text);
                }
                else
                {
                    entry = table.AddTextEntry(textEntryID, text);
                }

                return true;
            }

            entry = null;
            return false;
        }

        public static bool TryGetTextTable(int tableID, out TextTable table)
        {
            if (TextTables.TryGetValue(tableID, out var t))
            {
                table = t;
                return true;
            }

            table = null;
            return false;
        }

    }
}
