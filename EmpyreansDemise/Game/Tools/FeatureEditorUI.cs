using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Tools
{
    public class FeatureEditorUI
    {
        public UIObject LeftPane;
        public UIObject TopPane;
        public Scene Scene;

        public FeatureEditorUI(Scene scene)
        {
            Scene = scene;
            LeftPane = new UIBlock(default, new UIScale(0.6f, 1.75f), scaleAspectRatio:false);
            LeftPane.SetPositionFromAnchor(new Vector3(10, 115, 0), UIAnchorPosition.TopLeft);

            TopPane = new UIBlock(default, new UIScale(1.33f, 0.3f), scaleAspectRatio: false);
            TopPane.SAP(LeftPane.GAP(UIAnchorPosition.TopRight) + new Vector3(16, 0, 0), UIAnchorPosition.TopLeft);


            scene.AddUI(LeftPane);
            scene.AddUI(TopPane);

            CreateFeatureList();
        }

        public void Close()
        {
            Scene.RemoveUI(LeftPane);
            Scene.RemoveUI(TopPane);
        }

        public void CreateFeatureList()
        {
            LeftPane.RemoveChildren();

            FeatureBlockManager.LoadAllFeatureBlocks();


            ScrollableArea scrollableArea = new ScrollableArea(default, new UIScale(0.4f, 1f), default, new UIScale(0.4f, 1f), scaleAspectRatio: false);

            UIList list = new UIList(default, new UIScale(0.3f, 0.1f)) { _scaleAspectRatio = false };
            
            scrollableArea.BaseComponent.AddChild(list);
            scrollableArea.SetVisibleAreaPosition(LeftPane.GAP(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            list.SAP(scrollableArea.BaseComponent.GAP(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);

            LeftPane.AddChild(scrollableArea);

            foreach(var feature in FeatureBlockManager.GetAllLoadedFeatures())
            {
                list.AddItem(feature.DescriptiveName, (_) =>
                {

                });
            }

            //scrollableArea.FitToChildren();
        }
    }
}
