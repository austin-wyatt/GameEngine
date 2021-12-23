using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Quests
{
    public static class QuestManager
    {
        public static List<Quest> Quests = new List<Quest>();

        public static object _questLock = new object();

        public static void StartQuest(int id)
        {
            bool questCompleted = QuestLedger.GetStateValue(id, (int)QuestStates.Completed);

            if (!questCompleted && !Quests.Exists(q => q.ID == id))
            {
                Quest quest = QuestSerializer.LoadQuestFromFile(id);

                if (quest != null)
                {
                    lock (_questLock)
                    {
                        Quests.Add(quest);
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

        public static void CompleteQuest(Quest quest)
        {
            bool questCompleted = QuestLedger.GetStateValue(quest.ID, (int)QuestStates.Completed);

            if (questCompleted)
                return; //if the quest has already been completed then don't allow the event to be sent again

            //give completion reward for completing quest

            Console.WriteLine($"Quest {quest.ID} has been completed");

            QuestLedger.ModifyStateValue(quest.ID, (int)QuestStates.Completed);

            lock (_questLock)
            {
                Quests.Remove(quest);
            }
        }
    }
}
