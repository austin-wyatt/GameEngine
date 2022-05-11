
let LONG_MAX_VALUE = BigInt("9223372036854775807")

let Print = str => mscorlib.System.Console.WriteLine(str);


let SetStateValue = (type, id, hash, data, instruction) =>
{
    Print(LONG_MAX_VALUE)

    stateValue = new StateIDValuePair()
    stateValue.Type = type
    stateValue.StateID = BigInt(id)
    stateValue.ObjectHash = BigInt(hash)
    stateValue.Data = data
    stateValue.Instruction = instruction

    Ledgers.ApplyStateValue(stateValue)
}

let SubscribeToFeatureTrigger = (id, type, callback) =>
{
    stateValue = new StateIDValuePair()

    stateValue.Type = 1
    stateValue.StateID = BigInt(id)
    if (type == "Clear")
    {
        stateValue.ObjectHash = LONG_MAX_VALUE //cleared
    }
    else if(type == "PlayerInside")
    {
        stateValue.ObjectHash = LONG_MAX_VALUE - 200 //player inside
    }
    else if(type == "Explored")
    {
        stateValue.ObjectHash = LONG_MAX_VALUE - 26 //explored
    }
    else if(type == "Discovered")
    {
        stateValue.ObjectHash = LONG_MAX_VALUE - 25 //discovered
    }
    else if(type == "Loaded")
    {
        stateValue.ObjectHash = LONG_MAX_VALUE - 500 //loaded
    }

    stateValue.Data = 1
    stateValue.Instruction = 1 //subscribe

    subscriber = new StateSubscriber()
    subscriber.TriggerValue = stateValue
    subscriber.Script = callback

    Ledgers.AddSubscriber(subscriber)
}


let StartQuest = (questId) =>
{
    QuestLedger.StartQuest(questId);
}


let CompleteQuestObjective = (questId, stateIndex, objectiveIndex) =>
{
    QuestLedger.CompleteQuestObjective(questId, stateIndex, objectiveIndex);
}

let CQ_c = (id, callback) =>
{
    if (QuestManager.QuestAvailable(id))
    {
        eval(callback);
    }
}

let CQ = (id) =>
{
    return QuestManager.QuestAvailable(id);
}

let UnlockAbility = (treeId, abilityId, variant) =>
{
    SetStateValue(3, treeId, abilityId, variant)
}


let AddGold = (gold) =>
{
    PlayerParty.Inventory.AddGold(gold)
}