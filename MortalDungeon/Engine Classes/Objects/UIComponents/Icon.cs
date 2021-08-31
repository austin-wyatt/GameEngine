using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Icon : UIObject
    {
        public enum IconSheetIcons 
        {
            CrossedSwords,
            Shield,
            BleedingDagger,
            WalkingBoot,
            QuestionMark,
            SpiderWeb,
            Poison,
            BandagedHand,
            BowAndArrow,
            MasqueradeMask,
            BrokenMask = 12
        }

        public enum BackgroundType 
        {
            NeutralBackground = 10,
            BuffBackground = 30,
            DebuffBackground = 50
        }

        public Spritesheet _spritesheet;
        public Enum _spritesheetPosition;

        public static UIScale DefaultIconSize = new UIScale(0.25f, 0.25f);
        public static IconSheetIcons DefaultIcon = IconSheetIcons.QuestionMark; //question mark icon

        public Icon(UIScale size, Enum spritesheetPosition, Spritesheet spritesheet, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground)
        {
            Size = size;
            Name = "Icon";
            _spritesheet = spritesheet;
            _spritesheetPosition = spritesheetPosition;

            Animation tempAnimation;

            RenderableObject window = new RenderableObject(new SpritesheetObject(Convert.ToInt32(spritesheetPosition), _spritesheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            //window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject windowObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
            windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            BaseObjects.Add(windowObj);
            _baseObject = windowObj;

            //windowObj.OutlineParameters.SetAllInline(2);

            if (withBackground) 
            {
                RenderableObject background = new RenderableObject(new SpritesheetObject(Convert.ToInt32(backgroundType), Spritesheets.IconSheet, 2, 2).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

                //window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
                tempAnimation = new Animation()
                {
                    Frames = new List<RenderableObject>() { background },
                    Frequency = 0,
                    Repeats = 0
                };

                BaseObject backgroundObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
                BaseObjects.Add(backgroundObj);
                //UIObject iconBackground = new UIObject();
                //iconBackground.Name = "IconBackground";
                //iconBackground._baseObject = backgroundObj;
                //iconBackground.BaseObjects.Add(backgroundObj);

                //BaseComponent = iconBackground;
                //_baseObject = null;
                //BaseObjects.Clear();


                //AddChild(iconBackground);
            }

            SetSize(Size);

            ValidateObject(this);

            LoadTexture(this);
        }

        public Icon(Icon icon, UIScale size, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground) 
            : this(size, icon._spritesheetPosition, icon._spritesheet, withBackground, backgroundType) { }

        public override void OnClick()
        {
            base.OnClick();
        }

        public void SetCameraPerspective(bool camPerspective) 
        {
            BaseObjects.ForEach(b =>
            {
                b.BaseFrame.CameraPerspective = camPerspective;
            });
        }
    }
}
