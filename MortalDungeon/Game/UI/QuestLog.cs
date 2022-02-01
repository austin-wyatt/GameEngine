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
    public class QuestLog
    {
        public UIObject Window;
        public CombatScene Scene;

        public ScrollableArea MapArea;

        public bool Displayed = false;
        public Quest SelectedQuest = null;

        private UIBlock _selectedQuestBlock;

        public QuestLog(CombatScene scene)
        {
            Scene = scene;
        }

        public void CreateWindow()
        {
            RemoveWindow();

            Window = UIHelpers.CreateWindow(new UIScale(2, 1.75f), "QuestLog", null, Scene, customExitAction: () =>
            {
                RemoveWindow();
            });

            Window.Draggable = false;

            Window.SetPosition(WindowConstants.CenterScreen);

            Scene.AddUI(Window, 1000);

            

            Displayed = true;

            PopulateData();

            if(SelectedQuest != null)
            {
                PopulateQuestInfo();
            }
        }

        public void RemoveWindow()
        {
            if (Window != null)
            {
                Scene.UIManager.RemoveUIObject(Window);
            }

            Displayed = false;
        }

        public void PopulateData()
        {
            Text activeQuestsLabel = new Text("Active Quests", Text.DEFAULT_FONT, 48, Brushes.Black);
            activeQuestsLabel.SetTextScale(0.1f);

            activeQuestsLabel.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsLabel);

            ScrollableArea activeQuestsScrollArea = new ScrollableArea(default, new UIScale(0.5f, 1.5f), default, new UIScale(0.5f, 3f), enableScrollbar: false);
            activeQuestsScrollArea.SetVisibleAreaPosition(activeQuestsLabel.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsScrollArea);

            UIList activeQuestsList = new UIList(default, new UIScale(0.5f, 0.1f), 0.05f);
            activeQuestsList.SetPositionFromAnchor(activeQuestsScrollArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            activeQuestsScrollArea.BaseComponent.AddChild(activeQuestsList);

            foreach (var quest in QuestManager.Quests)
            {
                activeQuestsList.AddItem(TextTableManager.GetTextEntry(0, quest.Title), (_) =>
                {
                    SelectedQuest = quest;
                    PopulateQuestInfo();
                });
            }

            _selectedQuestBlock = new UIBlock(default, new UIScale(1.4f, 1.55f));
            _selectedQuestBlock.SetColor(_Colors.UILightGray);
            _selectedQuestBlock.SetPositionFromAnchor(activeQuestsScrollArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

            Window.AddChild(_selectedQuestBlock);
        }

        public void PopulateQuestInfo()
        {
            _selectedQuestBlock.RemoveChildren();

            Text title = new Text(TextTableManager.GetTextEntry(0, SelectedQuest.Title), Text.DEFAULT_FONT, 48, Brushes.Black);
            title.SetTextScale(0.1f);

            title.SetPositionFromAnchor(_selectedQuestBlock.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(5, 5, 0), UIAnchorPosition.TopLeft);
            _selectedQuestBlock.AddChild(title);

            Text body = new Text(EventLog.WrapString(TextTableManager.GetTextEntry(0, SelectedQuest.Body), 40), Text.DEFAULT_FONT, 48, Brushes.Black);
            body.SetTextScale(0.075f);

            body.SetPositionFromAnchor(title.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(2, 5, 0), UIAnchorPosition.TopLeft);
            _selectedQuestBlock.AddChild(body);

            Vector3 currentPos = body.GetAnchorPosition(UIAnchorPosition.BottomLeft);

            int count = 0;
            foreach(var state in SelectedQuest.QuestStates)
            {
                if(count > SelectedQuest.CurrentState)
                {
                    continue;
                }

                bool stateCompleted = state.IsStateCompleted();

                currentPos.Y += 20;

                Text stateText = new Text(EventLog.WrapString(TextTableManager.GetTextEntry(0, state.TextEntry), 40), Text.DEFAULT_FONT, 48, Brushes.Black);
                stateText.SetTextScale(0.075f);

                stateText.SetPositionFromAnchor(currentPos, UIAnchorPosition.TopLeft);
                _selectedQuestBlock.AddChild(stateText);

                if (stateCompleted)
                {
                    //strikethrough on state text
                    UIBlock strikethrough = new UIBlock(default, new UIScale(stateText.Size.X, stateText.Size.Y * 0.075f));
                    strikethrough.SetPositionFromAnchor(stateText.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
                    strikethrough.SetColor(_Colors.Black);
                    strikethrough.SetAllInline(0);
                    stateText.AddChild(strikethrough);
                }

                currentPos = stateText.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(20, 10, 0);
                float stateX = currentPos.X;

                foreach (var obj in state.QuestObjectives)
                {
                    Text objectiveText = new Text(EventLog.WrapString(TextTableManager.GetTextEntry(0, obj.TextEntry), 45), Text.DEFAULT_FONT, 28, Brushes.Black);
                    objectiveText.SetTextScale(0.06f);

                    objectiveText.SetPositionFromAnchor(currentPos, UIAnchorPosition.TopLeft);
                    _selectedQuestBlock.AddChild(objectiveText);

                    if (obj.IsCompleted())
                    {
                        //strikethrough on objective text
                        UIBlock strikethrough = new UIBlock(default, new UIScale(objectiveText.Size.X, objectiveText.Size.Y * 0.075f));
                        strikethrough.SetPositionFromAnchor(objectiveText.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
                        strikethrough.SetColor(_Colors.Black);
                        strikethrough.SetAllInline(0);
                        objectiveText.AddChild(strikethrough);
                    }

                    currentPos = objectiveText.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0);
                }

                //return to the unindented position
                currentPos.X = stateX;

                count++;
            }
        }
    }
}
