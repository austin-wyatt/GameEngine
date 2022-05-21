using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    public static class ConditionParser
    {
        private enum DataSource
        {
            CastingUnit,
            TargetUnit,
            Scene,
            AbilityEffect,
        }

        #region Unit
        private enum UnitCategory
        {
            ResF,
            ResI,
            Dead,
            BuffEffect,
            StatusCondition,
            UnitCondition,
            Species,
            UnitContext,
            Team,
            BaseTeam,
            VisibleBy
        }

        private static float EvaluateUnitSource(Unit unit, string[] elements, AbilityEffectResults effectResults)
        {
            UnitCategory category = (UnitCategory)Enum.Parse(typeof(UnitCategory), elements[1]);

            switch (category)
            {
                case UnitCategory.ResF:
                    return unit.GetResF((ResF)Enum.Parse(typeof(ResF), elements[2]));
                case UnitCategory.ResI:
                    return unit.GetResI((ResI)Enum.Parse(typeof(ResI), elements[2]));
                case UnitCategory.Dead:
                    return unit.Info.Dead ? 1 : 0;
                case UnitCategory.BuffEffect:
                    return unit.Info.BuffManager.GetValue((BuffEffect)Enum.Parse(typeof(BuffEffect), elements[2]));
                case UnitCategory.StatusCondition:
                    return unit.Info.StatusManager.CheckCondition((StatusCondition)Enum.Parse(typeof(StatusCondition), elements[2])) ? 1 : 0;
                case UnitCategory.UnitCondition:
                    return unit.Info.StatusManager.CheckCondition((UnitCondition)Enum.Parse(typeof(UnitCondition), elements[2])) ? 1 : 0;
                case UnitCategory.Species:
                    return (float)unit.Info.Species;
                case UnitCategory.UnitContext:
                    return unit.Info.Context.GetFlag((UnitContext)Enum.Parse(typeof(UnitContext), elements[2])) ? 1 : 0;
                case UnitCategory.Team:
                    return (float)unit.AI.GetTeam();
                case UnitCategory.BaseTeam:
                    return (float)unit.AI.Team;
                case UnitCategory.VisibleBy:
                    return unit.Info.Visible((UnitTeam)float.Parse(elements[2])) ? 1 : 0;
            }
            return float.MinValue;
        }

        #endregion

        #region Ability effect
        private static float EvaluateAbilityEffectSource(AbilityEffectResults effectResults, string[] elements)
        {
            AbilityEffectResult category = (AbilityEffectResult)Enum.Parse(typeof(AbilityEffectResult), elements[1]);

            if(effectResults.ResultValues.TryGetValue(category, out var result))
            {
                return result;
            }

            return float.MinValue;
        }
        #endregion

        /// <summary>
        /// Evaluates a scope that references game data
        /// </summary>
        private static float EvaluateDataScope(string condition, AbilityEffectResults effectResults)
        {
            string[] elements = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            float data = float.MinValue;

            DataSource dataSource;

            int selectedIndex = 0;

            int bracketIndex = elements[0].IndexOf("[");
            if (bracketIndex > -1)
            {
                string indexStr = "";

                for(int i = bracketIndex + 1; i < elements[0].Length; i++)
                {
                    if(elements[0][i] != ']')
                    {
                        indexStr += elements[0][i];
                    }
                    else
                    {
                        break;
                    }
                }

                selectedIndex = int.Parse(indexStr);

                dataSource = (DataSource)Enum.Parse(typeof(DataSource), elements[0].Substring(0, bracketIndex));
            }
            else
            {
                dataSource = (DataSource)Enum.Parse(typeof(DataSource), elements[0]);
            }

            

            switch (dataSource)
            {
                case DataSource.CastingUnit:
                    return EvaluateUnitSource(effectResults.Ability.CastingUnit, elements, effectResults);
                case DataSource.TargetUnit:
                    return EvaluateUnitSource(effectResults.Ability.SelectionInfo.SelectedUnits[selectedIndex], elements, effectResults);
                case DataSource.Scene:
                    break;
                case DataSource.AbilityEffect:
                    return EvaluateAbilityEffectSource(effectResults, elements);
            }

            return data;
        }

        private enum ConditionOperations
        {
            NotAnOperation,
            True,
            False,
            And,
            Or,
            Not,
            LessThan,
            GreaterThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
            Equal,
            NotEqual,
            Add,
            Subtract,
            Multiply,
            Divide,
            Number
        }

        private static ConditionOperations EvaluateBoolExpression(ref string[] elements, int elementIndex, AbilityEffectResults effectResults)
        {
            string expression = elements[elementIndex];

            switch (expression)
            {
                case "T":
                    return ConditionOperations.True;
                case "F":
                    return ConditionOperations.False;
                case "&&":
                    return ConditionOperations.And;
                case "||":
                    return ConditionOperations.Or;
                case "!":
                    return ConditionOperations.Not;
                case "<":
                    return ConditionOperations.LessThan;
                case "<=":
                    return ConditionOperations.LessThanOrEqual;
                case ">":
                    return ConditionOperations.GreaterThan;
                case ">=":
                    return ConditionOperations.GreaterThanOrEqual;
                case "==":
                    return ConditionOperations.Equal;
                case "~=":
                    return ConditionOperations.NotEqual;
                case "*":
                    return ConditionOperations.Multiply;
                case "/":
                    return ConditionOperations.Divide;
                case "+":
                    return ConditionOperations.Add;
                case "-":
                    return ConditionOperations.Subtract;
            }

            return ConditionOperations.NotAnOperation;
        }

        /// <summary>
        /// Returns true if the operation is true, false otherwise
        /// </summary>
        private static bool GetBool(this ConditionOperations operations)
        {
            return operations == ConditionOperations.True;
        }

        private static ConditionOperations GetOperationFromBool(bool value)
        {
            return value ? ConditionOperations.True : ConditionOperations.False;
        }

        /// <summary>
        /// Evaluates a scope as a boolean expression
        /// </summary>
        private static bool EvaluateBoolScope(string condition, AbilityEffectResults effectResults, out float data)
        {
            //match spaces, ! && and ||
            string[] elements = Regex.Split(condition, @"(!=| +|!|&{2}|\|{2}|={2}|>=|<=|>|<|\+|-|\*|/)");
            //string[] elements = Regex.Split(condition, @"(!=| +|={2}|>|<|<=|>=)");

            List<ConditionOperations> operations = new List<ConditionOperations>();

            //the data array will contain either an evaluated float number or 0
            List<float> dataArray = new List<float>();


            bool invert = false;
            for(int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == "" || elements[i][0] == ' ') 
                    continue;

                ConditionOperations operation = EvaluateBoolExpression(ref elements, i, effectResults);

                if(operation == ConditionOperations.NotAnOperation)
                {
                    if(float.TryParse(elements[i], out float value))
                    {
                        dataArray.Add(value);
                    }
                    else
                    {
                        dataArray.Add(0);
                    }

                    operations.Add(ConditionOperations.Number);
                    continue;
                }

                if (invert)
                {
                    operation = GetOperationFromBool(!GetBool(operation));
                    invert = false;
                }

                if (operation == ConditionOperations.Not)
                {
                    invert = true;
                    continue;
                }

                dataArray.Add(0);
                operations.Add(operation);
            }

            //evalute operations
            bool result = true;

            float currData = float.MinValue;

            //i represents the "write head" of the evaluation and result represents the current evaluation

            for (int i = 0; i < operations.Count; i++)
            {
                switch (operations[i])
                {
                    case ConditionOperations.True:
                        if (i == 0) 
                            result = true;
                        break;
                    case ConditionOperations.False:
                        if (i == 0)
                            result = false;
                        break;
                    case ConditionOperations.Or:
                        result |= operations[i + 1].GetBool();
                        i++;
                        break;
                    case ConditionOperations.And:
                        result &= operations[i + 1].GetBool();
                        i++;
                        break;

                    case ConditionOperations.Number:
                        currData = dataArray[i];
                        break;
                    case ConditionOperations.Subtract:
                        currData -= dataArray[i + 1];
                        i++; //skip the next number declaration
                        break;
                    case ConditionOperations.Add:
                        currData += dataArray[i + 1];
                        i++;
                        break;
                    case ConditionOperations.Multiply:
                        currData *= dataArray[i + 1];
                        i++;
                        break;
                    case ConditionOperations.Divide:
                        currData /= dataArray[i + 1];
                        i++;
                        break;

                    case ConditionOperations.GreaterThan:
                        result = currData > dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                    case ConditionOperations.LessThan:
                        result = currData < dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                    case ConditionOperations.GreaterThanOrEqual:
                        result = currData >= dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                    case ConditionOperations.LessThanOrEqual:
                        result = currData <= dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                    case ConditionOperations.Equal:
                        result = currData == dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                    case ConditionOperations.NotEqual:
                        result = currData != dataArray[i + 1];
                        i++;

                        currData = float.MinValue;
                        break;
                }
            }

            data = currData;

            return result;
        }

        public static bool ParseCondition(string condition, AbilityEffectResults effectResults)
        {
            Stack<int> scopeIndices = new Stack<int>();

            Stack<int> dataExpressionIndices = new Stack<int>();

            StringBuilder stringBuilder = new StringBuilder();

            condition = "(" + condition + ")";

            for (int i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '(')
                    scopeIndices.Push(i);
                else if(condition[i] == '{')
                    dataExpressionIndices.Push(i);


                if (condition[i] == '}')
                {
                    int startOfScope = dataExpressionIndices.Pop();

                    float data = EvaluateDataScope(condition.Substring(startOfScope + 1, i - (startOfScope + 1)), effectResults);


                    stringBuilder.Append(condition.AsSpan(0, startOfScope));

                    if (data != float.MinValue)
                    {
                        stringBuilder.Append(data);
                    }

                    if (i != condition.Length - 1)
                    {
                        stringBuilder.Append(condition.AsSpan(i + 1));
                    }

                    condition = stringBuilder.ToString();

                    i = startOfScope;

                    stringBuilder.Clear();
                }
                else if (condition[i] == ')')
                {
                    int startOfScope = scopeIndices.Pop();

                    bool statementResult = EvaluateBoolScope(condition.Substring(startOfScope + 1, i - (startOfScope + 1)), effectResults, out float data);

                    
                    stringBuilder.Append(condition.AsSpan(0, startOfScope));

                    if (data != float.MinValue)
                    {
                        stringBuilder.Append(data);
                    }
                    else
                    {
                        stringBuilder.Append(statementResult ? "T" : "F");
                    }
                    
                    if(i != condition.Length - 1)
                    {
                        stringBuilder.Append(condition.AsSpan(i + 1));
                    }

                    condition = stringBuilder.ToString();

                    i = startOfScope;

                    stringBuilder.Clear();
                }
            }


            return condition == "T";
        }
    }

    public class ChainCondition
    {
        //take in any data from the scene/ability/unit/effect completion information and determine whether to 
        //continue the chain further

        public AbilityEffect ChainedEffect;

        public string Condition;

        public ChainCondition(string condition)
        {
            Condition = condition;
        }

        public async Task ContinueEffect(AbilityEffectResults effectResults, CombinedAbilityEffectResults combinedResults)
        {
            if (ChainedEffect != null && CheckCondition(effectResults))
            {
                await ChainedEffect.EnactEffect(effectResults.Ability, combinedResults);
            }
        }

        public virtual bool CheckCondition(AbilityEffectResults effectResults)
        {
            return ConditionParser.ParseCondition(Condition, effectResults);
        }
    }
}
