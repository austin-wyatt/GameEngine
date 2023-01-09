using System;
using System.Collections.Generic;
using System.Text;
using DataObjects;

namespace Empyrean.Game.DataObjects
{
    public enum SaveDataLocations 
    {
        Quest = 15,
        Location = 16,
        Unit = 20,
    }

    /// <summary>
    /// Blocks of data where certain datasets are expected to lie. These demarcations are for development purpose only
    /// as the game will simply pull the data by id
    /// </summary>
    public enum StaticDataLocations 
    {
        //10000 block allotments for each set of definitions. Further block extensions can be created later if necessary

        //0-10000 block reserved just in case
        Quests = 10000, 
    }

    public static class DataManagerInitializer
    {
        //ALIASES
        //~unit = :user.20.unit
        //~loc = :user.20.loc

        //GENERAL IDS
        //:user.20 represents persistent user data

        public static bool Initialized { get; private set; }
        
        private const int USER_BLOCK_SIZE = 1000;
        private const int DATA_BLOCK_SIZE = 1000;
        private const int SETTINGS_BLOCK_SIZE = 10;

        public static void Initialize(string saveName) 
        {
            Initialized = true;

            string path = Serializers.SerializerParams.SAVE_BASE_PATH + saveName + "/";

            WriteDataBlockManager userSource = new WriteDataBlockManager(USER_BLOCK_SIZE, "savedata_", path);
            WriteDataBlockManager settingSource = new WriteDataBlockManager(SETTINGS_BLOCK_SIZE, "settings_", 
                Serializers.SerializerParams.DATA_BASE_PATH);

#if DEBUG
            WriteDataBlockManager staticDataSource = new WriteDataBlockManager(DATA_BLOCK_SIZE, "data_", 
                Serializers.SerializerParams.DATA_BASE_PATH, writeEnabled: true);
#else
            WriteDataBlockManager staticDataSource = new WriteDataBlockManager(DATA_BLOCK_SIZE, "data_", 
                Serializers.SerializerParams.DATA_BASE_PATH, writeEnabled: false);
#endif

            DataSourceManager.AddDataSource(userSource, "user");
            DataSourceManager.AddDataSource(staticDataSource, "static");
            DataSourceManager.AddDataSource(settingSource, "settings");

            DataSourceManager.PathAliases.Add("~unit", ":user.20");
            DataSourceManager.PathAliases.Add("~loc", ":user.16");

            //The ~quest path stores which quests and objectives are active, completed, and failed
            DataSourceManager.PathAliases.Add("~quest", Quests.QuestManager.USER_QUEST_PATH);
            //The ~questdata path stores data that is relevant to specific quests,
            //generally set by various objective logger actions
            DataSourceManager.PathAliases.Add("~questdata", ":user.1000");

            DataSourceManager.PathAliases.Add("~settings", ":settings.0");

            InitializeSave();
        }

        /// <summary>
        /// Should be called every time a save is loaded/created to ensure the expected scaffolding is available
        /// </summary>
        private static void InitializeSave()
        {
            //Quests
            DataSearchRequest questSearch = new DataSearchRequest("~quest");
            if (!questSearch.Exists())
            {
                questSearch.GetOrCreateEntry(out var questEntry, null);
                questEntry.SetValue(DOHelper.CopyTemplate(DOHelper.QUEST_SAVE_BASE));
            }
        }
    }
}
