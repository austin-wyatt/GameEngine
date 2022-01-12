using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class QuestLog
    {
        public UIObject Window;
        public CombatScene Scene;

        public ScrollableArea MapArea;

        public bool Displayed = false;

        public QuestLog(CombatScene scene)
        {
            Scene = scene;
        }

        public void CreateWindow()
        {
            if (Window != null)
            {
                Scene.UIManager.RemoveUIObject(Window);
            }

            Window = UIHelpers.CreateWindow(new UIScale(2, 1.75f), "QuestLog", null, Scene, customExitAction: () =>
            {
                Scene.UIManager.RemoveUIObject(Window);
                Displayed = false;
            });

            Window.Draggable = false;

            Window.SetPosition(WindowConstants.CenterScreen);
        }

        public void PopulateData()
        {
            Window.RemoveChildren();

            Text activeQuestsLabel = new Text("Active Quests", Text.DEFAULT_FONT, 48, Brushes.Black);

            activeQuestsLabel.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsLabel);

            ScrollableArea activeQuestsScrollArea = new ScrollableArea(default, new UIScale(0.3f, 1.5f), default, new UIScale(0.3f, 3f), enableScrollbar: false);
            activeQuestsScrollArea.SetVisibleAreaPosition(activeQuestsLabel.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsScrollArea);


        }


    }
}
