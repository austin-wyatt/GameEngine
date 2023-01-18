using System;
using System.Collections.Generic;
using System.Text;
using DataObject;
using DataObjects;
using Empyrean.Game.Logger;
using Empyrean.Game.Scripting;

namespace Empyrean.Game.Serializers
{
    public struct TextId
    {
        public int Id;

        public TextId(int id)
        {
            Id = id;
        }
    }

    public class TextEntry
    {
        private struct FormatEntry 
        {
            public string Specifier;
            public string Script;
            public Func<string> GetStringFunc;

            public FormatEntry(string specifier, string script)
            {
                Specifier = specifier;
                Script = script;
                GetStringFunc = null;
            }

            public FormatEntry(Func<string> getStringFunc)
            {
                GetStringFunc = getStringFunc;
                Specifier = "";
                Script = "";
            }

            public override string ToString()
            {
                if(Script == "")
                {
                    return GetStringFunc();
                }

                return JSManager.EvaluateScript<object>(Script).ToString();
            }
        }

        #region static methods and constructor
        private static WriteDataBlockManager _textSource;

        private static Dictionary<int, WeakReference<TextEntry>> LoadedTextEntries = new Dictionary<int, WeakReference<TextEntry>>();

        static TextEntry() 
        {
            _textSource = DataSourceManager.GetSource("text");
        }

        /// <summary>
        /// Gets a text entry corresponding to the passed id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="getUnique">True if a new text entry should be created regardless of it being loaded already, 
        /// false if it can be reused. <para/>
        /// An example of a reusable string is one that tracks a quest objective. <para/>
        /// A non reusable string would be one used to calculate the damage of an item. </param>
        public static TextEntry GetTextEntry(int id, bool getUnique = false)
        {
            if(!getUnique && LoadedTextEntries.TryGetValue(id, out var entry))
            {
                if(entry.TryGetTarget(out var target))
                {
                    return target;
                }
                else
                {
                    LoadedTextEntries.Remove(id);
                }
            }

            if (_textSource.GetDataObject(id, out Dictionary<string, object> data))
            {
                TextEntry newEntry = new TextEntry(data) { Id = id };

                if (!getUnique)
                    LoadedTextEntries.Add(id, new WeakReference<TextEntry>(newEntry));

                return newEntry;
            }

            return EMPTY_ENTRY;
        }

        public static TextEntry EMPTY_ENTRY = new TextEntry() { Text = "PLACEHOLDER TEXT" };

        private static string[] _newLineStr = { Environment.NewLine };
        #endregion

        public string Text;
        public int Id;

        private List<string> _triggers = new List<string>();
        private List<FormatEntry> _formatEntries = new List<FormatEntry>();
        private string[] _scriptResults = new string[0];

        private TextEntry() { }
        private TextEntry(Dictionary<string, object> dataObject)
        {
            const string formatStr = "format";
            const string triggerStr = "trigger";
            const string textStr = "text";

            Dictionary<string, object> formatDict;

            if (dataObject.TryGetValue(formatStr, out var dict))
            {
                formatDict = dict as Dictionary<string, object>;
                if(formatDict != null)
                {
                    foreach(var kvp in formatDict)
                    {
                        _formatEntries.Add(new FormatEntry(kvp.Key, (string)kvp.Value));
                    }
                }
            }

            _scriptResults = new string[_formatEntries.Count];

            if (dataObject.TryGetValue(triggerStr, out object str))
            {
                string[] unformattedTriggers = ((string)str).Split(_newLineStr, StringSplitOptions.None);

                for(int i = 0; i < unformattedTriggers.Length; i++)
                {
                    _triggers.Add(DOMethods.ResolveFullSearchPath(unformattedTriggers[i]));
                }
            }

            Text = (string)dataObject[textStr];

            CreateLoggerActions();
        }

        ~TextEntry()
        {
            DestroyLoggerActions();
        }

        private List<Dictionary<string, object>> _loggerActions = new List<Dictionary<string, object>>();
        private void CreateLoggerActions()
        {
            const string _type = "type";
            const string _callback = "callback";

            Action triggerActivated = OnTriggerActivated;

            for (int i = 0; i < _triggers.Count; i++)
            {
                Dictionary<string, object> action = new Dictionary<string, object>
                {
                    { _type,  _triggers[i] },
                    { _callback,  triggerActivated}
                };

                _loggerActions.Add(action);
                LoggerActionManager.AddLoggerAction(action);
            }
        }

        private void DestroyLoggerActions()
        {
            foreach(var action in _loggerActions)
            {
                LoggerActionManager.RemoveLoggerAction(action);
            }
        }

        public void AddFunctionFormatString(Func<string> formatString, int insertIndex)
        {
            FormatEntry newEntry = new FormatEntry(formatString);

            _formatEntries.Insert(insertIndex, newEntry);

            if(_scriptResults.Length < _formatEntries.Count)
            {
                string[] temp = new string[_formatEntries.Count];
                _scriptResults.CopyTo(temp, 0);
                _scriptResults = temp;
            }
        }

        public override string ToString()
        {
            for(int i = 0; i < _formatEntries.Count; i++)
            {
                _scriptResults[i] = _formatEntries[i].ToString();
            }

            return string.Format(Text, _scriptResults);
        }

        public event EventHandler TriggerActivated;

        public void OnTriggerActivated()
        {
            TriggerActivated?.Invoke(this, EventArgs.Empty);
        }
    }
}
