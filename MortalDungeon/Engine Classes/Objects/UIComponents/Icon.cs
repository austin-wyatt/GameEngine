﻿using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal enum IconSheetIcons
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
        Circle
    }
    internal class Icon : UIObject
    {
        internal enum BackgroundType 
        {
            NeutralBackground = 10,
            BuffBackground = 30,
            DebuffBackground = 50
        }

        internal Spritesheet _spritesheet;
        internal Enum _spritesheetPosition;

        internal UIObject ChargeDisplay = null;

        internal static UIScale DefaultIconSize = new UIScale(0.25f, 0.25f);
        internal static IconSheetIcons DefaultIcon = IconSheetIcons.QuestionMark; //question mark icon

        internal Icon(UIScale size, Enum spritesheetPosition, Spritesheet spritesheet, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground)
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

            AddBaseObject(windowObj);
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
                AddBaseObject(backgroundObj);
                //UIObject iconBackground = new UIObject();
                //iconBackground.Name = "IconBackground";
                //iconBackground._baseObject = backgroundObj;
                //iconBackground.AddBaseObject(backgroundObj);

                //BaseComponent = iconBackground;
                //_baseObject = null;
                //BaseObjects.Clear();


                //AddChild(iconBackground);
            }

            SetSize(Size);

            ValidateObject(this);

            LoadTexture(this);
        }

        internal Icon(Icon icon, UIScale size, bool withBackground = false, BackgroundType backgroundType = BackgroundType.NeutralBackground) 
            : this(size, icon._spritesheetPosition, icon._spritesheet, withBackground, backgroundType) { }

        internal override void OnClick()
        {
            base.OnClick();
        }

        internal void SetCameraPerspective(bool camPerspective) 
        {
            BaseObjects.ForEach(b =>
            {
                b.BaseFrame.CameraPerspective = camPerspective;
            });
        }

        internal void AddChargeDisplay(Ability ability) 
        {
            UIScale textBoxSize = new UIScale(Size);
            textBoxSize *= 0.333f;

            string energyString = $"({ability.GetCharges().ToString("n1").Replace(".0", "")})";

            float textScale = 0.05f;


            TextComponent energyCostBox = new TextComponent();
            energyCostBox.SetColor(Colors.UITextBlack);
            energyCostBox.SetText(energyString);
            energyCostBox.SetTextScale(textScale);

            UIScale textDimensions = energyCostBox.GetDimensions();

            if (textDimensions.X > textDimensions.Y)
            {
                energyCostBox.SetTextScale((textScale - 0.004f) * textDimensions.Y / textDimensions.X);
            }

            UIBlock energyCostBackground = new UIBlock();
            energyCostBackground.SetColor(Colors.UILightGray);
            energyCostBackground.MultiTextureData.MixTexture = false;

            energyCostBackground.SetSize(textBoxSize);

            energyCostBackground.SetPositionFromAnchor(GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.BottomLeft);
            energyCostBox.SetPositionFromAnchor(energyCostBackground.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);

            if (ability.GetCharges() == 0) 
            {
                energyCostBackground.SetColor(Colors.LessAggressiveRed);
            }

            energyCostBackground.Clickable = true;
            energyCostBackground.OnClickAction = () =>
            {
                if (ability.Scene._selectedAbility == null && ability.GetCharges() < ability.MaxCharges && ability.CanRecharge())
                {
                    ability.ApplyChargeRechargeCost();

                    ability.RestoreCharges(1);
                    ability.Scene.ActionEnergyBar.HoverAmount(0);
                    ability.Scene.Footer.UpdateFooterInfo();
                }
                else if (ability.Scene._selectedAbility != null) 
                {
                    ability.Scene.DeselectAbility();
                }
            };

            energyCostBackground.OnHoverEvent += (_) =>
            {
                if (ability.Scene._selectedAbility == null && ability.GetCharges() < ability.MaxCharges && ability.CanRecharge())
                {
                    ability.Scene.ActionEnergyBar.HoverAmount(Ability.ChargeRechargeActionCost);
                }
            };

            energyCostBackground.OnHoverEndEvent += (_) =>
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
        internal void AddActionCost(Ability ability) 
        {
            UIScale textBoxSize = new UIScale(Size);
            textBoxSize *= 0.16f;

            Vector3 pos = GetAnchorPosition(UIAnchorPosition.BottomRight) + new Vector3(-5, -10, -0.001f);

            for (int i = 0; i < ability.ActionCost; i++) 
            {
                UIBlock actionCost = new UIBlock(default, null, default, (int)IconSheetIcons.Channel, true, false, Spritesheets.IconSheet);
                actionCost.SetColor(Colors.White);


                actionCost.MultiTextureData.MixTexture = false;

                actionCost.SetSize(textBoxSize);

                actionCost.SetPositionFromAnchor(pos, UIAnchorPosition.BottomRight);

                actionCost.SetAllInline(1);

                actionCost.RenderAfterParent = true;

                AddChild(actionCost, 49);

                pos = actionCost.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(0, -2, -0.001f);
            }
        }

        internal void AddComboIndicator(Ability ability) 
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
                    comboIndicator.SetColor(Colors.LessAggressiveRed);
                }
                else 
                {
                    comboIndicator.SetColor(Colors.White);
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
