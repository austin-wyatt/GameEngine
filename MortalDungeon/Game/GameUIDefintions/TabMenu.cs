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

            PopulateMenus();
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

        private void PopulateMenus() 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), "Toggle combat", 0.043f, Colors.UILightGray, Colors.UITextBlack);
            button.BaseComponent.MultiTextureData.MixTexture = false;

            button.OnClickAction = () =>
            {
                if (Scene.InCombat)
                {
                    Scene.EndCombat();
                }
                else 
                {
                    Scene._units[0].Info.Health = 100;
                    Scene._units[0].SetShields(5);
                    Scene.StartCombat();
                }
            };

            button.SetPositionFromAnchor(Tabs[0].GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].AddChild(button);

            Button unitButton = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), "Select Guy", 0.043f, Colors.UILightGray, Colors.UITextBlack);
            unitButton.BaseComponent.MultiTextureData.MixTexture = false;

            unitButton.OnClickAction = () =>
            {
                Scene.CurrentUnit = Scene._units[0];
                Scene.CurrentTeam = Scene._units[0].AI.Team;

                Scene.SelectUnit(Scene._units[0]);

                Scene.FillInTeamFog(true);
            };

            unitButton.SetPositionFromAnchor(button.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].AddChild(unitButton);

            Button unitButton2 = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), "Select Skeleton", 0.043f, Colors.UILightGray, Colors.UITextBlack);
            unitButton2.BaseComponent.MultiTextureData.MixTexture = false;

            unitButton2.OnClickAction = () =>
            {
                Scene.CurrentUnit = Scene._units[2];
                Scene.CurrentTeam = Scene._units[2].AI.Team;

                Scene.SelectUnit(Scene._units[2]);

                Scene.FillInTeamFog(true);
            };

            unitButton2.SetPositionFromAnchor(unitButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].AddChild(unitButton2);

            Button toggleAI = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), "Toggle Skele AI", 0.043f, Colors.UILightGray, Colors.UITextBlack);
            toggleAI.BaseComponent.MultiTextureData.MixTexture = false;


            toggleAI.OnClickAction = () =>
            {
                Scene._units[2].AI.ControlType = Scene._units[2].AI.ControlType == Units.ControlType.Basic_AI ? Units.ControlType.Controlled : Units.ControlType.Basic_AI;
            };

            toggleAI.SetPositionFromAnchor(unitButton2.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].AddChild(toggleAI);

            Button updateMaps = CreateButton("Show Chunks", () =>
            {
                Scene._tileMapController.TileMaps.ForEach(map =>
                {
                    map.TileChunks.ForEach(chunk =>
                    {
                        chunk.Tiles.ForEach(t =>
                        {
                            if (chunk.Cull)
                            {
                                t.Color = Colors.White;
                            }
                            else
                            {
                                t.Color = Colors.Red;
                            }

                            t.Update();
                        });
                    });
                });
            }, toggleAI.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            Button turboButton = null;
            turboButton = CreateButton("Enable Turbo", () =>
            {
                if (!Settings.MovementTurbo)
                {
                    turboButton.TextBox.SetText("Disable Turbo");
                }
                else
                {
                    turboButton.TextBox.SetText("Enable Turbo");
                }
                Settings.MovementTurbo = !Settings.MovementTurbo;
            }, updateMaps.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));


            Button plusButton = CreateButton("Calc Lighting", () =>
            {
                Scene.RefillLightObstructions();
            }, turboButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            //Button minusColor = CreateButton("Fill Obstructions", () =>
            //{
                
            //}, plusButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            //Button minusBlue = CreateButton("- Blue", () =>
            //{
            //    CombatScene.EnvironmentColor.Sub(new Color(0, 0, 0.05f, 0));
            //    Scene.FillInTeamFog(true);
            //}, minusColor.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));
        }

        private Button CreateButton(string text, Action action, Vector3 prevButtonPos) 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), text, 0.043f, Colors.UILightGray, Colors.UITextBlack);
            button.BaseComponent.MultiTextureData.MixTexture = false;

            button.OnClickAction = action;

            button.SetPositionFromAnchor(prevButtonPos, UIAnchorPosition.TopLeft);

            Tabs[0].AddChild(button);

            return button;
        }
    }
}
