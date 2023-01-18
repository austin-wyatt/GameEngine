using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
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
        public List<UIObject> Events = new List<UIObject>();

        public float TextScale = 1f;

        public CombatScene Scene;

        public EventLog(CombatScene scene)
        {
            Scene = scene;

            LogArea = new ScrollableArea(default, new UIScale(0.75f, 0.5f), default, new UIScale(0.75f, 2f), 
                scrollbarWidth: 0.05f, scrollSide: ScrollbarSide.Left);
            LogArea.BaseComponent.SetColor(new Vector4(0.33f, 0.33f, 0.25f, 0.5f));
            //LogArea.BaseComponent.SetColor(new Vector4(0.15f, 0.15f, 0.1f, 1f));

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

            Text_Drawing textComponent = new Text_Drawing(eventText, Text_Drawing.DEFAULT_FONT, 12, brush, Color.FromArgb(84, 84, 64), lineHeightMult: 0.75f);
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
    }

    
}
