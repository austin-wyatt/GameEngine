
let Print = str => mscorlib.System.Console.WriteLine(str);

var Int32T = host.type("System.Int32");
var StringT = host.type("System.String");
var FloatT = host.type("System.Single");
var DoubleT = host.type("System.Double");
var LongT = host.type("System.Int64");
var BoolT = host.type("System.Boolean");

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