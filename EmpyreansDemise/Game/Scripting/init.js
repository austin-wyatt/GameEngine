
let Print = str => mscorlib.System.Console.WriteLine(str);

var Int32T = host.type("System.Int32");
var StringT = host.type("System.String");
var FloatT = host.type("System.Single");
var DoubleT = host.type("System.Double");
var LongT = host.type("System.Int64");
var BoolT = host.type("System.Boolean");
var CharT = host.type("System.Char");

var INT_MAX_VALUE = Math.pow(2, 31) - 1;

let cast = (type, obj) => {
    return host.cast(type, obj);
}

let DO = (searchString) => {
    let searchRequest = new DataSearchRequest(searchString);

    var foundValue = host.newVar(DataObjectEntry);

    if (searchRequest.GetEntry(foundValue.out)) {
        var entryVal = host.newVar(object);
        if (foundValue.TryGetValue(entryVal.out))
            return entryVal;
    }

    return null;
}

let SetDO = (searchString, value) => {
    let searchRequest = new DataSearchRequest(searchString);

    var foundValue = host.newVar(DataObjectEntry);

    if (searchRequest.GetOrCreateEntry(foundValue.out, value)) {
        foundValue.SetValue(value);
    }
}

let AddGold = (gold) =>
{
    PlayerParty.Inventory.AddGold(gold)
}

let SaveSource = (src) =>
{
    DataSourceManager.GetSource(src).SaveAllPendingBlocks();
}

let SaveSources = () =>
{
    DataSourceManager.SaveSources();
}



let GetD = (dict, key) =>
{
    var foundValue = host.newVar(object);
    if (cast(DictT, dict).TryGetValue(key, foundValue.out))
    {
        return foundValue;
    }

    return null;
}

//QUESTS
let StartQuest = (searchString) =>
{
    QuestManager.StartQuest(searchString);
}

let UpdateQuestInfo = (questId, status) =>
{
    QuestManager.UpdateQuestInfo(questId, status);
}

let UpdateObjectiveInfo = (questId, objectiveName, status, index = INT_MAX_VALUE) =>
{
    if (index == INT_MAX_VALUE)
    {
        QuestManager.UpdateObjectiveInfo(questId, objectiveName, status);
    }
    else
    {
        QuestManager.UpdateObjectiveInfo(questId, objectiveName, status, index);
    }
}

let ActivateObjective = (questId, objectiveName, index = INT_MAX_VALUE) =>
{
    if (index == INT_MAX_VALUE)
    {
        QuestManager.ActivateObjective(questId, objectiveName);
    }
    else
    {
        QuestManager.ActivateObjective(questId, objectiveName, index);
    }
}

let CompleteObjective = (questId, objectiveName, index = INT_MAX_VALUE) =>
{
    if (index == INT_MAX_VALUE) {
        QuestManager.CompleteObjective(questId, objectiveName);
    }
    else {
        QuestManager.CompleteObjective(questId, objectiveName, index);
    }
}

let RemoveLoggerAction = (loggerActionDict) =>
{
    LoggerActionManager.RemoveLoggerAction(loggerActionDict);
}

let AddLoggerAction = (loggerActionDict) =>
{
    LoggerActionManager.AddLoggerAction(loggerActionDict);
}


//let TestDPI = () =>
//{
//    var horizontal = host.newVar(FloatT);
//    var vertical = host.newVar(FloatT);

//    WindowConstants.CurrentWindow.TryGetCurrentMonitorDpiRaw(horizontal.out, vertical.out);

//    Print(horizontal.ToString() + ", " + vertical.ToString());
//}

let SetText = (val) =>
{
    Window._TEST_STRING.SetText(val);
}

let SetTextScale = (x, y) =>
{
    Window._TEST_STRING.SetTextScale(cast(FloatT, x), cast(FloatT, y));
}

let ST = () =>
{
    Window._TEST_STRING.SetTextScale(cast(FloatT, 1), cast(FloatT, 1));
}

let SetTextColor = (x, y, z, w) =>
{
    Window._TEST_STRING.SetTextColor(cast(FloatT, x), cast(FloatT, y), cast(FloatT, z), cast(FloatT, w));
}