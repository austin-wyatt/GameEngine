using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public enum EventSeverity 
    {
        Info,
        Caution,
        Severe,
        Positive
    }

    public class EventLog
    {
        public ScrollableArea LogArea;
        public List<UIObject> Events = new List<UIObject>();

        public float TextScale = 0.075f;

        public CombatScene Scene;

        public EventLog(CombatScene scene)
        {
            Scene = scene;

            LogArea = new ScrollableArea(default, new UIScale(1.5f, 0.26f), default, new UIScale(1.5f, 2f));
            LogArea.BaseComponent.SetColor(new Vector4(0.33f, 0.33f, 0.25f, 1));

            LogArea.Scrollbar.ScrollByPercentage(1f);

            LogArea.OnScrollAction = () =>
            {
                float eventTop = LogArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft).Y;
                float eventBot = LogArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

                for(int i = 0; i < Events.Count; i++)
                {
                    float top = Events[i].GetAnchorPosition(UIAnchorPosition.TopLeft).Y;
                    float bot = Events[i].GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

                    if(bot < eventTop || top > eventBot)
                    {
                        Events[i].SetRender(false);
                    }
                    else 
                    {
                        Events[i].SetRender(true);
                    }
                }
            };

            FeatureManager.FeatureEnter += (feature, unit) =>
            {
                Scene.SyncToRender(() =>
                {
                    string text = TextTableManager.GetTextEntry(0, feature.NameTextEntry);

                    AddEvent("Entering " + text);
                });
            };

            FeatureManager.FeatureExit += (feature, unit) =>
            {
                Scene.SyncToRender(() =>
                {
                    string text = TextTableManager.GetTextEntry(0, feature.NameTextEntry);

                    AddEvent("Leaving " + text);
                });
            };
        }

        private const int maxEventWidth = 49;
        private const int maxEvents = 30;
        public void AddEvent(string eventText, EventSeverity severity = EventSeverity.Info)
        {
            if(eventText.Length > maxEventWidth) 
            {
                eventText = WrapString(eventText, maxEventWidth);
            }

            Brush brush;


            switch (severity)
            {
                case EventSeverity.Caution:
                    brush = Brushes.LightGoldenrodYellow;
                    break;
                case EventSeverity.Severe:
                    brush = Brushes.IndianRed;
                    break;
                case EventSeverity.Positive:
                    brush = Brushes.LimeGreen;
                    break;
                default:
                    brush = Brushes.LightPink;
                    break;
            }

            Text textComponent = new Text(eventText, Text.DEFAULT_FONT, 20, brush);
            textComponent.SetTextScale(TextScale);


            //textComponent.BaseComponent.SetColor(new Vector4(1, 0, 0, 1));

            //textComponent.Hoverable = true;
            //textComponent.HoverColor = new Vector4(0.16f, 0.55f, 0.55f, 1);

            

            Events.Add(textComponent);

            

            for (int i = Events.Count - 1; i >= 0; i--)
            {
                if(i == Events.Count - 1) 
                {
                    Events[i].SetPositionFromAnchor(LogArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(5, 0, 0), UIAnchorPosition.BottomLeft);
                }
                else 
                {
                    Events[i].SetPositionFromAnchor(Events[i + 1].GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(0, 0, 0), UIAnchorPosition.BottomLeft);
                }
            }

            LogArea.Scrollbar.ScrollByPercentage(1f);

            LogArea.BaseComponent.AddChild(textComponent);

            if(Events.Count > maxEvents)
            {
                for(int i = 0;i < Events.Count - maxEvents; i++)
                {
                    LogArea.BaseComponent.RemoveChild(Events[i]);
                }
            }
        }

        public static string WrapString(string line, int maxWidth) 
        {
            if (line.Length < maxWidth)
                return line;

            string returnString = "";

            char[] testChars = new char[] { ' ', '"', '@' };

            //search for space
            bool matchFound = false;

            foreach(char c in testChars) 
            {
                if (matchFound)
                    break;

                for (int i = maxWidth; i > 1; i--)
                {
                    bool foundChar = false;

                    switch (c) 
                    {
                        case ' ':
                            foundChar = line[i] == ' ';
                            break;
                        case '"':
                            foundChar = line[i] == '"' || line[i] == '\'' || line[i] == '.' || line[i] == ',' || line[i] == '!' ||
                                        line[i] == ':' || line[i] == ';' || line[i] == '?';
                            break;
                        case '@':
                            foundChar = line[i] == '@' || line[i] == '#' || line[i] == '$' || line[i] == '%' || line[i] == '^' ||
                                        line[i] == '&' || line[i] == '*' || line[i] == '(' || line[i] == ')' || line[i] == '{' ||
                                        line[i] == '}' || line[i] == '[' || line[i] == ']' || line[i] == '/' || line[i] == '\\' ||
                                        line[i] == '|' || line[i] == '<' || line[i] == '>';
                            break;
                    }

                    if (foundChar)
                    {
                        if(c == ' ')
                        {
                            returnString = line.Substring(0, i) + "\n";
                            line = line.Substring(i + 1);
                        }
                        else
                        {
                            returnString = line.Substring(0, i + 1) + "\n";
                            line = line.Substring(i + 1);
                        }
                        

                        if (line.Length > maxWidth)
                        {
                            returnString += WrapString(line, maxWidth);
                        }
                        else
                        {
                            returnString += line;
                        }
                        matchFound = true;
                        break;
                    }
                }
            }

            if(matchFound == false) 
            {
                returnString = line.Substring(0, maxWidth) + "\n";

                line = line.Substring(maxWidth + 1);

                if (line.Length > maxWidth)
                {
                    returnString += WrapString(line, maxWidth);
                }
                else
                {
                    returnString += line;
                }
            }
            

            return returnString;
        }
    }

    
}
