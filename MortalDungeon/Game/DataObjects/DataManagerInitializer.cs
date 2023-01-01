using System;
using System.Collections.Generic;
using System.Text;
using DataObjects;

namespace Empyrean.Game.DataObjects
{
    public static class DataManagerInitializer
    {
        //ALIASES
        //~unit = :user.20.unit
        //~loc = :user.20.loc

        //GENERAL IDS
        //:user.20 represents persistent user data
        
        private const int USER_BLOCK_SIZE = 1000;
        private const int DATA_BLOCK_SIZE = 1000;
        private const int SETTINGS_BLOCK_SIZE = 10;

        public static void Initialize(string saveName) 
        {
            string path = Serializers.SerializerParams.SAVE_BASE_PATH + "\\" + saveName;

            WriteDataBlockManager userSource = new WriteDataBlockManager(USER_BLOCK_SIZE, "savedata_", path);
            WriteDataBlockManager settingSource = new WriteDataBlockManager(SETTINGS_BLOCK_SIZE, "settings_", path);

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

            DataSourceManager.PathAliases.Add("~unit", ":user.20.unit");
            DataSourceManager.PathAliases.Add("~loc", ":user.20.loc");
            DataSourceManager.PathAliases.Add("~settings", ":settings.0");
        }
    }
}
