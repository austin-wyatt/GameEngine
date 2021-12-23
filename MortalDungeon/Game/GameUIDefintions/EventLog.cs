using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
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

        public float TextScale = 0.04f;

        public EventLog()
        {
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
        }

        private const int maxEventWidth = 49;
        private const int maxEvents = 30;
        public void AddEvent(string eventText, EventSeverity severity = EventSeverity.Info)
        {
            if(eventText.Length > maxEventWidth) 
            {
                eventText = WrapString(eventText, maxEventWidth);
            }

            TextComponent textComponent = new TextComponent();
            textComponent.SetTextScale(TextScale);
            textComponent.SetText(eventText);

            switch (severity)
            {
                case EventSeverity.Info:
                    textComponent.SetColor(new Vector4(0.72f, 0.4f, 0.8f, 1));
                    break;
                case EventSeverity.Caution:
                    textComponent.SetColor(new Vector4(0.75f, 0.75f, 0, 1));
                    break;
                case EventSeverity.Severe:
                    textComponent.SetColor(new Vector4(0.58f, 0.16f, 0f, 1));
                    break;
                case EventSeverity.Positive:
                    textComponent.SetColor(new Vector4(0.04f, 0.52f, 0.13f, 1));
                    break;
            }

            textComponent._textField.SetScissorData(LogArea.BaseComponent.ScissorData);

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
