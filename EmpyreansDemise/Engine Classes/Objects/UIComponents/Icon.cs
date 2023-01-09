using Empyrean.Game.Abilities;
using Empyrean.Game.Objects;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Empyrean.Engine_Classes.TextHandling;
using System.Linq;

namespace Empyrean.Engine_Classes.UIComponents
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
        BrokenMask = 12,
        Channel,
        MonkSmall,
        MonkBig,
        Circle,
        StaminaPip
    }
    public class Icon : UIObject
    {
        public enum BackgroundType 
        {
            NeutralBackground = 10,
            BuffBackground = 30,
            DebuffBackground = 50
        }

        public Spritesheet _spritesheet;
        public Enum _spritesheetPosition;

        private UIObject _background;

        public UIObject ChargeDisplay = null;

        public static UIScale DefaultIconSize = new UIScale(0.25f, 0.25f);
        public static IconSheetIcons DefaultIcon = IconSheetIcons.QuestionMark; //question mark icon

        public Icon(UIScale size, Enum spritesheetPosition, Spritesheet spritesheet, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground)
        {
            Size = size;
            Name = "Icon";
            _spritesheet = spritesheet;
            _spritesheetPosition = spritesheetPosition;

            Animation tempAnimation;

            RenderableObject window = new RenderableObject(new SpritesheetObject(Convert.ToInt32(spritesheetPosition), _spritesheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER_DEFERRED);

            //window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { window },
                Frequency = 0,
                Repeats = 0
            };

            //BaseObject windowObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
            //windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            //AddBaseObject(windowObj);
            //_baseObject = windowObj;

            UIBlock mainBlock = new UIBlock(new List<Animation>() { tempAnimation }, default, size);
            BaseComponent = mainBlock;

            mainBlock.SetAllInline(0);

            AddChild(mainBlock, -10);

            if (withBackground) 
            {
                RenderableObject background = new RenderableObject(new SpritesheetObject(Convert.ToInt32(backgroundType), Spritesheets.IconSheet, 2, 2).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER_DEFERRED);

                //window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
                tempAnimation = new Animation()
                {
                    Frames = new List<RenderableObject>() { background },
                    Frequency = 0,
                    Repeats = 0
                };

                //BaseObject backgroundObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
                //AddBaseObject(backgroundObj);

                UIBlock backgroundBlock = new UIBlock(new List<Animation>() { tempAnimation }, default, size);
                backgroundBlock.SetAllInline(0);

                AddChild(backgroundBlock, -20);

                _background = backgroundBlock;
            }

            SetSize(Size);

            ValidateObject(this);

            LoadTexture(this);
        }

        public Icon(UIScale size, List<Animation> animations, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground)
        {
            Size = size;
            Name = "Icon";

            Animation tempAnimation;

            UIBlock block = new UIBlock(animations, default, size);
            block.SetAllInline(0);

            BaseComponent = block;
            AddChild(block, -10);

            //BaseObject windowObj = new BaseObject(animations, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
            //windowObj.BaseFrame.CameraPerspective = CameraPerspective;

            //AddBaseObject(windowObj);
            //_baseObject = windowObj;

            if (withBackground)
            {
                RenderableObject background = new RenderableObject(new SpritesheetObject(Convert.ToInt32(backgroundType), Spritesheets.IconSheet, 2, 2).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER_DEFERRED);

                //window.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
                tempAnimation = new Animation()
                {
                    Frames = new List<RenderableObject>() { background },
                    Frequency = 0,
                    Repeats = 0
                };

                UIBlock backgroundBlock = new UIBlock(new List<Animation>() { tempAnimation }, default, size);
                backgroundBlock.SetAllInline(0);

                AddChild(backgroundBlock, -20);
                _background = backgroundBlock;

                //BaseObject backgroundObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "", new Vector3(), EnvironmentObjects.UIBlockBounds);
                //AddBaseObject(backgroundObj);
            }

            SetSize(Size);

            ValidateObject(this);

            LoadTexture(this);
        }

        //public Icon(Icon icon, UIScale size, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground) 
        //    : this(size, icon._spritesheetPosition, icon._spritesheet, withBackground, backgroundType) { }

        public Icon(Icon icon, UIScale size, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground)
            : this(size, icon.BaseComponent.BaseObject.Animations.Values.ToList(), withBackground, backgroundType) { }

        public override void OnClick()
        {
            base.OnClick();
        }

        public override void SetSize(UIScale size)
        {
            base.SetSize(size);

            if(_background != null)
            {
                _background.SetSize(size);
            }
        }

        public override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            base.SetColor(color, flag);

            BaseComponent.SetColor(color, flag);

            if(_background != null)
            {
                _background.SetColor(color, flag);
            }
        }

        public void AddChargeDisplay(Ability ability) 
        {
            UIScale textBoxSize = new UIScale(Size);
            textBoxSize *= 0.333f;

            string energyString = $"({ability.GetCharges().ToString("n1").Replace(".0", "")})";

            float textScale = 0.05f;


            Text energyCostBox = new Text(energyString, Text.DEFAULT_FONT, 16, Brushes.Black);
            energyCostBox.SetTextScale(textScale);

            UIScale textDimensions = energyCostBox.GetDimensions();

            if (textDimensions.X > textDimensions.Y)
            {
                energyCostBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
            }

            UIBlock energyCostBackground = new UIBlock();
            energyCostBackground.SetColor(_Colors.UILightGray);
            energyCostBackground.MultiTextureData.MixTexture = false;

            energyCostBackground.SetSize(textBoxSize);

            energyCostBackground.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.BottomLeft);
            energyCostBox.SetPositionFromAnchor(energyCostBackground.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

            if (ability.GetCharges() == 0) 
            {
                energyCostBackground.SetColor(_Colors.LessAggressiveRed);
            }

            energyCostBackground.Clickable = true;
            energyCostBackground.Click += (s, e) =>
            {
                if (ability.Scene._selectedAbility == null && ability.GetCharges() < ability.MaxCharges && ability.CanRecharge())
                {
                    ability.ApplyChargeRechargeCost();

                    ability.RestoreCharges(1);
                    ability.Scene.ActionEnergyBar.HoverAmount(0);
                    ability.Scene.Footer.RefreshFooterInfo(forceUpdate: true);
                }
                else if (ability.Scene._selectedAbility != null) 
                {
                    ability.Scene.DeselectAbility();
                }
            };

            energyCostBackground.Hover += (s) =>
            {
                if (ability.Scene._selectedAbility == null && ability.GetCharges() < ability.MaxCharges && ability.CanRecharge())
                {
                    ability.Scene.ActionEnergyBar.HoverAmount(Ability.ChargeRechargeActionCost);
                }
            };

            energyCostBackground.HoverEnd += (s) =>
            {
                if (ability.Scene._selectedAbility == null && ability.GetCharges() < ability.MaxCharges && ability.CanRecharge())
                {
                    ability.Scene.ActionEnergyBar.HoverAmount(0);
                }
            };

            string chargeTooltip = $"{ability.GetCharges()}/{ability.GetMaxCharges()} Charges";

            if (ability.GetCharges() < ability.GetMaxCharges()) 
            {
                chargeTooltip += $"\nRestore cost: {ability.ChargeRechargeCost}";
            }

            UIHelpers.AddTimedHoverTooltip(energyCostBackground, chargeTooltip, ability.Scene);


            energyCostBackground.AddChild(energyCostBox);

            AddChild(energyCostBackground, 50);

            ChargeDisplay = energyCostBox;
        }

        /// <summary>
        /// Creates a pattern of action point objects to indicate how many action points the ability costs
        /// </summary>
        public void AddActionCost(Ability ability) 
        {
            UIScale textBoxSize = new UIScale(Size);
            textBoxSize *= 0.16f;

            Vector3 pos = GetAnchorPosition(UIAnchorPosition.BottomRight) + new Vector3(-5, -10, -0.001f);

            for (int i = 0; i < ability.GetCost(ResF.ActionEnergy); i++) 
            {
                UIBlock actionCost = new UIBlock(default, null, default, (int)IconSheetIcons.Channel, true, false, Spritesheets.IconSheet);
                actionCost.SetColor(_Colors.White);


                actionCost.MultiTextureData.MixTexture = false;

                actionCost.SetSize(textBoxSize);

                actionCost.SetPositionFromAnchor(pos, UIAnchorPosition.BottomRight);

                actionCost.SetAllInline(1);

                actionCost.RenderAfterParent = true;

                AddChild(actionCost, 49);

                pos = actionCost.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(0, -2, -0.001f);
            }
        }

        public void AddComboIndicator(Ability ability) 
        {
            UIScale textBoxSize = new UIScale(Size);
            textBoxSize *= 0.1f;

            int comboSize = ability.GetComboSize();
            int posInCombo = ability.GetPositionInCombo();

            Vector3 pos = GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(-5, 8, 0);

            for (int i = comboSize - 1; i >= 0; i--) 
            {
                UIBlock comboIndicator = new UIBlock(default, null, default, (int)IconSheetIcons.Circle, true, false, Spritesheets.IconSheet);
                if (i == posInCombo)
                {
                    comboIndicator.SetColor(_Colors.LessAggressiveRed);
                }
                else 
                {
                    comboIndicator.SetColor(_Colors.White);
                }
                
                comboIndicator.MultiTextureData.MixTexture = false;
                comboIndicator.SetSize(textBoxSize);
                comboIndicator.SetPositionFromAnchor(pos, UIAnchorPosition.TopRight);

                AddChild(comboIndicator, 49);

                pos = comboIndicator.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(-2, 0, 0);
            }
        }
    }
}
