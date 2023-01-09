using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.SelectionTypes
{
    public class RadialSelection : SelectionInfo
    {
        /// <summary>
        /// The direction (in radians) that lies in the center of the sweep angle
        /// </summary>
        public float Direction = 0;

        /// <summary>
        /// The range in which the radial selection can be changed (in radians). Represents the full
        /// range so the range on either side of the Direction field will be SweepAngle / 2. <para/>
        /// </summary>
        public float SweepAngle = MathHelper.PiOver2;

        public float CurrAngle = float.MinValue;
        public float CurrMagnitude = float.MinValue;

        public float MaxMagnitude = float.MinValue;
        public float MinMagnitude = float.MinValue;

        public bool CapMagnitude = true;

        public RadialSelection(Ability ability) : base(ability) 
        {
            CanSelectTiles = true;
        }

        //Visual effect will be a line from the source point to the mouse cursor as well
        //as 2 thicker lines denoting the edges of the sweep angle (if the angle is below 2PI)

        //Line will be updated when a tile is hovered

        protected override void CheckSelectionStatus()
        {
            OnConditionsMet();

            if (UseAbility)
            {
                Ability.EnactEffect();
            }
        }

        public override void TileHovered(Tile tile)
        {
            base.TileHovered(tile);

            UpdateHoverIndicator(tile);
        }

        public override bool TileClicked(Tile clickedTile)
        {
            float len = GetMagnitudeFromTile(clickedTile);

            if (AngleToTileIsValid(clickedTile) && MagnitudeIsValid(len))
            {
                SelectedTiles.Add(clickedTile);

                CheckSelectionStatus();
                return true;
            }

            return false;
        }



        private GameObject _leftSweepIndicator;
        private GameObject _rightSweepIndicator;

        private GameObject _lineIndicator;

        private bool _validAngle = false;

        public override void CreateVisualIndicators()
        {
            base.CreateVisualIndicators();

            if(SweepAngle < MathHelper.TwoPi)
            {
                float yScale = 1;

                float indicatorScaleOffset = 0.4f;

                _leftSweepIndicator = new GameObject(Spritesheets.UISheet, 71);
                _leftSweepIndicator.SetScale(0.05f, yScale, 1);

                float leftSweepAngle = Direction + SweepAngle * 0.5f + MathHelper.PiOver2;

                _leftSweepIndicator.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(leftSweepAngle));
                _leftSweepIndicator.SetColor(_Colors.Tan);

                _leftSweepIndicator.SetPosition(SourceTile._position + 
                    new Vector3(
                        MathF.Sin(leftSweepAngle) * yScale * WindowConstants.ScreenUnits.X * indicatorScaleOffset, 
                        MathF.Cos(leftSweepAngle) * yScale * WindowConstants.ScreenUnits.Y * indicatorScaleOffset, 0.1f));
                _leftSweepIndicator.Name = "Left sweep indicator";
                Window.Scene._genericObjects.Add(_leftSweepIndicator);


                _rightSweepIndicator = new GameObject(Spritesheets.UISheet, 71);
                _rightSweepIndicator.SetScale(0.05f, yScale, 1);

                float rightSweepAngle = Direction - SweepAngle * 0.5f + MathHelper.PiOver2;

                _rightSweepIndicator.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(rightSweepAngle));
                _rightSweepIndicator.SetColor(_Colors.Tan);

                _rightSweepIndicator.SetPosition(SourceTile._position + 
                    new Vector3(
                        MathF.Sin(rightSweepAngle) * yScale * WindowConstants.ScreenUnits.X * indicatorScaleOffset, 
                        MathF.Cos(rightSweepAngle) * yScale * WindowConstants.ScreenUnits.Y * indicatorScaleOffset, 0.1f));
                _rightSweepIndicator.Name = "Right sweep indicator";
                Window.Scene._genericObjects.Add(_rightSweepIndicator);


                _lineIndicator = new GameObject(Spritesheets.UISheet, 71);
                _lineIndicator.SetScale(0.05f, yScale, 1);

                _lineIndicator.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(leftSweepAngle));
                _lineIndicator.SetColor(_Colors.Red);

                _lineIndicator.SetPosition(SourceTile._position);
                _lineIndicator.Name = "Line indicator";
                Window.Scene._genericObjects.Add(_lineIndicator);

                UpdateHoverIndicator(Window.Scene._tileMapController._hoveredTile);
            }
        }

        public override void RemoveVisualIndicators()
        {
            base.RemoveVisualIndicators();

            if(_leftSweepIndicator != null)
            {
                Window.Scene._genericObjects.Remove(_leftSweepIndicator);
                Window.Scene._genericObjects.Remove(_rightSweepIndicator);
                Window.Scene._genericObjects.Remove(_lineIndicator);
            }
        }

        private void UpdateHoverIndicator(Tile tile)
        {
            if (tile == null)
                return;


            float angle = GMath.AngleOfPoints(tile._position, SourceTile._position);

            if (float.IsNaN(angle))
                return;

            float len = GetMagnitudeFromTile(tile);

            if (AngleToTileIsValid(tile) && MagnitudeIsValid(len))
            {
                _lineIndicator.SetColor(_Colors.Green);
                _validAngle = true;
            }
            else
            {
                _lineIndicator.SetColor(_Colors.Red);
                _validAngle = false;
            }


            //Console.WriteLine(len);

            _lineIndicator.SetScale(0.03f, len * 2, 1);

            angle += MathHelper.PiOver2;

            _lineIndicator.BaseObject.BaseFrame.ResetRotation();

            _lineIndicator.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(angle));


            Vector3 linePos = SourceTile._position +
                new Vector3(
                    MathF.Sin(angle) * len * WindowConstants.ScreenUnits.X * 0.5f,
                    MathF.Cos(angle) * len * WindowConstants.ScreenUnits.Y * 0.5f, 0);

            linePos.Z = Math.Max(SourceTile._position.Z, tile._position.Z) + 0.11f;

            _lineIndicator.SetPosition(linePos);
        }

        private bool AngleToTileIsValid(Tile tile)
        {
            if (tile == null)
                return false;

            float angle = GMath.AngleOfPoints(tile._position, SourceTile._position);

            if (float.IsNaN(angle))
                return false;

            CurrAngle = angle;

            float leftSweepAngle = Direction + SweepAngle * 0.5f;
            float rightSweepAngle = Direction - SweepAngle * 0.5f;

            return GMath.IsAngleBetween(angle, leftSweepAngle, rightSweepAngle);
        }

        private bool MagnitudeIsValid(float magnitude)
        {
            bool valid = true;

            if(MaxMagnitude != float.MinValue)
            {
                valid = valid && (magnitude <= MaxMagnitude || CapMagnitude);
            }

            if(MinMagnitude != float.MinValue)
            {
                valid = valid && (magnitude >= MinMagnitude || CapMagnitude);
            }

            if (valid)
            {
                CurrMagnitude = magnitude;
            }

            return valid;
        }

        private float GetMagnitudeFromTile(Tile tile)
        {
            Vector3 lineVec = SourceTile._position - tile._position;

            lineVec.X /= WindowConstants.ScreenUnits.X;
            lineVec.Y /= WindowConstants.ScreenUnits.Y;

            float magnitude = lineVec.Xy.Length;

            if(MaxMagnitude != float.MinValue)
            {
                if(magnitude > MaxMagnitude && CapMagnitude)
                {
                    magnitude = MaxMagnitude;
                }
            }

            if (MinMagnitude != float.MinValue)
            {
                if (magnitude < MinMagnitude && CapMagnitude)
                {
                    magnitude = MinMagnitude;
                }
            }

            return magnitude;
        }
    }
}
