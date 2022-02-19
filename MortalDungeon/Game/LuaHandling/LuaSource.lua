import('MortalDungeon', 'MortalDungeon.Game')
import('MortalDungeon', 'MortalDungeon.Game.Serializers')
import('MortalDungeon', 'MortalDungeon.Game.Ledger')
LONG_MAX_VALUE = 9223372036854775807
--LedgerUpdateTypes
--Dialogue = 0,
--Feature = 1,
--Quest = 2,
--GeneralState = 3,
--Unit = 4

--StateValue fields
--Type
--StateID
--ObjectHash
--Data
--Instruction (set, subscribe, clear, permanent subscriber)
--Values

--StateSubscriber
--TriggerValue
--SubscribedValues (applies on trigger)
--Script
--Permanent

function SetStateValue(type, id, hash, data, instruction)
    stateValue = StateIDValuePair()
    stateValue.Type = type
    stateValue.StateID = id
    stateValue.ObjectHash = hash
    stateValue.Data = data
    stateValue.Instruction = instruction

    Ledgers.ApplyStateValue(stateValue)
end


function SubscribeToFeatureTrigger(id, type, callback)
    stateValue = StateIDValuePair()

    stateValue.Type = 1
    stateValue.StateID = id
    if(type == "Clear") then
        stateValue.ObjectHash = LONG_MAX_VALUE -- cleared
    elseif(type == "PlayerInside") then
        stateValue.ObjectHash = LONG_MAX_VALUE - 200 -- player inside
    elseif(type == "Explored") then
        stateValue.ObjectHash = LONG_MAX_VALUE - 26 -- explored
    elseif(type == "Discovered") then
        stateValue.ObjectHash = LONG_MAX_VALUE - 25 -- discovered
    elseif(type == "Loaded") then
        stateValue.ObjectHash = LONG_MAX_VALUE - 500 -- loaded
    end

    stateValue.Data = 1
    stateValue.Instruction = 1 --subscribe

    subscriber = StateSubscriber()
    subscriber.TriggerValue = stateValue
    subscriber.Script = callback

    Ledgers.AddSubscriber(subscriber)
end

function StartQuest(questId)
    QuestLedger.StartQuest(questId)
end

function CompleteQuestObjective(questId, stateIndex, objectiveIndex)
    QuestLedger.CompleteQuestObjective(questId, stateIndex, objectiveIndex)
end

--checks if the quest with the passed id is available to start
function CQ(id, callback)
    if(QuestManager.QuestAvailable(id)) then
        callback()
    end
end

--Sets the state value associated with a given ability to the maximum unlocked variant. (variants for ability tree nodes are essentially upgraded versions of the ability)
function UnlockAbility(treeId, abilityId, variant)
    SetStateValue(3, treeId, abilityId, variant)
end


-- sandboxing
import = function() end