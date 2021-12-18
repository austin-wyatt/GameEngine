using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Units;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;

namespace MortalDungeon.Game.UI
{
    internal enum EnergyStates
    {
        Empty,
        Energized,
        Flashing,
        PartiallyEnergized
    }

    internal class EnergyDisplayBar : UIObject
    {
        private const int MaxEnergy = 25;

        internal float CurrentMaxEnergy = 10;
        internal float CurrentEnergy = 10;
        internal float EnergyHovered = 0;
        internal List<EnergyPip> Pips = new List<EnergyPip>(MaxEnergy);

        CombatScene Scene;

        //todo, add a onHover tooltip to display exact energy amount

        internal EnergyDisplayBar(CombatScene scene, Vector3 position, UIScale size, int maxEnergy = 10, int spriteSheetPos = (int)IconSheetIcons.WalkingBoot, Spritesheet spriteSheet = null)
        {
            Position = position;
            Size = size;
            Name = "EnergyDisplayBar";
            CameraPerspective = false;
            Scene = scene;

            CurrentEnergy = maxEnergy;
            CurrentMaxEnergy = maxEnergy;

            Hoverable = true;
            Clickable = true;

            float aspectRatio = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;
            UIScale ScaleFactor = new UIScale(Size.X, Size.Y);
            SetOrigin(aspectRatio, ScaleFactor);

            float pipWidth = 0;
            float padding = 2;

            if (spriteSheet == null) 
            {
                spriteSheet = Spritesheets.IconSheet;
            }
            

            for (int i = 0; i < MaxEnergy; i++)
            {
                EnergyPip energyPip = new EnergyPip(new Vector3(Position.X + (pipWidth + padding) * i, Position.Y, 0), new UIScale(0.12f, 0.12f), spriteSheetPos, spriteSheet) { Clickable = true };
                pipWidth = energyPip.GetDimensions().X;
                energyPip.HoverAnimation.SetDefaultValues();

                energyPip.HasTimedHoverEffect = true;

                UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(Scene, CurrentEnergy.ToString("n1") + " Energy", energyPip, this);

                void timedHover(GameObject obj)
                {
                    param.Text = CurrentEnergy.ToString("n1") + " Energy";
                    UIHelpers.CreateToolTip(param);
                }

                energyPip.OnTimedHoverEvent += timedHover;

                energyPip.OnCleanUp += (_) =>
                {
                    energyPip.OnTimedHoverEvent -= timedHover;
                };

                Pips.Add(energyPip);

                if (i == 0) 
                {
                    BaseComponent = energyPip;
                }

                if (i > CurrentMaxEnergy)
                    energyPip.SetRender(false);

                AddChild(energyPip);
            }

            PropertyAnimation hoverColorShift = new PropertyAnimation(GetBaseObject().BaseFrame) { Repeat = true };

            Color shiftedColor = new Color(0.11f, 0.48f, 0.11f, 1);
            Color color = new Color(Pips[0].EnergizedColor);

            int shiftDelay = 2;
            int shifts = 30;

            Color deltaColor = (color - shiftedColor) / (shifts / 2);

            for (int i = 0; i < Pips.Count; i++) 
            {
                Pips[i]._baseObject.BaseFrame.AddAppliedColor(color);
            }

            for (int i = 0; i < shifts; i++) 
            {
                Keyframe temp = new Keyframe(i * shiftDelay);



                temp.Action = () =>
                {
                    if (temp.ActivationTick < shiftDelay * shifts / 2)
                    {
                        color.Sub(deltaColor);
                    }
                    else 
                    {
                        color.Add(deltaColor);
                    }

                    for (int j = 0; j < Pips.Count; j++) 
                    {
                        if (Pips[j].HoverAnimation.Finished) 
                        {
                            Pips[j]._baseObject.BaseFrame.CalculateInterpolatedColor();
                        }
                    }
                };

                hoverColorShift.Keyframes.Add(temp);
            }

            PropertyAnimations.Add(hoverColorShift);
            hoverColorShift.Play();
        }

        internal void SetActiveEnergy(float newEnergy)
        {
            CurrentEnergy = newEnergy > CurrentMaxEnergy ? CurrentMaxEnergy : newEnergy < 0 ? 0 : newEnergy;


            for (int i = 0; i < CurrentMaxEnergy; i++)
            {
                if (i < CurrentEnergy)
                {
                    float afterDecimal = CurrentEnergy - (float)Math.Truncate(CurrentEnergy);

                    if (i + 1 >= CurrentEnergy && afterDecimal != 0)
                    {
                        Pips[i].ChangeEnergyState(EnergyStates.PartiallyEnergized, CurrentEnergy - (float)Math.Truncate(CurrentEnergy));
                    }
                    else
                    {
                        Pips[i].ChangeEnergyState(EnergyStates.Energized);
                    }
                }
                else
                {
                    Pips[i].ChangeEnergyState(EnergyStates.Empty);
                }
            }
        }

        internal void SetMaxEnergy(float maxEnergy) 
        {
            CurrentMaxEnergy = maxEnergy;

            for (int i = 0; i < MaxEnergy; i++) 
            {
                if (i < maxEnergy)
                    Pips[i].SetRender(true);
                else
                    Pips[i].SetRender(false);
            }
        }

        internal void AddEnergy(float energy)
        {
            SetActiveEnergy(CurrentEnergy + (energy + 0.0001f));
        }

        private int _currentHoveredIndex = MaxEnergy;
        internal void HoverAmount(float energyToHover) 
        {
            if (energyToHover == 0) 
            {
                Pips.ForEach(pip => pip.EndHoverAnimation());
                return;
            }

            if (_currentHoveredIndex > CurrentMaxEnergy - 1)
                _currentHoveredIndex = (int)CurrentMaxEnergy - 1;

            int amountToHover = (int)(CurrentEnergy - energyToHover);

            if (CurrentEnergy - (int)CurrentEnergy < 0.01f) 
            {
                CurrentEnergy = (int)CurrentEnergy;
            }

            EnergyHovered = energyToHover;

            for (int i = (int)CurrentMaxEnergy - 1; i >= 0; i--) 
            {
                if (i < CurrentEnergy)
                {
                    if (i >= CurrentMaxEnergy)
                        i--;

                    if (i >= amountToHover && i >= 0)
                    {
                        if (!Pips[i].HoverAnimation.Finished)
                        {
                            Pips[i].PlayHoverAnimation();
                        }

                        _currentHoveredIndex = i;
                    }
                    else
                    {
                        Pips[i].EndHoverAnimation();
                    }
                }
                else 
                {
                    Pips[i].EndHoverAnimation();
                }
            }
        }

        //internal void SetEnergyFromUnit(Unit unit) 
        //{
        //    CurrentMaxEnergy = unit.MaxEnergy;
        //    CurrentEnergy = unit.CurrentEnergy;

        //    SetActiveEnergy(CurrentEnergy);
        //}
    }

    /// <summary>
    /// A hexagon energy pip
    /// </summary>
    internal class EnergyPip : UIObject
    {
        internal BaseObject Pip;
        //internal BaseObject Backdrop;
        internal Vector4 EnergizedColor = new Vector4(0.13f, 0.69f, 0.13f, 1);
        internal Vector4 EmptyColor = new Vector4(0.20f, 0.28f, 0.20f, 1);

        internal EnergyStates EnergyState = EnergyStates.Energized;

        internal PropertyAnimation HoverAnimation;

        internal EnergyPip(Vector3 position, UIScale size = default, int spriteSheetPos = 21, Spritesheet spriteSheet = null)
        {
            Position = position;
            Size = size == null ? Size : size;
            Name = "EnergyPip";
            CameraPerspective = false;

            Hoverable = true;

            Vector2i SpritesheetDimensions = new Vector2i(1, 1);

            Animation tempAnimation;
            float aspectRatio = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;

            UIScale ScaleFactor = new UIScale(Size.X, Size.Y);

            if(spriteSheet == null) 
            {
                spriteSheet = Spritesheets.TestSheet;
            }

            RenderableObject pip = new RenderableObject(new SpritesheetObject(spriteSheetPos, spriteSheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
            EnergizedColor = new Vector4(1, 1, 1, 1);


            pip.ScaleX(aspectRatio);
            pip.ScaleX(ScaleFactor.X);
            pip.ScaleY(ScaleFactor.Y);
            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { pip },
                Frequency = 0,
                Repeats = 0
            };

            BaseObject pipObj = new BaseObject(new List<Animation>() { tempAnimation }, 0, "EnergyPip", position, EnvironmentObjects.BASE_TILE.Bounds);
            pipObj.BaseFrame.CameraPerspective = CameraPerspective;

            AddBaseObject(pipObj);

            _baseObject = pipObj;

            Pip = pipObj;

            Pip.OutlineParameters.SetAllInline(1);

            SetOrigin(aspectRatio, ScaleFactor);

            HoverAnimation = new LiftAnimation(GetBaseObject(this).BaseFrame);

            PropertyAnimations.Add(HoverAnimation);

            ChangeEnergyState(EnergyState);
        }

        internal void ChangeEnergyState(EnergyStates state, float percent = 1)
        {
            EnergyState = state;

            switch (state)
            {
                case EnergyStates.Empty:
                    Pip.BaseFrame.SetBaseColor(EmptyColor);
                    break;
                case EnergyStates.Energized:
                    Pip.BaseFrame.SetBaseColor(EnergizedColor);
                    break;
                case EnergyStates.PartiallyEnergized:
                    Vector4 colorDif = EnergizedColor - EmptyColor;

                    Pip.BaseFrame.SetBaseColor(EnergizedColor - colorDif * (1 - percent));
                    break;
            }

            HoverAnimation.SetDefaultColor();
        }

        internal void PlayHoverAnimation()
        {
            HoverAnimation.Playing = true;
            _baseObject.BaseFrame.UseAppliedColors(true);
        }

        internal void EndHoverAnimation()
        {
            //HoverAnimation.SetDefaultColor();
            _baseObject.BaseFrame.UseAppliedColors(false);
            HoverAnimation.Reset();
        }
    }
}
