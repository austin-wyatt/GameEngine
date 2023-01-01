using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Scripting
{
    public enum ScriptTokenType
    {
        None = -1,
        Packet,
        Local,
        GetDataObject,
        SetDataObject,
        Cast,
        SetDataObjectDynamic
    }

    public class ScriptTokenDict
    {
        public Dictionary<char, ScriptTokenDict> Tokens = new Dictionary<char, ScriptTokenDict>();

        public ScriptTokenDefinition Outlet;
    }

    public class ScriptTokenDefinition
    {
        public string Begin;
        public string End;

        public ScriptTokenType Type;

        /// <summary>
        /// If true, the token accepts context data in its closing identifier
        /// </summary>
        public bool CheckEnd = false;
    }

    public class ScriptToken
    {
        public ScriptTokenType Type = ScriptTokenType.None;

        /// <summary>
        /// The beginning of the token starting after the token indentifier
        /// </summary>
        public int BeginIndex;

        /// <summary>
        /// The length to the end of the token not including the token identifier
        /// </summary>
        public int Length;

        public string FormattedString;

        public List<ScriptToken> RequiredTokens = new List<ScriptToken>();

        public int BaseBegin { get { return BeginIndex - ScriptFormat.BEGIN_TOKENS[(int)Base.Type].Length; } }
        public int BaseEnd { get { return BeginIndex + Length + ScriptFormat.END_TOKENS[(int)Base.Type].Length; } }

        public ScriptTokenDefinition Base;
    }

    public static class ScriptFormat
    {
        public static ScriptTokenDict TokenDict = new ScriptTokenDict();

        static ScriptFormat()
        {
            FillTokenDict();
        }

        private static void FillTokenDict()
        {
            ScriptTokenDict tempDict;

            for (int i = 0; i < BEGIN_TOKENS.Length; i++)
            {
                ScriptTokenDict dict = TokenDict;

                for (int j = 0; j < BEGIN_TOKENS[i].Length; j++)
                {
                    char c = BEGIN_TOKENS[i][j];

                    if (dict.Tokens.TryGetValue(c, out tempDict))
                    {
                        dict = tempDict;
                    }
                    else
                    {
                        tempDict = new ScriptTokenDict();
                        dict.Tokens.Add(c, tempDict);

                        dict = tempDict;
                    }

                    if (j == BEGIN_TOKENS[i].Length - 1)
                    {
                        dict.Outlet = new ScriptTokenDefinition()
                        {
                            Begin = BEGIN_TOKENS[i],
                            End = END_TOKENS[i],
                            Type = TOKEN_TYPES[i],
                            CheckEnd = END_TOKENS[i][^1] == '*'
                        };
                    }
                }
            }
        }

        public static string FormatString(ReadOnlySpan<char> str, Dictionary<string, object> packet,
            Dictionary<string, object> loggerAction, ref HashSet<string> exposedObjects)
        {
            List<ScriptToken> tokens = WalkString(str);

            //fill required tokens
            List<ScriptToken> parentlessTokens = new List<ScriptToken>(tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                ScriptToken token = tokens[i];

                for (int j = 0; j < parentlessTokens.Count; j++)
                {
                    ScriptToken parentlessToken = parentlessTokens[j];

                    if (parentlessToken.BaseBegin > token.BaseBegin && parentlessToken.BaseEnd < token.BaseEnd)
                    {
                        token.RequiredTokens.Add(parentlessToken);
                        parentlessTokens.Remove(parentlessToken);
                        j--;
                    }
                }

                parentlessTokens.Add(token);
            }

            int begin = 0;
            int end;

            StringBuilder finalString = new StringBuilder(str.Length * 2);

            for (int i = 0; i < parentlessTokens.Count; i++)
            {
                ScriptToken token = parentlessTokens[i];

                FormatToken(str, token, packet, loggerAction, ref exposedObjects);

                end = token.BaseBegin - begin;

                finalString.Append(str.Slice(begin, end));

                finalString.Append(token.FormattedString);

                begin = token.BaseEnd;
            }

            end = str.Length - begin;

            finalString.Append(str.Slice(begin, end));

            return finalString.ToString();
        }

        /// <summary>
        /// Returns a queue of script tokens in the order they should be resolved to avoid conflicts
        /// </summary>
        public static List<ScriptToken> WalkString(ReadOnlySpan<char> str)
        {
            Stack<ScriptToken> unresolvedTokens = new Stack<ScriptToken>();
            List<ScriptToken> resolvedTokens = new List<ScriptToken>();

            Queue<ScriptTokenDict> currQueue = new Queue<ScriptTokenDict>();
            Queue<ScriptTokenDict> newQueue = new Queue<ScriptTokenDict>();
            Queue<ScriptTokenDict> temp;

            currQueue.Enqueue(TokenDict);

            for (int i = 0; i < str.Length; i++)
            {
                //we must always resolve the most recently added token first
                if (unresolvedTokens.Count > 0)
                {
                    ScriptToken token = unresolvedTokens.Peek();

                    int len = END_TOKENS[(int)token.Type].Length;
                    if (str.Length >= i + len && CheckForEndToken(str.Slice(i, len), token.Type))
                    {
                        token.Length = i - token.BeginIndex;

                        resolvedTokens.Add(token);
                        unresolvedTokens.Pop();
                    }
                }

                while (currQueue.Count > 0)
                {
                    ScriptTokenDict tempDict = currQueue.Dequeue();

                    if (tempDict.Outlet != null)
                    {
                        unresolvedTokens.Push(new ScriptToken()
                        {
                            Base = tempDict.Outlet,
                            BeginIndex = i,
                            Type = tempDict.Outlet.Type
                        });

                        currQueue.Clear();
                    }

                    if (tempDict.Tokens.TryGetValue(str[i], out ScriptTokenDict newDict))
                    {
                        newQueue.Enqueue(newDict);
                    }
                }

                currQueue.Enqueue(TokenDict);

                temp = currQueue;
                currQueue = newQueue;
                newQueue = temp;
            }

            return resolvedTokens;
        }

        public static void FormatToken(ReadOnlySpan<char> str, ScriptToken token, Dictionary<string, object> packet,
            Dictionary<string, object> loggerAction, ref HashSet<string> exposedObjects)
        {
            int begin = token.BeginIndex;
            int end;

            //allocate a minimum buffer of 2 times the string's length to hopefully avoid some reallocations when appending and replacing
            StringBuilder unformattedString = new StringBuilder(str.Length * 2);

            for (int i = 0; i < token.RequiredTokens.Count; i++)
            {
                ScriptToken requiredToken = token.RequiredTokens[i];

                FormatToken(str, requiredToken, packet, loggerAction, ref exposedObjects);

                end = requiredToken.BaseBegin - begin;

                unformattedString.Append(str.Slice(begin, end));

                unformattedString.Append("^" + i + "^");

                begin = requiredToken.BaseEnd;
            }

            end = token.BeginIndex + token.Length - begin;

            unformattedString.Append(str.Slice(begin, end));

            string formatSubstr;
            object value;

            //format here
            switch (token.Type)
            {
                case ScriptTokenType.Local:
                    if (loggerAction.TryGetValue(unformattedString.ToString(), out value))
                    {
                        unformattedString.Insert(0, "L_");
                        formatSubstr = unformattedString.ToString();

                        JSManager.ExposeObject(formatSubstr, value);
                        exposedObjects.Add(formatSubstr);
                    }
                    break;
                case ScriptTokenType.Packet:
                    formatSubstr = unformattedString.ToString();
                    if (packet.TryGetValue(formatSubstr, out value))
                    {
                        unformattedString.Insert(0, "P_");
                        formatSubstr = unformattedString.ToString();

                        JSManager.ExposeObject(formatSubstr, value);
                        exposedObjects.Add(formatSubstr);
                    }
                    break;
                case ScriptTokenType.GetDataObject:
                    //GetDataObject supports casting based on the wildcard value at the end of the token
                    switch (str[token.BaseEnd - 1])
                    {
                        case 'I':
                            unformattedString.Insert(0, "host.cast(Int32T, DO('");
                            unformattedString.Append("')?? 0)");
                            break;
                        case 'D':
                            unformattedString.Insert(0, "host.cast(DoubleT, DO('");
                            unformattedString.Append("')?? 0)");
                            break;
                        case 'F':
                            unformattedString.Insert(0, "host.cast(FloatT, DO('");
                            unformattedString.Append("')?? 0)");
                            break;
                        case 'B':
                            unformattedString.Insert(0, "host.cast(BoolT, DO('");
                            unformattedString.Append("')?? false)");
                            break;
                        case 'S':
                            //unformattedString.Insert(0, "host.cast(StringT,");
                            //unformattedString.Append("'))");
                            unformattedString.Insert(0, "DO('");
                            unformattedString.Append("').ToString()");
                            break;
                        case 'L':
                            unformattedString.Insert(0, "host.cast(LongT, DO('");
                            unformattedString.Append("')?? 0)");
                            break;
                        default:
                            unformattedString.Insert(0, "DO('");
                            unformattedString.Append("')");
                            break;
                    }
                    break;
                case ScriptTokenType.Cast:
                    switch (str[token.BaseEnd - 1])
                    {
                        case 'I':
                            unformattedString.Insert(0, "host.cast(Int32T,");
                            unformattedString.Append("?? 0)");
                            break;
                        case 'D':
                            unformattedString.Insert(0, "host.cast(DoubleT,");
                            unformattedString.Append("?? 0)");
                            break;
                        case 'F':
                            unformattedString.Insert(0, "host.cast(FloatT,");
                            unformattedString.Append("?? 0)");
                            break;
                        case 'B':
                            unformattedString.Insert(0, "host.cast(BoolT,");
                            unformattedString.Append("?? false)");
                            break;
                        case 'S':
                            //unformattedString.Insert(0, "host.cast(StringT,");
                            //unformattedString.Append(")");
                            unformattedString.Insert(0, "DO('");
                            unformattedString.Append("').ToString()");
                            break;
                        case 'L':
                            unformattedString.Insert(0, "host.cast(LongT,");
                            unformattedString.Append("?? 0)");
                            break;
                    }
                    break;
                case ScriptTokenType.SetDataObject:
                case ScriptTokenType.SetDataObjectDynamic:
                    formatSubstr = unformattedString.ToString();

                    int equalIndex = formatSubstr.IndexOf('=');

                    ReadOnlySpan<char> doStr = formatSubstr.AsSpan(0, equalIndex);
                    doStr = doStr.TrimEnd();

                    unformattedString.Remove(0, equalIndex + 1);

                    if (token.Type == ScriptTokenType.SetDataObjectDynamic)
                        unformattedString.Insert(0, "SetDO(" + doStr.ToString() + ",");
                    else
                        unformattedString.Insert(0, "SetDO('" + doStr.ToString() + "',");
                    unformattedString.Append(");");
                    break;
            }

            //Swap in formatted strings for placeholder values
            for (int i = 0; i < token.RequiredTokens.Count; i++)
            {
                unformattedString.Replace("^" + i + "^", token.RequiredTokens[i].FormattedString);
            }

            token.FormattedString = unformattedString.ToString();
        }

        public static bool CheckForEndToken(ReadOnlySpan<char> str, ScriptTokenType type)
        {
            int endLen = END_TOKENS[(int)type].Length;

            if (str.Length < endLen)
                return false;

            for (int i = 0; i < endLen; i++)
            {
                bool equal = str[i] == END_TOKENS[(int)type][i];
                //wildcard can't be the start character
                bool wildcard = END_TOKENS[(int)type][i] == '*'
                    && str[i] != END_TOKENS[(int)type][0]
                    && !_forbiddenWildcardChars.Contains(str[i])
                    && !char.IsNumber(str[i]);
                bool firstIndex = i == 0;

                if ((!equal && firstIndex) || (!equal && !wildcard))
                    return false;
            }

            return true;
        }

        public static readonly string[] BEGIN_TOKENS = new string[]
        {
            "!<", //packet
            "@<", //local
            "@@", //get data object
            "$$", //set data object
            "%%", //cast expression
            "$!", //set data object dynamic
        };

        public static readonly string[] END_TOKENS = new string[]
        {
            ">!", //packet
            ">@", //local
            "@*", //get data object
            ";", //set data object
            "%*", //cast expression
            ";", //set data object dynamic
        };

        public static readonly ScriptTokenType[] TOKEN_TYPES = new ScriptTokenType[]
        {
            ScriptTokenType.Packet,
            ScriptTokenType.Local,
            ScriptTokenType.GetDataObject,
            ScriptTokenType.SetDataObject,
            ScriptTokenType.Cast,
            ScriptTokenType.SetDataObjectDynamic
        };

        private static HashSet<char> _forbiddenWildcardChars = new HashSet<char>()
        {
            ' ',
            ';',
            '(',
            ')',
            '[',
            ']',
            '{',
            '}'
        };
    }
}
