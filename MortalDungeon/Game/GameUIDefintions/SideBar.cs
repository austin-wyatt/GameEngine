using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Objects.PropertyAnimations;
using Empyrean.Game.Player;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.UI
{
    public class SideBar
    {
        public CombatScene Scene;

        public UIObject ControlBar = null;
        public UIObject MinimizedBar = null;

        public UIObject PartyWindow = null;
        public UIObject CampWindow = null;

        public UIObject ParentObject = new UIBlock(new Vector3(-500, 0, 0));

        public SideBar(CombatScene scene) 
        {
            Scene = scene;

            const float BAR_HEIGHT = 0.5f;

            ControlBar = UIHelpers.CreateWindow(new UIScale(0.15f, BAR_HEIGHT), "Control bar", null, scene, false, false);
            ControlBar.SetPositionFromAnchor(new Vector3(0, WindowConstants.ScreenUnits.Y * 0.15f, 0), UIAnchorPosition.TopLeft);
            ControlBar.Draggable = false;

            MinimizedBar = UIHelpers.CreateWindow(new UIScale(0.02f, BAR_HEIGHT), "Minimized bar", null, scene, false, false);
            MinimizedBar.SetPositionFromAnchor(new Vector3(0, WindowConstants.ScreenUnits.Y * 0.15f, 0), UIAnchorPosition.TopLeft);
            //MinimizedBar.SetPositionFromAnchor(ControlBar.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            MinimizedBar.Draggable = false;

            MinimizedBar.Clickable = true;
            MinimizedBar.Click += (s, e) =>
            {
                MaximizeSidebar();
            };
            MinimizedBar.GenerateReverseTree(Scene.UIManager);


            Icon minimizeIcon = new Icon(new UIScale(0.07f, 0.07f), UISheetIcons.Minimize, Spritesheets.UISheet);
            minimizeIcon.Clickable = true;
            minimizeIcon.SetColor(_Colors.UILightGray);
            UIHelpers.AddTimedHoverTooltip(minimizeIcon, "Minimize", scene);

            minimizeIcon.Click += (s, e) =>
            {
                MinimizeSidebar();
            };

            minimizeIcon.SetPositionFromAnchor(ControlBar.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(-2, 2, 0), UIAnchorPosition.TopCenter);

            Icon partyIcon = new Icon(new UIScale(0.12f, 0.12f), UISheetIcons.PartyIcon, Spritesheets.UISheet, true);
            partyIcon.Clickable = true;
            UIHelpers.AddTimedHoverTooltip(partyIcon, "Party", scene);

            partyIcon.Click += (s, e) =>
            {
                CreatePartyWindow();
            };

            partyIcon.SetPositionFromAnchor(ControlBar.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, 30, 0), UIAnchorPosition.TopCenter);


            Icon campIcon = new Icon(new UIScale(0.12f, 0.12f), UISheetIcons.Fire, Spritesheets.UISheet, true);
            campIcon.Clickable = true;
            UIHelpers.AddTimedHoverTooltip(campIcon, "Camp", scene);
            campIcon.RenderAfterParent = true;

            campIcon.Click += (s, e) =>
            {
                CreateCampWindow();
            };

            campIcon.SetPositionFromAnchor(partyIcon.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 15, -0.0001f), UIAnchorPosition.TopCenter);

            Icon moveIcon = new Icon(new UIScale(0.12f, 0.12f), IconSheetIcons.WalkingBoot, Spritesheets.IconSheet, true);
            moveIcon.Clickable = true;
            UIHelpers.AddTimedHoverTooltip(moveIcon, "Enable right click movement", scene);
            moveIcon.RenderAfterParent = true;

            moveIcon.SelectedColor = _Colors.IconSelected;

            moveIcon.Click += (s, e) =>
            {
                moveIcon.OnSelect(!moveIcon.Selected);

                Scene.ContextManager.SetFlag(GeneralContextFlags.RightClickMovementEnabled, moveIcon.Selected);
            };

            moveIcon.SetPositionFromAnchor(campIcon.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 15, -0.0001f), UIAnchorPosition.TopCenter);


            EventLog log = new EventLog(Scene);
            Scene.EventLog = log;
            log.LogArea.SetVisibleAreaPosition(ControlBar.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(15, 10, 0), UIAnchorPosition.TopLeft);


            ControlBar.AddChild(minimizeIcon);
            ControlBar.AddChild(partyIcon);
            ControlBar.AddChild(campIcon);
            ControlBar.AddChild(moveIcon);
            ControlBar.AddChild(log.LogArea, 10);

            ParentObject.AddChild(ControlBar);
            ParentObject.AddChild(MinimizedBar);

            MaximizeSidebar();
        }

        public void MinimizeSidebar() 
        {
            ControlBar.SetRender(false);
            MinimizedBar.SetRender(true);
        }

        public void MaximizeSidebar()
        {
            ControlBar.SetRender(true);
            MinimizedBar.SetRender(false);
        }

        public void CreatePartyWindow() 
        {
            if (PartyWindow != null)
            {
                ParentObject.RemoveChild(PartyWindow);
            }

            PartyWindow = UIHelpers.CreateWindow(new UIScale(0.5f, 0.75f), "PartyWindow", ParentObject, Scene, true);

            CreatePartyWindowList();

            #region group buttons
            Icon createGroup = new Icon(new UIScale(0.1f, 0.1f), UISheetIcons.Shield, Spritesheets.UISheet, true);
            createGroup.BaseObject.BaseFrame.SetBaseColor(new Vector4(0.125f, 0.836f, 0.125f, 1));
            createGroup.Clickable = true;
            UIHelpers.AddTimedHoverTooltip(createGroup, "Create group\nYour friends must be within 10 yalms", Scene);
            createGroup.RenderAfterParent = true;

            createGroup.Click += (s, e) =>
            {
                Task.Run(() => 
                {
                    //Scene.CreateUnitGroup();
                    CreatePartyWindowList();
                });
            };

            createGroup.SetPositionFromAnchor(PartyWindow.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-10, 50, -0.000001f), UIAnchorPosition.TopRight);

            PartyWindow.AddChild(createGroup, 10);

            Icon dissolveGroup = new Icon(new UIScale(0.1f, 0.1f), UISheetIcons.BrokenShield, Spritesheets.UISheet, true);
            dissolveGroup.BaseObject.BaseFrame.SetBaseColor(new Vector4(0.76f, 0.08f, 0.16f, 1));
            dissolveGroup.Clickable = true;
            dissolveGroup.RenderAfterParent = true;

            dissolveGroup.Click += (s, e) =>
            {
                //Task.Run(() => Scene.DissolveUnitGroup(true, CreatePartyWindowList));
            };

            dissolveGroup.SetPositionFromAnchor(createGroup.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(-0, 10, -0.000001f), UIAnchorPosition.TopLeft);
            UIHelpers.AddTimedHoverTooltip(dissolveGroup, "Dissolve group", Scene);


            PartyWindow.AddChild(dissolveGroup, 10);
            #endregion

            ParentObject.AddChild(PartyWindow, 11);
        }

        public void CreatePartyWindowList() 
        {
            if (PartyWindow == null)
                return;

            var foundScrollArea = PartyWindow.Children.Find(obj => obj.Name == "party_window_scrollable_area");
            if (foundScrollArea != null) 
            {
                PartyWindow.Children.Remove(foundScrollArea);
            }

            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(0.3f, 0.7f), default, new UIScale(0.35f, 0.7f), 0.05f) 
            { 
                Name = "party_window_scrollable_area" 
            };

            UIList list = new UIList(default, new UIScale(0.3f, 0.13f), 0.075f);

            var unitList = Scene._units.FindAll(u => u.AI.GetControlType() == ControlType.Controlled && u.AI.Team == UnitTeam.PlayerUnits);

            //if(Scene.UnitGroup != null) 
            //{
            //    foreach (var unit in Scene.UnitGroup.SecondaryUnitsInGroup) 
            //    {
            //        unitList.Add(unit);
            //    }
            //}

            //for (int i = 0; i < unitList.Count; i++)
            //{
            //    var item = list.AddItem("");

            //    Vector4 textColor = _Colors.UITextBlack;

            //    bool isSecondaryUnit = false;

            //    string unitName = unitList[i].Name;
            //    if (Scene.UnitGroup != null)
            //    {
            //        if (Scene.UnitGroup.PrimaryUnit == unitList[i])
            //        {
            //            textColor = new Vector4(0f, 0.32f, 0.07f, 1);
            //        }
            //        else if (Scene.UnitGroup.SecondaryUnitsInGroup.Contains(unitList[i]))
            //        {
            //            textColor = new Vector4(0.56f, 0.05f, 0.55f, 1);
            //            isSecondaryUnit = true;
            //        }
            //    }

            //    TextComponent nameBox = new TextComponent();
            //    nameBox.SetTextScale(0.03f);
            //    nameBox.SetText(unitName);
            //    nameBox.SetColor(textColor);

            //    nameBox.SetPositionFromAnchor(item.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopCenter) + new Vector3(0, 3, 0), UIAnchorPosition.TopCenter);

            //    item.BaseComponent.AddChild(nameBox);

            //    HealthBar bar = new HealthBar(default, new UIScale(0.25f, 0.03f));

            //    bar.SetHealthPercent(unitList[i].Info.Health / unitList[i].Info.MaxHealth, UnitTeam.PlayerUnits);
            //    bar.SetPositionFromAnchor(nameBox.GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 3, 0), UIAnchorPosition.TopCenter);

            //    item.BaseComponent.AddChild(bar);

            //    ShieldBar shieldBar = new ShieldBar(default, new UIScale(0.15f, 0.04f));
            //    shieldBar.SetCurrentShields(unitList[i].Info.CurrentShields);
            //    shieldBar.SetPositionFromAnchor(bar.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 3, 0), UIAnchorPosition.TopLeft);

            //    item.BaseComponent.AddChild(shieldBar);

            //    int index = i;
            //    item.Click += (s, e) =>
            //    {
            //        if (isSecondaryUnit) 
            //        {
            //            Scene.SmoothPanCameraToUnit(Scene.UnitGroup.PrimaryUnit, 1);
            //        }
            //        else
            //        {
            //            Scene.SmoothPanCameraToUnit(unitList[index], 1);
            //            Scene.SelectUnit(unitList[index]);
            //        }
            //    };
            //}


            scrollableArea.SetVisibleAreaPosition(PartyWindow.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(5, 5, 0), UIAnchorPosition.TopLeft);
            list.SetPositionFromAnchor(scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);

            scrollableArea.BaseComponent.AddChild(list);

            PartyWindow.AddChild(scrollableArea);
        }

        public void CreateCampWindow() 
        {
            if(CampWindow != null)
            {
                ParentObject.RemoveChild(CampWindow);
            }

            CampWindow = UIHelpers.CreateWindow(new UIScale(0.3f, 0.3f), "CampWindow", ParentObject, Scene, true);

            Icon campButton = new Icon(new UIScale(0.15f, 0.15f), UISheetIcons.Fire, Spritesheets.UISheet, true);
            //createGroup.BaseObject.BaseFrame.SetBaseColor(new Vector4(0.125f, 0.836f, 0.125f, 1));
            campButton.Clickable = true;
            UIHelpers.AddTimedHoverTooltip(campButton, "Camp for four hours", Scene);
            campButton.RenderAfterParent = true;

            campButton.Click += (s, e) =>
            {
                if (!Scene.InCombat || true) 
                {
                    foreach(var unit in PlayerParty.UnitsInParty) 
                    {
                        unit.Rest();
                    }

                    //if(Scene.UnitGroup != null) 
                    //{
                    //    foreach (var unit in Scene.UnitGroup.SecondaryUnitsInGroup)
                    //    {
                    //        unit.Rest();
                    //    }
                    //}

                    Scene.SetTime(Scene.Time + DayNightCycle.HOUR * 4);
                }
            };

            campButton.SetPositionFromAnchor(CampWindow.GetAnchorPosition(UIAnchorPosition.Center) + new Vector3(0, 0, -0.000001f), UIAnchorPosition.Center);

            CampWindow.AddChild(campButton);

            ParentObject.AddChild(CampWindow, 10);
        }
    }
}
