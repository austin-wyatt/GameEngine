using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class WorldMap
    {
        public UIObject Window;
        public CombatScene Scene;

        public ScrollableArea MapArea;

        public bool Displayed = false;

        private const int MAX_TILES = 40000;

        public float TilesToScreenUnits = MAX_TILES / 1000;


        public WorldMap(CombatScene scene)
        {
            Scene = scene;

            CreateWindow();
        }

        public void CreateWindow() 
        {
            if(Window != null)
            {
                Scene.UIManager.RemoveUIObject(Window);
            }

            Window = UIHelpers.CreateWindow(new UIScale(3, 1.75f), "WorldMap", null, Scene, customExitAction: () =>
            {
                Scene.UIManager.RemoveUIObject(Window);
                Displayed = false;
            });

            Window.Draggable = false;

            Window.SetPosition(WindowConstants.CenterScreen);

            MapArea = new ScrollableArea(default, new UIScale(2.75f, 1.5f), default, new UIScale(1, 1), enableScrollbar: false, setScrollable: false) 
            {
                MaintainBaseAreaRelativePosition = true,
            };

            MapArea.SetVisibleAreaPosition(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(5, 50, 0), UIAnchorPosition.TopLeft);

            MapArea.BaseComponent.Draggable = true;
            MapArea.BaseComponent.Clickable = true;

            MapArea.BaseComponent.SetPositionFromAnchor(MapArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

            MapArea.VisibleArea.Scrollable = true;

            MapArea.VisibleArea.Scroll += (s, mouseState) =>
            {
                Vector3 localCoord = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(MortalDungeon.Window._cursorCoords));

                UIDimensions prevDimension = MapArea.BaseComponent.GetDimensions();

                Vector3 mousePosOnObj = localCoord - MapArea.BaseComponent.Position;

                Vector3 ratioFromEdge = new Vector3(mousePosOnObj.X / prevDimension.X, mousePosOnObj.Y / prevDimension.Y, 0);

                mousePosOnObj.Z = 0;

                Vector3 pos = MapArea.BaseComponent.Position;
                if (mouseState.ScrollDelta.Y > 0)
                {
                    MapArea.BaseComponent.ScaleXY(1.1f, 1.1f);


                    TilesToScreenUnits = MAX_TILES / (MapArea.BaseComponent.Scale.X * 1000);
                }
                else
                {
                    MapArea.BaseComponent.ScaleXY(0.9f, 0.9f);

                    TilesToScreenUnits = MAX_TILES / (MapArea.BaseComponent.Scale.X * 1000);

                }

                Vector3 currTopLeft = MapArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

                UIDimensions currDimensions = MapArea.BaseComponent.GetDimensions();

                Vector3 idealMousePos = new Vector3(currDimensions.X * ratioFromEdge.X, currDimensions.Y * ratioFromEdge.Y, 0);

                Vector3 diff = new Vector3(mousePosOnObj.X - idealMousePos.X, mousePosOnObj.Y - idealMousePos.Y, 0);

                MapArea.BaseComponent.SetPosition(MapArea.BaseComponent.Position + diff);

                PopulateFeatures();
            };

            MapArea.BaseComponent.SetColor(_Colors.UILightGray);

            Window.AddChild(MapArea);

            PopulateFeatures();
        }

        public void PopulateFeatures()
        {
            MapArea.BaseComponent.RemoveChildren();

            //place an icon for the current unit's position
            if (Scene.CurrentUnit != null)
            {
                float scaleFactor = 1 / TilesToScreenUnits * 0.05f * 5;

                if (scaleFactor > 0.25f)
                    scaleFactor = 0.25f;

                //UIBlock featureDisplay = new UIBlock(default, new UIScale(0.01f, 0.01f));
                UIBlock featureDisplay = new UIBlock(default, new UIScale(scaleFactor, scaleFactor));

                var baseObject = Scene.CurrentUnit.CreateBaseObject();

                baseObject.BaseFrame.ScaleX(scaleFactor);
                baseObject.BaseFrame.ScaleY(scaleFactor);

                baseObject.BaseFrame.ScaleX(1 / WindowConstants.AspectRatio);

                featureDisplay.BaseObjects.Insert(0, baseObject);
                featureDisplay._baseObject = baseObject;

                var coords = FeatureEquation.PointToMapCoords(Scene.CurrentUnit.Info.TileMapPosition);

                Vector3 topLeftPoint = MapArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

                Vector3 globalOrigin = new Vector3(coords.X, coords.Y, 0);

                globalOrigin /= TilesToScreenUnits;

                globalOrigin.X /= WindowConstants.AspectRatio;

                featureDisplay.SetPosition(globalOrigin + MapArea.BaseComponent.Position);

                MapArea.BaseComponent.AddChild(featureDisplay);
            }

            foreach (var item in FeatureManager.AllFeatures.Features)
            {
                if (item.MapSize == 0)
                    continue;

                float scaleFactor = 1 / TilesToScreenUnits * 0.05f * item.MapSize;

                if (scaleFactor > 0.25f)
                    scaleFactor = 0.25f;

                //UIBlock featureDisplay = new UIBlock(default, new UIScale(0.01f, 0.01f));
                UIBlock featureDisplay = new UIBlock(default, new UIScale(scaleFactor, scaleFactor));

                var animationList = AnimationManager.AnimationSets[item.AnimationSetName].BuildAnimationsFromSet();

                var baseObject = new BaseObject(animationList, 0, "", default, EnvironmentObjects.BaseTileBounds);

                baseObject.BaseFrame.ScaleX(scaleFactor);
                baseObject.BaseFrame.ScaleY(scaleFactor);

                baseObject.BaseFrame.ScaleX(1 / WindowConstants.AspectRatio);

                featureDisplay.BaseObjects.Insert(0, baseObject);
                featureDisplay._baseObject = baseObject;

                Vector3 topLeftPoint = MapArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

                Vector3 globalOrigin = new Vector3(item.Origin.X, item.Origin.Y, 0);

                globalOrigin /= TilesToScreenUnits;

                globalOrigin.X /= WindowConstants.AspectRatio;

                featureDisplay.SetPosition(globalOrigin + MapArea.BaseComponent.Position);

                UIHelpers.AddTimedHoverTooltip(featureDisplay, TextTableManager.GetTextEntry(0, item.NameTextEntry), Scene);

                MapArea.BaseComponent.AddChild(featureDisplay);
            }
        }
    }
}
