using Empyrean.Game.Items;
using Empyrean.Game.Player;
using Empyrean.Game.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{

    [Serializable] 
    public class QuestReward : ISerializable
    {
        public List<ItemEntry> ItemRewards = new List<ItemEntry>();
        public int GoldReward = 0;

        public List<string> Scripts = new List<string>();

        public QuestReward() { }

        public void ApplyRewards()
        {
            if(GoldReward > 0)
            {
                PlayerParty.Inventory.AddGold(GoldReward);
            }

            foreach(var entry in ItemRewards)
            {
                var item = entry.GetItemFromEntry();
                PlayerParty.Inventory.AddItemToInventory(item);
            }

            foreach(var script in Scripts)
            {
                JSManager.ApplyScript(script);
            }
        }

        public void PrepareForSerialization()
        {

        }

        public void CompleteDeserialization()
        {

        }
    }

    [Serializable]
    public class QuestUnlockInfo
    {
        [XmlElement("QUIid")]
        public int QuestID;
        [XmlElement("QUIud")]
        public TextInfo QuestUnlockDescription;
    }
}
