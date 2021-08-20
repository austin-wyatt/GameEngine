using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class TabMenu : UIObject
    {
        private CombatScene Scene;

        public List<UIBlock> Tabs = new List<UIBlock>();
        public List<Button> TabAccessButtons = new List<Button>();

        public int CurrentTab = 0;

        public TabMenu() 
        {
            UIBlock mainWindow = new UIBlock(WindowConstants.CenterScreen, new UIDimensions(WindowConstants.ScreenUnits.X * 0.75f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 2));
            mainWindow.SetColor(Colors.UILightGray);
            //mainWindow.MultiTextureData.MixTexture = false;

            mainWindow.SetPositionFromAnchor(WindowConstants.CenterScreen * new Vector3(2, 1, 1), UIAnchorPosition.RightCenter);

            BaseComponent = mainWindow;

            AddChild(mainWindow, 1);

            Clickable = true;

            CreateTab();
            CreateTab();
            CreateTab();
            CreateTab();

            CreateTabAccessButton(0, "Dev");
            CreateTabAccessButton(1, "Inventory");
            CreateTabAccessButton(2, "Journal");
            CreateTabAccessButton(3, "Map");
        }

        public int CreateTab() 
        {
            UIBlock tab = new UIBlock(default, new UIDimensions(WindowConstants.ScreenUnits.X * 0.7f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 1.8f));
            tab.SetColor(Colors.UILightGray);
            tab.MultiTextureData.MixTexture = false;

            tab.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new UIScale(0.0125f, -0.0125f).ToDimensions(), UIAnchorPosition.BottomLeft);

            BaseComponent.AddChild(tab);
            tab.SetRender(false);

            Tabs.Add(tab);
            return Tabs.IndexOf(tab);
        }

        public void CreateTabAccessButton(int tab, string name) 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.24f, BaseComponent.Size.Y / 15), name, 0.043f, Colors.UILightGray, Colors.UITextBlack);
            button.BaseComponent.MultiTextureData.MixTexture = false;

            button.OnClickAction = () =>
            {
                SelectTab(tab);
            };


            if (tab > 0)
            {
                button.SetPositionFromAnchor(TabAccessButtons[tab - 1].GetAnchorPosition(UIAnchorPosition.BottomRight) + new Vector3(2, 0, 0), UIAnchorPosition.BottomLeft);
            }
            else
            {
                button.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(4, 4, 0), UIAnchorPosition.TopLeft);
            }
            TabAccessButtons.Add(button);
            BaseComponent.AddChild(button);
        }

        public void SelectTab(int tab) 
        {
            if (tab >= Tabs.Count)
                return;

            Tabs[CurrentTab].SetRender(false);
            TabAccessButtons[CurrentTab].SetSelected(false);

            Tabs[tab].SetRender(true);
            TabAccessButtons[tab].SetSelected(true);

            CurrentTab = tab;
        }

        public void AddToScene(CombatScene scene) 
        {
            scene.AddUI(this, 999999);

            Scene = scene;
        }

        public void Display(bool display) 
        {
            SetRender(display);

            Scene.ContextManager.SetFlag(GeneralContextFlags.TabMenuOpen, display);

            if (display)
            {
                Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
                Scene.MessageCenter.SendMessage(msg);
            }
            else 
            {
                Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
                Scene.MessageCenter.SendMessage(msg);
            }
        }
    }
}
