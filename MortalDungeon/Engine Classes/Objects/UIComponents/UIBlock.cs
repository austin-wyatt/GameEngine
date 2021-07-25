using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Game.UI
{
    public class UIBlock : UIObject
    {
        private bool _scaleAspectRatio = true;
        protected BaseObject _window;

        public UIBlock(Vector3 position, Vector2 size = default, Vector2i spritesheetDimensions = default, int spritesheetPosition = 71, bool scaleAspectRatio = true, bool cameraPerspective = false)
        {
            Position = position;
            Size = size.X == 0 ? Size : size;
            _scaleAspectRatio = scaleAspectRatio;
            Name = "UIBlock";
            CameraPerspective = cameraPerspective;


            Vector2i SpritesheetDimensions = spritesheetDimensions.X == 0 ? new Vector2i(1, 1) : spritesheetDimensions;
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Animation tempAnimation;

            RenderableObject window = new RenderableObject(new SpritesheetObject(spritesheetPosition, Spritesheets.UISheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject windowObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "UIWindow", position, EnvironmentObjects.UIBlockBounds);
            windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            BaseObjects.Add(windowObj);
            _window = windowObj;

            windowObj.OutlineParameters.SetAllInline(2);

            MultiTextureData.MixTexture = true;
            MultiTextureData.MixPercent = 0.5f;
            MultiTextureData.Texture = new Texture(UIHelpers.UI_BACKGROUND.Handle, TextureName.FogTexture);
            MultiTextureData.TextureLocation = OpenTK.Graphics.OpenGL4.TextureUnit.Texture1;
            MultiTextureData.TextureName = TextureName.FogTexture;


            SetSize(Size);

            SetOrigin(aspectRatio, Size);
        }

        public void SetSize(Vector2 size)
        {
            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;

            Vector2 ScaleFactor = new Vector2(size.X, size.Y);
            _window.BaseFrame.SetScaleAll(1);

            _window.BaseFrame.ScaleX(aspectRatio);
            _window.BaseFrame.ScaleX(ScaleFactor.X);
            _window.BaseFrame.ScaleY(ScaleFactor.Y);

            Size = size;
        }

        public override void SetColor(Vector4 color)
        {
            _window.BaseFrame.Color = color;
        }

        public override void ScaleAddition(float f)
        {
            base.ScaleAddition(f);
        }

        public override void ScaleAll(float f)
        {
            base.ScaleAll(f);
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);

            float aspectRatio = _scaleAspectRatio ? (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X : 1;
            Origin = new Vector3(Position.X - _originOffset.X, Position.Y - _originOffset.Y, Position.Z - _originOffset.Z);
        }
    }
}
