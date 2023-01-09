using DataObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.DataObjects
{
    public enum DataObjectType
    {
        Quest,
    }

    public static class DOHelper
    {
        /// <summary>
        /// Map an object id to a data object entry's id. 
        /// Ie the quest with id 0 maps to the data object 10000
        /// </summary>
        public static int MapIDToDO(int id, DataObjectType type)
        {
            switch (type)
            {
                case DataObjectType.Quest:
                    return id + (int)StaticDataLocations.Quests;

                default:
                    return id;
            }
        }
        
        public static Dictionary<string, object> CopyTemplate(Dictionary<string, object> template)
        {
            return DOMethods.DeepCopyDictionary(template);
        }

        /// <summary>
        /// Used for templates to save memory. Reusing the reference doesn't affect anything since
        /// we are making deep copies of each template
        /// </summary>
        private static Dictionary<string, object> _emptyDict = new Dictionary<string, object>();

        public static Dictionary<string, object> QuestSaveTemplate = new Dictionary<string, object>
        {
            { "active", _emptyDict },
            { "complete", _emptyDict },
            { "failed", _emptyDict },
            { "__ind", 0 },
        };

        public static Dictionary<string, object> ObjectiveSaveTemplate = new Dictionary<string, object>
        {
            { "index", 0 },
            { "formatStr", "" }, //ex. add a separator after, italicize text, etc
        };

        #region Base level user save info
        /// <summary>
        /// Quest information that every save file should have by default
        /// </summary>
        public static Dictionary<string, object> QUEST_SAVE_BASE = new Dictionary<string, object>
        {
            { "active", _emptyDict },
            { "complete", _emptyDict },
            { "failed", _emptyDict },
        };
        #endregion
    }
}
