using System;
using System.Collections.Generic;
using System.Text;
using DataObjects;

namespace Empyrean.Game.DataObjects
{
    public static class DataManagerInitializer
    {
        private const int USER_BLOCK_SIZE = 1000;
        private const int DATA_BLOCK_SIZE = 1000;

        public static void Initialize(string saveName) 
        {
            string path = Serializers.SerializerParams.SAVE_BASE_PATH + "\\" + saveName;

            WriteDataBlockManager userSource = new WriteDataBlockManager(USER_BLOCK_SIZE, "savedata_", path);
            WriteDataBlockManager staticDataSource = new WriteDataBlockManager(DATA_BLOCK_SIZE, "data_", 
                Serializers.SerializerParams.DATA_BASE_PATH, writeEnabled: false);

            DataSourceManager.AddDataSource(userSource, "user");
            DataSourceManager.AddDataSource(staticDataSource, "static");
        }
    }
}
