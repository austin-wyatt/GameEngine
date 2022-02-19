using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    public static class QuestManager
    {
        public static List<Quest> Quests = new List<Quest>();
        public static List<Quest> CompletedQuests = new List<Quest>();

        public static object _questLock = new object();

        public static void StartQuest(int id)
        {
            bool questCompleted = QuestLedger.GetStateValue(id, (int)QuestStates.Completed);

            if (!questCompleted && !Quests.Exists(q => q.ID == id))
            {
                Quest quest = QuestBlockManager.GetQuest(id);

                if (quest != null)
                {
                    lock (_questLock)
                    {
                        Quests.Add(quest);

                        quest.AddCurrentStateObjectivesToSubscriber();
                    }

                    Console.WriteLine($"Quest {quest.ID} has been started");
                }
            }
            else 
            {
                if (questCompleted)
                {
                    Console.WriteLine($"Quest {id} has already been completed");
                }
            }
        }

        public static bool QuestAvailable(int id)
        {
            bool questCompleted = QuestLedger.GetStateValue(id, (int)QuestStates.Completed);

            if (!questCompleted && !Quests.Exists(q => q.ID == id))
            {
                return true;
            }

            return false;
        }

        public static void CompleteQuest(Quest quest)
        {
            bool questCompleted = QuestLedger.GetStateValue(quest.ID, (int)QuestStates.Completed);

            if (questCompleted)
                return; //if the quest has already been completed then don't allow the event to be sent again

            //give completion reward for completing quest

            Console.WriteLine($"Quest {quest.ID} has been completed");

            quest.QuestReward.ApplyRewards();

            StateIDValuePair completeQuestStateValue = new StateIDValuePair()
            {
                Type = (int)LedgerUpdateType.Quest,
                StateID = quest.ID,
                ObjectHash = 0,
                Data = (int)QuestStates.Completed,
            };

            Ledgers.ApplyStateValue(completeQuestStateValue);

            lock (_questLock)
            {
                CompletedQuests.Add(quest);
                Quests.Remove(quest);
            }
        }

        public static bool GetQuestCompleted(int questId)
        {
            return QuestLedger.GetStateValue(questId, (int)QuestStates.Completed);
        }
    }
}
