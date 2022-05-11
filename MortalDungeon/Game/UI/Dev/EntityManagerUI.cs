using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Entities;
using Empyrean.Game.Map;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static Empyrean.Engine_Classes.UIComponents.Icon;

namespace Empyrean.Game.UI.Dev
{
    public class EntityManagerUI
    {
        public UIObject Window;

        public UIObject AddEntityWindow;
        public UIObject EntityPropertiesWindow;
        public UIObject EntityAbilitiesWindow;
        public UIObject UnitPrefabWindow;

        public CombatScene Scene;

        private ScrollableArea EntityArea;
        private UIList EntityList;

        public bool Displayed = false;

        public EntityManagerUI(CombatScene scene)
        {
            Scene = scene;


            Window = new UIBlock(new Vector3(500, 500, 0), new UIScale(1.5f, 1.5f));

            Window.MultiTextureData.MixTexture = false;
            Window.ZIndex = 1000;
            Window.Draggable = true;
            Window.Clickable = true;
            Window.Hoverable = true;

            Window.GenerateReverseTree(scene.UIManager);

            Window.Name = "Entity Manager";

            Icon exit = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.CrossedSwords, Spritesheets.IconSheet);
            exit.Clickable = true;
            exit.Click += (s, e) =>
            {
                Scene.RemoveUI(Window);
                exit.OnHoverEnd();

                Displayed = false;
            };
            exit.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            UIHelpers.AddTimedHoverTooltip(exit, "Exit", scene);

            Window.AddChild(exit);



            Icon refresh = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.Shield, Spritesheets.IconSheet, true);
            refresh.Clickable = true;
            refresh.Click += (s, e) =>
            {
                PopulateEntityList();
            };
            refresh.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

            UIHelpers.AddTimedHoverTooltip(refresh, "Populate List", scene);

            Window.AddChild(refresh);


            Icon addEntityButton = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.SpiderWeb, Spritesheets.IconSheet, true);
            addEntityButton.BaseComponent.SetColor(new Vector4(1, 0, 0, 1));
            addEntityButton.Clickable = true;
            addEntityButton.Click += (s, e) =>
            {
                CreateAddEntityWindow();
            };
            addEntityButton.SetPositionFromAnchor(refresh.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

            UIHelpers.AddTimedHoverTooltip(addEntityButton, "Add Entity", scene);

            Window.AddChild(addEntityButton);


            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(1.3f, 1.25f), default, new UIScale(1.3f, 5));
            scrollableArea.SetVisibleAreaPosition(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 50, 0), UIAnchorPosition.TopLeft);
            scrollableArea.BaseComponent.SetColor(_Colors.UIDisabledGray);

            EntityArea = scrollableArea;

            Window.AddChild(scrollableArea);

            UIList entityList = new UIList(default, new UIScale(1, 0.1f), 0.075f);
            entityList.SetPositionFromAnchor(scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            scrollableArea.BaseComponent.AddChild(entityList);

            EntityList = entityList;

            PopulateEntityList();
        }

        public void PopulateEntityList() 
        {
            EntityList.ClearItems();

            foreach (Entity entity in EntityManager.Entities) 
            {
                var item = EntityList.AddItem($"{(entity.Loaded ? "(L)" : "(U)")} " + entity.Handle.Name);

                item.Click += (s, e) =>
                {
                    CreateContextMenuFromEntity(entity);
                };
            }
        }

        public void CreateContextMenuFromEntity(Entity entity)
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(entity.Handle.Name);

            if (entity.Loaded)
            {
                list.AddItem("Unload Entity", (item) =>
                {
                    EntityManager.UnloadEntity(entity);
                    PopulateEntityList();
                    Scene.CloseContextMenu();
                });
            }
            else 
            {
                list.AddItem("Load Entity", (item) =>
                {
                    void loadEntityOnTile(Tile tile, MouseButton button)
                    {
                        EntityManager.LoadEntity(entity, new FeaturePoint(tile));

                        Scene.TileClicked -= loadEntityOnTile;
                        PopulateEntityList();
                    }

                    Scene.TileClicked += loadEntityOnTile;

                    Scene.CloseContextMenu();
                });
            }

            list.AddItem("Remove Entity", (item) =>
            {
                EntityManager.RemoveEntity(entity);
                PopulateEntityList();
                Scene.CloseContextMenu();
            });

            list.AddItem("Properties", (item) =>
            {
                CreateEntityPropertiesWindow(entity);
                Scene.CloseContextMenu();
            });

            list.AddItem("Abilities", (item) =>
            {
                CreateEntityAbilitiesWindow(entity);
                Scene.CloseContextMenu();
            });


            Scene.OpenContextMenu(menu);
        }
        public void CreateAbilityContextMenu(Ability ability)
        {
            (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList(ability.Name.ToString());

            
            list.AddItem("Remove Ability", (item) =>
            {
                ability.CastingUnit.Info.Abilities.Remove(ability);
                Scene.CloseContextMenu();
            });

            list.AddItem("Move Up", (item) =>
            {
                int index = ability.CastingUnit.Info.Abilities.IndexOf(ability);

                if (index > 0) 
                {
                    ability.CastingUnit.Info.Abilities.Remove(ability);
                    ability.CastingUnit.Info.Abilities.Insert(index - 1, ability);
                }

                Scene.CloseContextMenu();
            });

            list.AddItem("Move Down", (item) =>
            {
                int index = ability.CastingUnit.Info.Abilities.IndexOf(ability);

                if (index < ability.CastingUnit.Info.Abilities.Count - 1)
                {
                    ability.CastingUnit.Info.Abilities.Remove(ability);
                    ability.CastingUnit.Info.Abilities.Insert(index + 1, ability);
                }
                Scene.CloseContextMenu();
            });

            Scene.OpenContextMenu(menu);
        }


        public void CreateAddEntityWindow() 
        {
            if (AddEntityWindow != null) 
            {
                Window.RemoveChild(AddEntityWindow);
                AddEntityWindow = null;
            }

            AddEntityWindow = new UIBlock(new Vector3(500, 500, 0), new UIScale(1f, 1f));
            AddEntityWindow.MultiTextureData.MixTexture = false;
            AddEntityWindow.Draggable = true;
            AddEntityWindow.Clickable = true;
            AddEntityWindow.Hoverable = true;

            AddEntityWindow.Name = "AddEntityWindow";

            Icon exit = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.CrossedSwords, Spritesheets.IconSheet);
            exit.Clickable = true;
            exit.Click += (s, e) =>
            {
                Window.RemoveChild(AddEntityWindow);
                exit.OnHoverEnd();
            };
            exit.SetPositionFromAnchor(AddEntityWindow.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            UIHelpers.AddTimedHoverTooltip(exit, "Exit", Scene);

            AddEntityWindow.AddChild(exit);

            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(0.8f, 0.4f), default, new UIScale(0.9f, 5), 0.05f);
            scrollableArea.SetVisibleAreaPosition(AddEntityWindow.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            scrollableArea.BaseComponent.SetColor(_Colors.UIDisabledGray);
            scrollableArea.OnScroll(0);

            AddEntityWindow.AddChild(scrollableArea);

            
            AddUnitsToAddEntityArea(scrollableArea);


            Window.AddChild(AddEntityWindow, 1000000);
        }

        private void AddUnitsToAddEntityArea(UIObject scrollableArea) 
        {
            List<UnitCreationInfo> unitInfo = UnitCreationInfoSerializer.LoadAllUnitCreationInfo();


            Vector3 position = scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0);

            float count = 0;

            const int units_per_row = 4;

            foreach (UnitCreationInfo info in unitInfo) 
            {
                Unit u = info.CreateUnit(Scene);

                BaseObject obj = u.CreateBaseObject();

                obj._currentAnimation.Reset();
                obj._currentAnimation.Pause();

                obj.BaseFrame.ScaleX(0.15f / WindowConstants.AspectRatio);
                obj.BaseFrame.ScaleY(0.15f);

                UIBlock block = new UIBlock(default, new UIScale(0.15f, 0.15f));
                block.MultiTextureData.MixTexture = false;
                block.SetColor(_Colors.UILightGray);

                block.BaseObjects.Insert(0, obj);
                block._baseObject = obj;

                block.SetPositionFromAnchor(position + new Vector3(count % units_per_row * 50, (float)Math.Floor(count / units_per_row) * 80, 0), UIAnchorPosition.TopLeft);

                block.Clickable = true;

                UIHelpers.AddTimedHoverTooltip(block, info.Name, Scene);

                block.Click += (s, e) =>
                {
                    Entity newEntity = new Entity(info.CreateUnit(Scene));

                    newEntity.Handle.Name = $"{info.Name} {newEntity.EntityID}";

                    EntityManager.AddEntity(newEntity);
                    PopulateEntityList();
                };

                count++;

                scrollableArea.BaseComponent.AddChild(block, 10000);
            }
        }



        public void CreateEntityPropertiesWindow(Entity entity)
        {
            if (EntityPropertiesWindow != null)
            {
                Window.RemoveChild(EntityPropertiesWindow);
                EntityPropertiesWindow = null;
            }

            EntityPropertiesWindow = new UIBlock(new Vector3(500, 500, 0), new UIScale(1f, 1f));
            EntityPropertiesWindow.MultiTextureData.MixTexture = false;
            EntityPropertiesWindow.Draggable = true;
            EntityPropertiesWindow.Clickable = true;
            EntityPropertiesWindow.Hoverable = true;

            EntityPropertiesWindow.Name = "EntityPropertiesWindow";

            Icon exit = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.CrossedSwords, Spritesheets.IconSheet);
            exit.Clickable = true;
            exit.Click += (s, e) =>
            {
                Window.RemoveChild(EntityPropertiesWindow);
                exit.OnHoverEnd();
            };
            exit.SetPositionFromAnchor(EntityPropertiesWindow.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            UIHelpers.AddTimedHoverTooltip(exit, "Exit", Scene);

            EntityPropertiesWindow.AddChild(exit);


            Input nameField = new Input(default, new UIScale(0.8f, 0.1f), entity.Handle.Name, 0.075f);
            //nameField._textBox.SetTextColor(_Colors.UITextBlack);

            nameField.OnTypeAction = (name) =>
            {
                entity.Handle.SetName(name);
                
                PopulateEntityList();
            };

            nameField.SetPositionFromAnchor(EntityPropertiesWindow.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            EntityPropertiesWindow.AddChild(nameField, 100);

            #region Team select
            Select teamSelect = new Select(new UIScale(0.8f, 0.1f), 0.05f);

            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam))) 
            {
                SelectItem item = teamSelect.AddItem(team.Name(), () =>
                {
                    entity.Handle.SetTeam(team);
                    PopulateEntityList();
                });

                if (team == entity.Handle.AI.Team) 
                {
                    teamSelect.ItemSelected(item);
                }
            }

            teamSelect.SetPositionFromAnchor(nameField.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            EntityPropertiesWindow.AddChild(teamSelect, 100);
            #endregion

            #region AI select
            Select aiSelect = new Select(new UIScale(0.8f, 0.1f), 0.05f);

            foreach (ControlType controlType in Enum.GetValues(typeof(ControlType)))
            {
                SelectItem item = aiSelect.AddItem(controlType.Name(), () =>
                {
                    entity.Handle.AI.ControlType = controlType;
                    PopulateEntityList();
                });

                if (controlType == entity.Handle.AI.ControlType)
                {
                    aiSelect.ItemSelected(item);
                }
            }

            aiSelect.SetPositionFromAnchor(teamSelect.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            EntityPropertiesWindow.AddChild(aiSelect, 100);
            #endregion

            Window.AddChild(EntityPropertiesWindow, 10000);
        }


        public void CreateEntityAbilitiesWindow(Entity entity)
        {
            #region basic window
            if (EntityAbilitiesWindow != null)
            {
                Window.RemoveChild(EntityAbilitiesWindow);
                EntityAbilitiesWindow = null;
            }

            EntityAbilitiesWindow = new UIBlock(new Vector3(500, 500, 0), new UIScale(1f, 1f));
            EntityAbilitiesWindow.MultiTextureData.MixTexture = false;
            EntityAbilitiesWindow.Draggable = true;
            EntityAbilitiesWindow.Clickable = true;
            EntityAbilitiesWindow.Hoverable = true;

            EntityAbilitiesWindow.Name = "EntityAbilitiesWindow";

            Icon exit = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.CrossedSwords, Spritesheets.IconSheet);
            exit.Clickable = true;
            exit.Click += (s, e) =>
            {
                Window.RemoveChild(EntityAbilitiesWindow);
                exit.OnHoverEnd();
            };
            exit.SetPositionFromAnchor(EntityAbilitiesWindow.GetAnchorPosition(UIAnchorPosition.TopRight), UIAnchorPosition.TopRight);

            UIHelpers.AddTimedHoverTooltip(exit, "Exit", Scene);

            EntityAbilitiesWindow.AddChild(exit);

            
            #endregion

            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(0.8f, 0.4f), default, new UIScale(0.9f, 3), 0.05f);
            scrollableArea.SetVisibleAreaPosition(EntityAbilitiesWindow.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 50, 0), UIAnchorPosition.TopLeft);
            scrollableArea.BaseComponent.SetColor(_Colors.UIDisabledGray);
            scrollableArea.OnScroll(0);

            EntityAbilitiesWindow.AddChild(scrollableArea);

            UIList abilityList = new UIList(default, new UIScale(1, 0.1f), 0.075f);
            abilityList.SetPositionFromAnchor(scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            scrollableArea.BaseComponent.AddChild(abilityList);

            void populateAbilityList() 
            {
                abilityList.ClearItems();

                foreach (var ability in entity.Handle.Info.Abilities)
                {
                    abilityList.AddItem(ability.Name.ToString(), (_) =>
                    {
                        CreateAbilityContextMenu(ability);
                    });
                }
            }

            populateAbilityList();

            Icon refresh = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.Shield, Spritesheets.IconSheet, true);
            refresh.Clickable = true;
            refresh.Click += (s, e) =>
            {
                populateAbilityList();
            };
            refresh.SetPositionFromAnchor(EntityAbilitiesWindow.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

            UIHelpers.AddTimedHoverTooltip(refresh, "Populate List", Scene);

            EntityAbilitiesWindow.AddChild(refresh);


            Select abilityPrefabSelect = new Select(new UIScale(0.8f, 0.1f), 0.05f);

            //foreach (Prefab prefab in EntityParser.Prefabs)
            //{
            //    if (prefab.Type != PrefabType.Ability)
            //        continue;

            //    SelectItem item = abilityPrefabSelect.AddItem($"{prefab.Name}{(prefab.HasProfile ? " *" : "")}", () =>
            //    {
            //        Ability newAbility = EntityParser.ApplyPrefabToAbility(prefab, entity.Handle);

            //        if (newAbility == null)
            //            return;

            //        entity.Handle.Info.Abilities.Add(newAbility);

            //        PopulateEntityList();
            //        populateAbilityList();
            //    });
            //}

            abilityPrefabSelect.SetPositionFromAnchor(scrollableArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);

            EntityAbilitiesWindow.AddChild(abilityPrefabSelect, 100);


            Window.AddChild(EntityAbilitiesWindow, 10000);
        }
    }
}
