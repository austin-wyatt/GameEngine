using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI.Dev
{
    public class FeatureManagerUI
    {
        //Have sections for:
        //currently loaded features
        //all features sorted by Id and showing their descriptive name
        //
        //have a way to jump to any feature's origin point (or just generally a way to jump to any point on the map)
        //
        //include a method to reload all of the features from the feature list file.
        //  (reinitialize the FeatureManager then load adjacent tilemaps from the current position.)

        public CombatScene Scene;

        public UIObject Window;
        public bool Displayed = false;

        public List<Button> TabAccessButtons = new List<Button>();

        public int SelectedTab = 0;

        private ScrollableArea _tableArea;
        private UIList _featureList;

        public FeatureManagerUI(CombatScene scene, Action onClose)
        {
            Scene = scene;

            Window = UIHelpers.CreateWindow(new UIScale(2f, 1.5f), "FeatureManagerUI", null, scene, customExitAction: onClose);

            CreateTabAccessButton(0, "Current");
            CreateTabAccessButton(1, "All");

            _tableArea = new ScrollableArea(default, new UIScale(1.5f, 1), default, new UIScale(1.5f, 2));

            Window.AddChild(_tableArea);

            _tableArea.SetVisibleAreaPosition(TabAccessButtons[0].GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);


            _featureList = new UIList(default, new UIScale(1.5f, 0.075f), 0.075f);

            _tableArea.BaseComponent.AddChild(_featureList);
            _featureList.SetPositionFromAnchor(_tableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);

            SelectTab(0);

            Icon refresh = new Icon(new UIScale(0.1f, 0.1f), IconSheetIcons.Shield, Spritesheets.IconSheet, true);
            refresh.Clickable = true;
            refresh.Hoverable = true;

            refresh.HoverColor = _Colors.UILightGray;

            refresh.Click += (s, e) =>
            {
                FeatureManager.AllFeatures = FeatureSerializer.LoadFeatureListFile();
            };
            refresh.SetPositionFromAnchor(TabAccessButtons[^1].GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);

            UIHelpers.AddTimedHoverTooltip(refresh, "Refetch feature definitions", scene);

            Window.AddChild(refresh);
        }

        public void CreateTabAccessButton(int tab, string name)
        {
            Button button = new Button(default, new UIScale(0.6f, 0.1f), name, 0.5f, _Colors.UILightGray, _Colors.UITextBlack);
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
                button.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(4, 4, 0), UIAnchorPosition.TopLeft);
            }
            TabAccessButtons.Add(button);
            Window.AddChild(button);
        }

        public void SelectTab(int tab)
        {
            TabAccessButtons[SelectedTab].SetSelected(false);
            TabAccessButtons[tab].SetSelected(true);

            SelectedTab = tab;

            switch (SelectedTab)
            {
                case 0:
                    BuildCurrentFeatureList();
                    break;
                case 1:
                    BuildAllFeaturesList();
                    break;
            }
        }

        public void BuildCurrentFeatureList()
        {
            _featureList.ClearItems();

            foreach (var feature in FeatureManager.LoadedFeatures)
            {
                var listItem = _featureList.AddItem(feature.Value.DescriptiveName);

                listItem.Click += (s, e) =>
                {
                    if (TileMapHelpers.IsValidTile(feature.Value.Origin))
                    {
                        var tile = TileMapHelpers.GetTile(feature.Value.Origin);

                        Vector3 pos = tile.Position;

                        var featurePos = new Vector3(pos.X, pos.Y, Scene._camera.Position.Z);

                        Scene.SmoothPanCamera(featurePos, 1);
                    }
                };
            }

            _featureList.ForceTreeRegeneration();
            _tableArea.PropagateScissorData(_featureList);
        }

        public void BuildAllFeaturesList()
        {
            _featureList.ClearItems();

            foreach (var feature in FeatureManager.AllFeatures.Features)
            {
                var listItem = _featureList.AddItem(feature.DescriptiveName + $" ({feature.Origin.X}, {feature.Origin.Y})");


                listItem.Click += (s, e) =>
                {
                    (var tooltip, var list) = UIHelpers.GenerateContextMenuWithList(feature.DescriptiveName);

                    var loadFeature = list.AddItem("Load feature");

                    loadFeature.Click += (s, e) =>
                    {
                        var tileMapPoint = new TileMapPoint(feature.Origin.X / TileMapManager.TILE_MAP_DIMENSIONS.X, feature.Origin.Y / TileMapManager.TILE_MAP_DIMENSIONS.Y);


                        TileMapManager.SetCenter(tileMapPoint);

                        TileMapManager.LoadMapsAroundCenter();

                        if (TileMapHelpers.IsValidTile(feature.Origin))
                        {
                            var tile = TileMapHelpers.GetTile(feature.Origin);

                            Vector3 pos = tile.Position;

                            var featurePos = new Vector3(pos.X, pos.Y, Scene._camera.Position.Z);

                            Scene.SmoothPanCamera(featurePos, 1);
                        }

                        //void loadTileMaps(SceneEventArgs args)
                        //{
                        //    //Scene._tileMapController.LoadSurroundingTileMaps(tileMapPoint, onFinish: () =>
                        //    //{
                        //    //    if (TileMapHelpers.IsValidTile(feature.Origin))
                        //    //    {
                        //    //        var tile = TileMapHelpers.GetTile(feature.Origin);

                        //    //        var featurePos = new Vector3(tile.BaseObject.BaseFrame.Position.X, tile.BaseObject.BaseFrame.Position.Y, Scene._camera.Position.Z);

                        //    //        Scene.SmoothPanCamera(featurePos, 1);
                        //    //    }
                        //    //});

                        //    Scene.RenderEnd -= loadTileMaps;
                        //}

                        //Scene.RenderEnd += loadTileMaps;
                    };

                    Scene.OpenContextMenu(tooltip);
                };
            }

            _featureList.ForceTreeRegeneration();
            _tableArea.PropagateScissorData(_featureList);
        }
    }
}
