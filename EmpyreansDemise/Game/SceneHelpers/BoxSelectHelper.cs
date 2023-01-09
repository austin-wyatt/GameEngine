using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.SceneHelpers
{
    public class BoxSelectHelper
    {
        public CombatScene Scene;

        public bool BoxSelecting = false;

        private bool _allowSelection = true;
        public bool AllowSelection
        {
            get => _allowSelection;
            set
            {
                _allowSelection = value;
                if (BoxSelecting)
                {
                    EndSelection();
                }
            }
        }

        public Vector2 AnchorMouseCoords = new Vector2();
        public Vector2 CurrentMouseCoords = new Vector2();

        public Vector2 AnchorPoint = new Vector2();
        public Vector2 CurrentPoint = new Vector2();

        private UIBlock _selectionBlock;

        private bool _valid = false;

        public BoxSelectHelper() { }
        public BoxSelectHelper(CombatScene scene)
        {
            Scene = scene;

            _valid = true;
        }

        public bool PreliminarySelection = false;
        private int _preliminaryCounter = 0;
        public void StartPreliminarySelection()
        {
            _preliminaryCounter = 0;
            PreliminarySelection = true;
        }

        public void CheckPreliminarySelection()
        {
            _preliminaryCounter++;

            if(_preliminaryCounter > 1)
            {
                _preliminaryCounter = 0;
                PreliminarySelection = false;
                StartSelection();
            }
        }

        public void StartSelection()
        {
            if (!_valid)
                return;

            BoxSelecting = true;

            _selectionBlock = new UIBlock(scaleAspectRatio: false);
            _selectionBlock.SetRender(false);

            _selectionBlock.SetColor(_Colors.Transparent);

            Scene.AddUI(_selectionBlock);
        }

        public void EndSelection()
        {
            if (!_valid)
                return;

            BoxSelecting = false;

            Scene.RemoveUI(_selectionBlock);

            var units = GetUnitsInSelectionBox();

            Scene.SelectUnits(units);
        }

        public void DrawSelectionBox()
        {
            //TODO, draw box lines, highlight units and tiles, etc

            _selectionBlock.SetRender(true);

            Vector3 anchorScreenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(AnchorPoint);
            Vector3 currentScreenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(CurrentPoint);

            Vector3 topLeft = new Vector3(Math.Min(anchorScreenSpace.X, currentScreenSpace.X), Math.Min(anchorScreenSpace.Y, currentScreenSpace.Y), 0);
            Vector2 size = new Vector2(Math.Abs(anchorScreenSpace.X - currentScreenSpace.X), Math.Abs(anchorScreenSpace.Y - currentScreenSpace.Y));

            size.X /= WindowConstants.ScreenUnits.X * 0.5f;
            size.Y /= WindowConstants.ScreenUnits.Y * 0.5f;

            _selectionBlock.SetSize(new UIScale(size));
            _selectionBlock.SAP(topLeft, UIAnchorPosition.TopLeft);
        }

        public List<Unit> GetUnitsInSelectionBox()
        {
            List<Unit> units = new List<Unit>();

            Vector3 anchorRayNear = Scene._mouseRay.UnProject(AnchorMouseCoords.X, AnchorMouseCoords.Y, 0, Scene._camera, WindowConstants.ClientSize); // start of ray (near plane)
            Vector3 anchorRayFar = Scene._mouseRay.UnProject(AnchorMouseCoords.X, AnchorMouseCoords.Y, 1, Scene._camera, WindowConstants.ClientSize); // end of ray (far plane)

            Vector3 currentRayNear = Scene._mouseRay.UnProject(CurrentMouseCoords.X, CurrentMouseCoords.Y, 0, Scene._camera, WindowConstants.ClientSize); // start of ray (near plane)
            Vector3 currentRayFar = Scene._mouseRay.UnProject(CurrentMouseCoords.X, CurrentMouseCoords.Y, 1, Scene._camera, WindowConstants.ClientSize); // end of ray (far plane)


            float xUnit = anchorRayFar.X - anchorRayNear.X;
            float yUnit = anchorRayFar.Y - anchorRayNear.Y;

            float percentageAlongLine = (0 - anchorRayNear.Z) / (anchorRayFar.Z - anchorRayNear.Z);

            Vector3 anchorPointAtZ = new Vector3(anchorRayNear.X + xUnit * percentageAlongLine, anchorRayNear.Y + yUnit * percentageAlongLine, 0);

            xUnit = currentRayFar.X - currentRayNear.X;
            yUnit = currentRayFar.Y - currentRayNear.Y;

            percentageAlongLine = (0 - currentRayNear.Z) / (currentRayFar.Z - currentRayNear.Z);

            Vector3 currentPointAtZ = new Vector3(currentRayNear.X + xUnit * percentageAlongLine, currentRayNear.Y + yUnit * percentageAlongLine, 0);

            //anchorPointAtZ = WindowConstants.ConvertLocalToGlobalCoordinates(anchorPointAtZ);
            //currentPointAtZ = WindowConstants.ConvertLocalToGlobalCoordinates(currentPointAtZ);

            Vector3 unitPos = new Vector3();
            for (int i = 0; i < Scene._units.Count; i++)
            {
                unitPos = new Vector3(Scene._units[i].Position);
                WindowConstants.ConvertGlobalToLocalCoordinatesInPlace(ref unitPos);

                if (CheckRange(unitPos.X, anchorPointAtZ.X, currentPointAtZ.X) && CheckRange(unitPos.Y, anchorPointAtZ.Y, currentPointAtZ.Y))
                {
                    units.Add(Scene._units[i]);
                }
            }

            return units;
        }

        private bool CheckRange(float testVal, float val1, float val2)
        {
            float min = Math.Min(val1, val2);
            float max = Math.Max(val1, val2);

            return testVal >= min && testVal <= max;
        }
    }
}
