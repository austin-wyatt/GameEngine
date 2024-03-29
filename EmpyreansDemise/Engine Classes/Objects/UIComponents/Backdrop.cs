﻿using Empyrean.Engine_Classes;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Empyrean.Engine_Classes.UIComponents
{
    /// <summary>
    /// Functionally similar to the UIBlock class but only contains the backdrop as opposed to the backdrop + primary window
    /// </summary>
    public class Backdrop : UIObject
    {
        public Action _onClick;

        public Backdrop(Vector3 position, UIScale size = default, Vector2i spritesheetDimensions = default, int spritesheetPosition = 90, bool scaleAspectRatio = true)
        {
            Position = position;
            Size = size == null ? Size : size;
            _scaleAspectRatio = scaleAspectRatio;
            Name = "Backdrop";


            Vector2i SpritesheetDimensions = spritesheetDimensions.X == 0 ? new Vector2i(1, 1) : spritesheetDimensions;

            Animation tempAnimation;
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            UIScale ScaleFactor = new UIScale(Size.X, Size.Y);

            RenderableObject window = new RenderableObject(new SpritesheetObject(spritesheetPosition, Spritesheets.UISheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER_DEFERRED);

            window.ScaleX(aspectRatio);
            window.ScaleX(ScaleFactor.X);
            window.ScaleY(ScaleFactor.Y);
            window.SetBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1));
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject backdropObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindow", position, EnvironmentObjects.UIBlockBounds);
            backdropObj.BaseFrame.CameraPerspective = CameraPerspective;

            AddBaseObject(backdropObj);
            _baseObject = backdropObj;

            SetOrigin(aspectRatio, ScaleFactor);

            ValidateObject(this);
        }

        public override void SetColor(Vector4 color, SetColorFlag setColorFlag = SetColorFlag.Base)
        {
            _baseObject.BaseFrame.SetBaseColor(color);
        }

        public override void ScaleAddition(float f)
        {
            base.ScaleAddition(f);
        }

        public override void ScaleAll(float f)
        {
            base.ScaleAll(f);
        }

        public override void OnClick()
        {
            _onClick?.Invoke();
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;
            Origin = new Vector3(Position.X - _originOffset.X, Position.Y - _originOffset.Y, Position.Z - _originOffset.Z);
        }
    }
}
