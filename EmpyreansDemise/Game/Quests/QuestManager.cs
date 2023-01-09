using System;
using System.Collections.Generic;
using System.Text;
using DataObject;
using DataObjects;
using Empyrean.Engine_Classes;
using Empyrean.Game.DataObjects;
using Empyrean.Game.Scripting;
using Empyrean.Game.UI;

namespace Empyrean.Game.Quests
{
    public enum GenericStatus
    {
        None = 0,
        Active = 1,
        Complete = 2,
        Failed = 4,
        Delete = 8,
        Available = 16
    }

    public static class QuestManager
    {
        public const string USER_QUEST_PATH = ":user.15";

        private static Dictionary<GenericStatus, string> _statusNames = new Dictionary<GenericStatus, string>
        {
            { GenericStatus.None, "" },
            { GenericStatus.Active, "active" },
            { GenericStatus.Complete, "complete" },
            { GenericStatus.Failed, "failed" },
            { GenericStatus.Delete, "" },
        };


        public static void StartQuest(int questId)
        {
            StartQuest(questId.ToString());
        }

        /// <summary>
        /// Places a quest into the "active" category of the user save data and 
        /// evaluates the startScript of the quest. <para/>
        /// No validation is done in this method beyond removing the quest information
        /// from other save categories.
        /// </summary>
        public static void StartQuest(string searchString)
        {
            DataSearchRequest request = new DataSearchRequest(searchString);
            int questId = request.ObjectId;
            request.ObjectId = DOHelper.MapIDToDO(request.ObjectId, DataObjectType.Quest);

            if (request.GetEntry(out DataObjectEntry questEntry))
            {
                //First check if this quest id exists in either the completed or failed tabs
                //if it does, remove it
                RemoveQuestFromSavedCategories(questId, GenericStatus.Complete | GenericStatus.Failed);

                //Then check to see if it exists in active quests. If it does, return
                //otherwise add the QuestSaveTemplate at that id and run the start script
                string questSavePath = "~quest.active." + questId;
                DataSearchRequest questSearchRequest = new DataSearchRequest(questSavePath);

                if (questSearchRequest.Exists())
                    return;

                //Set the quest save template for the quest
                questSearchRequest.GetOrCreateKey().SetValue(DOHelper.CopyTemplate(DOHelper.QuestSaveTemplate));

                if (questEntry.Parent.ContainsKey("startScript"))
                {
                    //within the startScript field, the first objective should be added
                    JSManager.EvaluateScript<object>((string)questEntry.Parent["startScript"], questEntry.Parent);
                }

                Console.WriteLine("Starting quest " + questId);
            }
            else
            {
                throw new Exception("Quest does not exist: " + searchString);
            }
        }

        public static void UpdateQuestInfo(int questId, GenericStatus status)
        {
            const string questBasePath = "~quest";

            string questIdString = questId.ToString();

            DataObjectEntry baseEntry = new DataSearchRequest(questBasePath).GetEntry();

            DataObjectEntry tempEntry;

            foreach (var kvp in _statusNames)
            {
                tempEntry = baseEntry.GetSubEntry(kvp.Value);
                if(tempEntry != null)
                {
                    DataObjectEntry questEntry = tempEntry.GetSubEntry(questIdString);

                    if(questEntry != null && kvp.Key != status)
                    {
                        baseEntry.GetSubEntry(_statusNames[status]).SetSubValue(questIdString, questEntry.GetValue());
                        baseEntry.EntryModified();

                        if(status == GenericStatus.Complete)
                        {
                            //finish the quest and evaluate the endScript of the quest
                            ResolveQuestEnd(questId, "endScript");
                            Console.WriteLine("Finished quest " + questId);
                        }
                        else if(status == GenericStatus.Failed)
                        {
                            ResolveQuestEnd(questId, "failScript");
                            Console.WriteLine("Failed quest " + questId);
                        }

                        questEntry.DeleteEntry();
                        return;
                    }
                    else if (questEntry != null)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Changes the status of the objective between active, complete, and failed. Delete can also be passed
        /// as the status to remove the objective from the save data. <para/>
        /// The index represents the order that the objectives will be drawn in the quest log. If no index is passed
        /// it will be determined by an internal increment value in the objective save info. <para/>
        /// This function is only intended to be used for active quests. Inactive quests should not have their objectives
        /// updated.
        /// </summary>
        public static void UpdateObjectiveInfo(int questId, string objectiveName, GenericStatus status, int index = int.MaxValue)
        {
            string questSearchString = "~quest.active." + questId;

            const string indexStr = "__ind";
            const string objectiveIndexStr = "index";

            DataObjectEntry baseEntry = new DataSearchRequest(questSearchString).GetEntry();

            DataObjectEntry indexEntry = baseEntry.GetSubEntry(indexStr);

            DataObjectEntry tempEntry;
            GenericStatus foundCategory = GenericStatus.None;

            //Attempt to move the entry if it already exists elsewhere
            //We make an assumption that the objective will only exist in one category at a time
            foreach(var kvp in _statusNames)
            {
                tempEntry = baseEntry.GetSubEntry(kvp.Value, objectiveName);
                if (tempEntry != null)
                {
                    foundCategory = kvp.Key;

                    if (foundCategory == GenericStatus.Delete)
                    {
                        tempEntry.DeleteEntry();
                    }
                    else if (foundCategory != status)
                    {
                        var newEntry = new DataSearchRequest(questSearchString + "." + _statusNames[status]).GetOrCreateKey();
                        newEntry.SetSubValue(objectiveName, DOMethods.DeepCopyDictionary((Dictionary<string, object>)tempEntry.GetValue()));

                        newEntry.EntryModified();
                        tempEntry.DeleteEntry();

                        Console.WriteLine("Changed status of objective " + objectiveName + " from " + kvp.Value + " to " + _statusNames[status]);

                        if (index != int.MaxValue)
                        {
                            newEntry.GetSubEntry(objectiveName, objectiveIndexStr).SetValue(index);
                        }
                        return;
                    }
                    else if (foundCategory == status)
                        return;
                }
            }

            //create the entry if it wasn't found to already exist
            if(foundCategory == GenericStatus.None)
            {
                var newEntry = new DataSearchRequest(questSearchString + "." + _statusNames[status]).GetOrCreateKey();
                newEntry.SetSubValue(objectiveName, DOMethods.DeepCopyDictionary(DOHelper.ObjectiveSaveTemplate));
                newEntry.EntryModified();

                if (index == int.MaxValue)
                {
                    index = Convert.ToInt32(indexEntry.GetValue());
                    indexEntry.SetValue(index + 1);
                }

                newEntry.GetSubEntry(objectiveName, objectiveIndexStr).SetValue(index);
            }
        }

        /// <summary>
        /// Activate a quest's objective. <para/>
        /// </summary>
        public static void ActivateObjective(int questId, string objectiveName, int index = int.MaxValue)
        {
            DataSearchRequest request;

            request = new DataSearchRequest(questId.ToString() + "." + objectiveName);
            request.ObjectId = DOHelper.MapIDToDO(request.ObjectId, DataObjectType.Quest);

            DataObjectEntry objectiveEntry = request.GetEntry();
            if(objectiveEntry != null)
            {
                UpdateObjectiveInfo(questId, objectiveName, GenericStatus.Active, index);
                string activationScript = (string)objectiveEntry.GetSubEntry("activationScript").GetValue();
                JSManager.EvaluateScript<object>(activationScript, (Dictionary<string, object>)objectiveEntry.GetValue());
            }
        }

        public static void CompleteObjective(int questId, string objectiveName, int index = int.MaxValue)
        {
            DataSearchRequest request;

            request = new DataSearchRequest(questId.ToString() + "." + objectiveName);
            request.ObjectId = DOHelper.MapIDToDO(request.ObjectId, DataObjectType.Quest);

            DataObjectEntry objectiveEntry = request.GetEntry();
            if (objectiveEntry != null)
            {
                UpdateObjectiveInfo(questId, objectiveName, GenericStatus.Complete, index);
                string endScript = (string)objectiveEntry.GetSubEntry("endScript").GetValue();
                JSManager.EvaluateScript<object>(endScript, (Dictionary<string, object>)objectiveEntry.GetValue());
            }
        }

        private static void ResolveQuestEnd(int id, string scriptName)
        {   
            DataSearchRequest request = new DataSearchRequest(id.ToString());
            request.ObjectId = DOHelper.MapIDToDO(request.ObjectId, DataObjectType.Quest);

            if (request.GetEntry(out DataObjectEntry questEntry))
            {
                DataObjectEntry endScriptEntry = questEntry.GetSubEntry(scriptName);
                if(endScriptEntry != null)
                {
                    string script = (string)endScriptEntry.GetValue();
                    JSManager.EvaluateScript<object>(script);
                }
            }
        }

        /// <summary>
        /// Removes the quest informtion from the supplied flag categories if applicable
        /// </summary>
        private static void RemoveQuestFromSavedCategories(int id, GenericStatus questCategoriesFlag)
        {
            DataSearchRequest searchRequest;
            DataObjectEntry baseEntry = new DataSearchRequest("~quest").GetEntry();

            DataObjectEntry entry;

            if ((questCategoriesFlag & GenericStatus.Complete) != GenericStatus.None)
            {
                entry = baseEntry.GetSubEntry("complete", id.ToString());
                if(entry != null)
                {
                    entry.DeleteEntry();
                }
            }
            
            if((questCategoriesFlag & GenericStatus.Failed) != GenericStatus.None)
            {
                entry = baseEntry.GetSubEntry("failed", id.ToString());
                if (entry != null)
                {
                    entry.DeleteEntry();
                }
            }

            if ((questCategoriesFlag & GenericStatus.Active) != GenericStatus.None)
            {
                entry = baseEntry.GetSubEntry("active", id.ToString());
                if (entry != null)
                {
                    entry.DeleteEntry();
                }
            }
        }
    }
}
