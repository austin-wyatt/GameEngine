using MortalDungeon.Game.Player;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "con")]
    [Serializable]
    public class Conditional : ISerializable
    {
        public static Conditional TRUE = new Conditional() { _condition = "T", Condition = "T" };
        public static Conditional FALSE = new Conditional() { _condition = "F", Condition = "F" };

        private static Dictionary<string, string> _transpiledMap = new Dictionary<string, string>()
        {
            { "_F", "Feature" },
            
            { "O", "OR" },
            { "A", "AND" },
            { "N", "NOT" },
            { "T", "T" },
            { "F", "F" },

            { "_S", "Scene" },
            { "_c", "inCombat" },

            { "_I", "Item" },
            { "_i", "inInventory" },
            { "_e", "equipped" },
            { "_gl", "goldLessThan" },
            { "_gg", "goldGreaterThan" },
            { "_ge", "goldEqual" },

            { "_Q", "Quest" },
            { "_q", "completed" },
            { "_qa", "available" },
            { "_qp", "inProgress" },

            { "_D", "Dialogue" },
            { "_d", "outcome" },
        };

        private static Dictionary<string, string> _fullToTranspiledMap = new Dictionary<string, string>();

        static Conditional()
        {
            foreach(var kvp in _transpiledMap)
            {
                _fullToTranspiledMap.TryAdd(kvp.Value, kvp.Key);
            }
        }

        public Conditional() { }
        public Conditional(Conditional conditional)
        {
            Condition = conditional.Condition;
            _condition = conditional._condition;
        }

        [XmlElement("_c")]
        public string _condition = "";

        [XmlIgnore]
        public string Condition = "";

        public string GetPlainTextCondition()
        {
            string plainTextCondition = "";

            void addPlainTextString(string text)
            {
                if (text.Length > 0)
                {
                    if (_transpiledMap.TryGetValue(text, out var plainText))
                    {
                        plainTextCondition += plainText;
                    }
                    else
                    {
                        plainTextCondition += text;
                    }
                }
            }

            string currStr = "";
            for(int i = 0; i < _condition.Length; i++)
            {
                if (_condition[i] == '(' || _condition[i] == ')' || _condition[i] == ' ')
                {
                    addPlainTextString(currStr);
                    currStr = "";

                    plainTextCondition += _condition[i];
                }
                else
                {
                    currStr += _condition[i];
                }
            }

            addPlainTextString(currStr);

            return plainTextCondition;
        }

        private string TranspileCondition()
        {
            string transpiledCondition = "";

            void addTranspiledTextString(string text)
            {
                if (text.Length > 0)
                {
                    if (_fullToTranspiledMap.TryGetValue(text, out var transpiled))
                    {
                        transpiledCondition += transpiled;
                    }
                    else
                    {
                        transpiledCondition += text;
                    }
                }
            }

            string currStr = "";
            for (int i = 0; i < Condition.Length; i++)
            {
                if (Condition[i] == '(' || Condition[i] == ')' || Condition[i] == ' ')
                {
                    addTranspiledTextString(currStr);
                    currStr = "";

                    transpiledCondition += Condition[i];
                }
                else
                {
                    currStr += Condition[i];
                }
            }

            addTranspiledTextString(currStr);

            return transpiledCondition;
        }

        public bool Check()
        {
            Stack<int> opening = new Stack<int>();

            string str = "(" + _condition + ")";

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '(')
                {
                    opening.Push(i);
                }
                else if (str[i] == ')')
                {
                    int start = opening.Pop();
                    string expression = str.Substring(start, i - start);

                    bool result = EvaluateExpression(expression);

                    str = str.Substring(0, start) + (result ? "T" : "F") + str.Substring(i + 1);
                    i -= expression.Length;
                }
            }

            return str == "T";
        }

        /// <summary>
        /// Evaluates a single, non nested, expression
        /// </summary>
        private bool EvaluateExpression(string expression)
        {
            expression = expression[1..];

            bool result = false;

            if(expression.Length == 0)
            {
                return false;
            }

            string[] blocks = expression.Split(' ');

            List<char> evaluatedBlocks = new List<char>();

            bool not = false;

            void addBool(bool result)
            {
                if (not)
                {
                    result = !result;
                    not = false;
                }

                evaluatedBlocks.Add(result ? 'T' : 'F');
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                switch (blocks[i])
                {
                    case "_E":
                        continue;
                    case "A":
                        evaluatedBlocks.Add('A');
                        break;
                    case "N":
                        not = true;
                        break;
                    case "O":
                        evaluatedBlocks.Add('O');
                        break;
                    case "T":
                        addBool(true);
                        break;
                    case "F":
                        addBool(false);
                        break;
                    case "_Q":
                        if(int.TryParse(blocks[i + 1], out int questId))
                        {
                            if (blocks[i + 2] == "_q")
                            {
                                addBool(QuestManager.GetQuestCompleted(questId));
                            }
                            else if(blocks[i + 2] == "_qp")
                            {
                                addBool(QuestManager.Quests.Exists(q => q.ID == questId));
                            }
                            else if (blocks[i + 2] == "_qa")
                            {
                                addBool(QuestManager.QuestAvailable(questId));
                            }
                        }

                        i += 2;
                        break;
                    case "_S":
                        if(blocks[i + 1] == "_c")
                        {
                            addBool(TileMapManager.Scene.InCombat);
                        }

                        i += 1;
                        break;
                    case "_I":
                        if (blocks[i + 1] == "_i")
                        {
                            if(int.TryParse(blocks[i + 2], out int itemId))
                            {
                                addBool(PlayerParty.Inventory.Items.Exists(i => i.Id == itemId));
                            }
                        }
                        else if (blocks[i + 1] == "_e")
                        {
                            if (int.TryParse(blocks[i + 2], out int itemId))
                            {
                                bool found = false;

                                foreach(var unit in PlayerParty.UnitsInParty)
                                {
                                    foreach(var item in unit.Info.Equipment.EquippedItems.Values)
                                    {
                                        if (item.Id == itemId)
                                        {
                                            found = true;
                                            addBool(true);
                                            break;
                                        }
                                    }

                                    if (found)
                                        break;
                                }

                                if(!found)
                                    addBool(false);
                            }
                        }
                        else if (blocks[i + 1] == "_gl")
                        {
                            if (int.TryParse(blocks[i + 2], out int gold))
                            {
                                addBool(PlayerParty.Inventory.Gold < gold);
                            }
                        }
                        else if (blocks[i + 1] == "_gg")
                        {
                            if (int.TryParse(blocks[i + 2], out int gold))
                            {
                                addBool(PlayerParty.Inventory.Gold > gold);
                            }
                        }
                        else if (blocks[i + 1] == "_ge")
                        {
                            if (int.TryParse(blocks[i + 2], out int gold))
                            {
                                addBool(PlayerParty.Inventory.Gold == gold);
                            }
                        }

                        i += 2;
                        break;
                    case "_D":
                        if (int.TryParse(blocks[i + 1], out int dialogueId))
                        {
                            if(blocks[i + 2] == "_d" && int.TryParse(blocks[i + 3], out int outcome))
                            {
                                addBool(DialogueLedger.GetStateValue(dialogueId, outcome));
                            }
                        }
                        i += 3;
                        break;
                }
            }

            for(int i = 0; i < evaluatedBlocks.Count; i++)
            {
                if (evaluatedBlocks[i] == 'T')
                {
                    result = true;
                }
                else if (evaluatedBlocks[i] == 'F')
                {
                    result = false;
                }
                else if (evaluatedBlocks[i] == 'A')
                {
                    result &= evaluatedBlocks[i + 1] == 'T';
                    i++;
                }
                else if (evaluatedBlocks[i] == 'O')
                {
                    result |= evaluatedBlocks[i + 1] == 'T';
                    i++;
                }
            }

            return result;
        }

        public void PrepareForSerialization()
        {
            _condition = TranspileCondition();
        }

        public void CompleteDeserialization()
        {
            Condition = GetPlainTextCondition();
        }
    }
}
