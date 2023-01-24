using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.Text;
using Empyrean.Engine_Classes.TextHandling;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Empyrean.Game.UI
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
        public List<TextString> Events = new List<TextString>();

        public CombatScene Scene;

        public EventLog(CombatScene scene)
        {
            Scene = scene;

            LogArea = new ScrollableArea(default, new UIScale(0.75f, 0.5f), default, new UIScale(0.75f, 2f),
                scrollbarWidth: 0.05f, scrollSide: ScrollbarSide.Left);

            LogArea.BaseComponent.SetColor(new Vector4(0.33f, 0.33f, 0.25f, 0.5f));

            LogArea.Scrollbar.ScrollByPercentage(1f);

            LogArea.OnScrollAction = () =>
            {
                float visAreaTop = LogArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopLeft).Y;
                float visAreaBot = LogArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

                for(int i = 0; i < Events.Count; i++)
                {
                    float top = Events[i].GetAnchorPosition(UIAnchorPosition.TopLeft).Y;
                    float bot = Events[i].GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

                    //if(bot < visAreaTop || top > visAreaBot)
                    //{
                    //    Events[i].SetRender(false);
                    //}
                    //else 
                    //{
                    //    Events[i].SetRender(true);
                    //}
                }
            };

            FeatureManager.FeatureEnter += (feature, unit) =>
            {
                string text = TextEntry.GetTextEntry(feature.NameTextEntry).ToString();

                AddEvent("Entering " + text);
            };

            FeatureManager.FeatureExit += (feature, unit) =>
            {
                string text = TextEntry.GetTextEntry(feature.NameTextEntry).ToString();

                AddEvent("Leaving " + text);
            };
        }

        private const int maxEventWidth = 35;
        private const int maxEvents = 30;
        public void AddEvent(string eventText, EventSeverity severity = EventSeverity.Info)
        {
            if(eventText.Length > maxEventWidth) 
            {
                eventText = UIHelpers.WrapString(eventText, maxEventWidth);
            }

            Vector4 color;

            switch (severity)
            {
                case EventSeverity.Caution:
                    color = new Vector4(250f / 256, 250f / 256, 210f / 256, 1);
                    break;
                case EventSeverity.Severe:
                    color = new Vector4(153f / 256, 64f / 256, 35f / 256, 1);
                    break;
                case EventSeverity.Positive:
                    color = new Vector4(28f / 256, 249f / 256, 19f / 256, 1);
                    break;
                default:
                    color = new Vector4(248f / 256, 136f / 256, 209f / 256, 1);
                    break;
            }

            TextString textComponent = new TextString(new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 12))
            {
                TextColor = color
            };
            textComponent.SetText(eventText);

            //textComponent.Hoverable = true;
            //textComponent.HoverColor = new Vector4(0.16f, 0.55f, 0.55f, 1);


            Events.Add(textComponent);

            Vector3 botLeft = LogArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft);
            for (int i = Events.Count - 1; i >= 0; i--)
            {
                if (i == Events.Count - 1)
                {
                    Events[i].SetPositionFromAnchor(botLeft + new Vector3(5, -15, 0), UIAnchorPosition.BottomLeft);
                }
                else
                {
                    Events[i].SetPositionFromAnchor(Events[i + 1].GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(0, 0, 0), UIAnchorPosition.BottomLeft);
                }
            }

            LogArea.BaseComponent.AddTextString(textComponent);

            LogArea.Scrollbar.ScrollByPercentage(1f);

            if(Events.Count > maxEvents)
            {
                for(int i = 0;i < Events.Count - maxEvents; i++)
                {
                    LogArea.BaseComponent.RemoveTextString(Events[i]);
                }
            }
        }
    }

    
}
