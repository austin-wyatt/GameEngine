using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Entities;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Engine_Classes.Rendering;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Empyrean.Game.Settings;
using Empyrean.Engine_Classes.Text;

namespace Empyrean.Game.UI
{
    public class TabMenu : UIObject
    {
        private CombatScene Scene;

        //public List<UIBlock> Tabs = new List<UIBlock>();
        public List<ScrollableArea> Tabs = new List<ScrollableArea>();
        public List<Button> TabAccessButtons = new List<Button>();

        public int CurrentTab = 0;

        public TabMenu() 
        {
            UIBlock mainWindow = new UIBlock(WindowConstants.CenterScreen, new UIDimensions(WindowConstants.ScreenUnits.X * 0.75f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 2));
            mainWindow.SetColor(_Colors.UILightGray);
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
            //UIBlock tab = new UIBlock(default, new UIDimensions(WindowConstants.ScreenUnits.X * 0.7f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 1.8f));

            ScrollableArea tab = new ScrollableArea(default, new UIDimensions(WindowConstants.ScreenUnits.X * 0.7f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 1.8f), 
                default, new UIDimensions(WindowConstants.ScreenUnits.X * 0.7f * WindowConstants.AspectRatio, WindowConstants.ScreenUnits.Y * 3f));

            tab.SetColor(_Colors.UILightGray);
            tab.MultiTextureData.MixTexture = false;

            //tab.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new UIScale(0.0125f, -0.0125f).ToDimensions(), UIAnchorPosition.BottomLeft);

            tab.SetVisibleAreaPosition(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new UIScale(0.0125f, -0.0125f).ToDimensions(), UIAnchorPosition.BottomLeft);

            BaseComponent.AddChild(tab);
            tab.SetRender(false);

            Tabs.Add(tab);
            return Tabs.IndexOf(tab);
        }

        public void CreateTabAccessButton(int tab, string name) 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.24f, BaseComponent.Size.Y / 15), UIManager.DEFAULT_FONT_INFO_16, name);
            button.BaseComponent.MultiTextureData.MixTexture = false;

            button.Click += (s, e) =>
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

            ForceTreeRegeneration();
        }

        private void PopulateMenus() 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 14), "Toggle combat");
            button.BaseComponent.MultiTextureData.MixTexture = false;

            button.Click += (s, e) =>
            {
                if (Scene.InCombat)
                {
                    Scene.EndCombat();
                }
            };

            button.SetPositionFromAnchor(Tabs[0].BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].BaseComponent.AddChild(button);

            Button unitButton = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 14), "Toggle Wireframe");
            unitButton.BaseComponent.MultiTextureData.MixTexture = false;

            bool wireframe = false;

            unitButton.Click += (s, e) =>
            {
                wireframe = !wireframe;

                if (wireframe)
                {
                    Window.QueueToRenderCycle(() => GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line));
                }
                else
                {
                    Window.QueueToRenderCycle(() => GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill));
                }
            };

            unitButton.SetPositionFromAnchor(button.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].BaseComponent.AddChild(unitButton);

            Button unitButton2 = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 14), "(-)Pattern Tool");
            unitButton2.BaseComponent.MultiTextureData.MixTexture = false;

            unitButton2.Click += (s, e) =>
            {
                Scene.ContextManager.SetFlag(GeneralContextFlags.PatternToolEnabled, !Scene.ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled));
                if (Scene.ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled))
                {
                    unitButton2.TextBox.SetText("(+)Pattern Tool");
                }
                else
                {
                    unitButton2.TextBox.SetText("(-)Pattern Tool");
                }
            };

            unitButton2.SetPositionFromAnchor(unitButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].BaseComponent.AddChild(unitButton2);

            Button toggleAI = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 14), "Toggle frame limit");
            toggleAI.BaseComponent.MultiTextureData.MixTexture = false;


            toggleAI.Click += (s, e) =>
            {
                Program.Window.RenderFrequency = Program.Window.RenderFrequency > 1 ? 0 : 200;
            };

            toggleAI.SetPositionFromAnchor(unitButton2.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            Tabs[0].BaseComponent.AddChild(toggleAI);

            Button updateMaps = CreateButton("Toggle Vysnc", () =>
            {
                Window.QueueToRenderCycle(() =>
                {
                    if (SettingsManager.GetSetting<bool>(Setting.VsyncEnabled))
                    {
                        SettingsManager.SetSetting(Setting.VsyncEnabled, false);
                        Program.Window.VSync = VSyncMode.Off;
                    }
                    else
                    {
                        SettingsManager.SetSetting(Setting.VsyncEnabled, true);
                        Program.Window.VSync = VSyncMode.On;
                    }
                });
                
            }, toggleAI.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            Button turboButton = null;
            turboButton = CreateButton("Enable Turbo", () =>
            {
                bool turbo = SettingsManager.GetSetting<bool>(Setting.MovementTurbo);
                if (!turbo)
                {
                    turboButton.TextBox.SetText("Disable Turbo");
                }
                else
                {
                    turboButton.TextBox.SetText("Enable Turbo");
                }

                SettingsManager.SetSetting(Setting.MovementTurbo, !turbo);
            }, updateMaps.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));


            Button plusButton = CreateButton("Measure Obj Size", () =>
            {
                //var tent = new Structures.Tent();

                //long preObj = GC.GetTotalMemory(true);
                ////var list1 = tent.TilePattern.ToList();
                ////var rotations = (int)tent.Rotations;
                ////var idealCenter = new Map.FeaturePoint(10, 25);

                ////var tempList = Scene._tileMapController.LoadedFeatures[1].AffectedPoints.ToList();

                //long postObj = GC.GetTotalMemory(true);

                //Console.WriteLine("Size of object is: " + (postObj - preObj) + " bytes");

                GC.Collect();
            }, turboButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            Button visionTestButton = CreateButton("Music test", () =>
            {
                Sound music = new Sound(Music.HopefulMusic) { Gain = 0.02f, Loop = true, EndTime = 215 };
                music.Play();
            }, plusButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            Button minusColor = CreateButton("Heal All", () =>
            {
                foreach(var u in Scene._units)
                {
                    u.Revive();
                    u.SetResF(ResF.Health, 100);

                    var instance = new Abilities.DamageInstance();
                    instance.Damage.Add(Abilities.DamageType.HealthRemoval, -100);
                    u.ApplyDamage(new DamageParams(instance));
                }
            }, visionTestButton.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            bool display = false;
            Button minusBlue = CreateButton("Toggle move bar", () =>
            {
                display = !display;
                Scene.ShowEnergyDisplayBars(display);
            }, minusColor.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));


            Button loadEntity = CreateButton("Collect Garbage", () =>
            {
                GC.Collect();
            }, minusBlue.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0));

            //ForceTreeRegeneration();
        }

        private Button CreateButton(string text, Action action, Vector3 prevButtonPos) 
        {
            Button button = new Button(default, new UIScale(BaseComponent.Size.X * 0.4f, BaseComponent.Size.Y / 15), new FontInfo(UIManager.DEFAULT_FONT_INFO_16, 14), text);
            //button.BaseComponent.MultiTextureData.MixTexture = false;

            button.Click += (s, e) =>
            {
                action?.Invoke();
            };

            button.SetPositionFromAnchor(prevButtonPos, UIAnchorPosition.TopLeft);

            Tabs[0].BaseComponent.AddChild(button);

            return button;
        }
    }
}
