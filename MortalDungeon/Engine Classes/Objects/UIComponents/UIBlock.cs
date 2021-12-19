using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal class UIBlock : UIObject
    {
        internal UIBlock(Vector3 position = default, UIScale size = default, Vector2i spritesheetDimensions = default, int spritesheetPosition = 71, bool scaleAspectRatio = true, bool cameraPerspective = false, Spritesheet spritesheet = null)
        {
            //if (position == default)
            //{
            //    position = new Vector3(-1000, 0, 0);
            //}

            Position = position;
            Size = size == null ? Size : size;
            _scaleAspectRatio = scaleAspectRatio;
            Name = "UIBlock";
            CameraPerspective = cameraPerspective;


            Vector2i SpritesheetDimensions = spritesheetDimensions.X == 0 ? new Vector2i(1, 1) : spritesheetDimensions;
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Animation tempAnimation;

            if (spritesheet == null) 
            {
                spritesheet = Spritesheets.UISheet;
            }

            RenderableObject window = new RenderableObject(new SpritesheetObject(spritesheetPosition, spritesheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            window.BaseColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            window.SetBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1));
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject windowObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindow", position, EnvironmentObjects.UIBlockBounds);
            windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            AddBaseObject(windowObj);
            _baseObject = windowObj;

            windowObj.OutlineParameters.SetAllInline(2);

            MultiTextureData.MixTexture = true;
            MultiTextureData.MixPercent = 0.5f;
            MultiTextureData.MixedTexture = new Texture(UIHelpers.UI_BACKGROUND.Handle, TextureName.FogTexture);
            MultiTextureData.MixedTextureLocation = OpenTK.Graphics.OpenGL4.TextureUnit.Texture1;
            MultiTextureData.MixedTextureName = TextureName.FogTexture;


            SetSize(Size);

            SetOrigin(aspectRatio, Size);

            ValidateObject(this);
        }

        internal override void SetSize(UIScale size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 oldSize = new Vector2(Size.X, Size.Y);

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            _baseObject.BaseFrame.SetScaleAll(1);

            _baseObject.BaseFrame.ScaleX(aspectRatio);
            _baseObject.BaseFrame.ScaleX(ScaleFactor.X);
            _baseObject.BaseFrame.ScaleY(ScaleFactor.Y);

            Vector3 oldOrigin = new Vector3(Origin);

            Size = size;
            SetOrigin(aspectRatio, Size);
        }

        internal void SetOrigin() 
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;
            SetOrigin(aspectRatio, Size);
        }

        internal override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            if (flag == SetColorFlag.Base)
                DefaultColor = color;

            _baseObject.BaseFrame.SetBaseColor(color);
        }

        internal override void ScaleAddition(float f)
        {
            base.ScaleAddition(f);
        }

        internal override void ScaleAll(float f)
        {
            base.ScaleAll(f);
        }

        internal override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            Origin = new Vector3(Position.X - _originOffset.X, Position.Y - _originOffset.Y, Position.Z - _originOffset.Z);
        }
    }
}
