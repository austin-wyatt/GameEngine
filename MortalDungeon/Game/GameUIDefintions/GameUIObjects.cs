using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using MortalDungeon.Game.Objects.PropertyAnimations;

namespace MortalDungeon.Game.UI
{
    public class GameUIObjects
    {
        #region Enums
        public enum EnergyStates
        {
            Empty,
            Energized,
            Flashing
        }
        #endregion

        public class EnergyDisplayBar : UIObject 
        {
            private const int MaxEnergy = 25;

            public int CurrentMaxEnergy = 10;
            public int CurrentEnergy = 10;
            public List<EnergyPip> Pips = new List<EnergyPip>(MaxEnergy);

            public EnergyDisplayBar(Vector3 position, Vector2 size, int maxEnergy = 10) 
            {
                Position = position;
                Size = size;
                Name = "EnergyDisplayBar";
                CameraPerspective = false;

                CurrentEnergy = maxEnergy;
                CurrentMaxEnergy = maxEnergy;

                float aspectRatio = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;
                Vector2 ScaleFactor = new Vector2(Size.X, Size.Y);
                SetOrigin(aspectRatio, ScaleFactor);

                float pipWidth = 0;
                float padding = 2;

                for (int i = 0; i < CurrentMaxEnergy; i++) 
                {
                    EnergyPip energyPip = new EnergyPip(new Vector3(Position.X + (pipWidth + padding) * i, Position.Y, 0), new Vector2(0.12f, 0.12f)) { Clickable = true };
                    pipWidth = energyPip.GetDimensions().X;

                    energyPip.OnClickAction = () =>
                    {
                        energyPip.ChangeEnergyState(EnergyStates.Empty);
                    };

                    Pips.Add(energyPip);

                    AddChild(energyPip);
                }

            }

            public void SetActiveEnergy(int newEnergy) 
            {
                CurrentEnergy = newEnergy > CurrentMaxEnergy ? CurrentMaxEnergy : newEnergy < 0 ? 0 : newEnergy;

                for (int i = 0; i < CurrentMaxEnergy; i++) 
                {
                    if (i < CurrentEnergy)
                    {
                        Pips[i].ChangeEnergyState(EnergyStates.Energized);
                    }
                    else 
                    {
                        Pips[i].ChangeEnergyState(EnergyStates.Empty);
                    }
                }
            }

            public void AddEnergy(int energy) 
            {
                SetActiveEnergy(CurrentEnergy + energy);
            }
        }
        
        /// <summary>
        /// A hexagon energy pip with a dark backdrop
        /// </summary>
        public class EnergyPip : UIObject 
        {
            public BaseObject Pip;
            //public BaseObject Backdrop;
            public Vector4 EnergizedColor = new Vector4(0.13f, 0.69f, 0.13f, 1);
            public Vector4 EmptyColor = new Vector4(0.20f, 0.28f, 0.20f, 1);

            public EnergyStates EnergyState = EnergyStates.Energized;

            public EnergyPip(Vector3 position, Vector2 size = default)
            {
                Position = position;
                Size = size.X == 0 ? Size : size;
                Name = "EnergyPip";
                CameraPerspective = false;

                Hoverable = true;

                Vector2i SpritesheetDimensions = new Vector2i(1, 1);

                Animation tempAnimation;
                float aspectRatio = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;

                Vector2 ScaleFactor = new Vector2(Size.X, Size.Y);

                RenderableObject pip = new RenderableObject(new SpritesheetObject(21, Spritesheets.TestSheet, SpritesheetDimensions.X, SpritesheetDimensions.Y).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

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

                BaseObjects.Add(pipObj);
                Pip = pipObj;

                Pip.OutlineParameters.SetAllInline(1);

                SetOrigin(aspectRatio, ScaleFactor);
                ChangeEnergyState(EnergyState);

                PropertyAnimations.Add(new BounceAnimation(GetDisplay()));
            }

            public override void OnHover()
            {
                if (!Hovered && Hoverable)
                {
                    PlayBouncingAnimation();
                }
                base.OnHover();
            }

            public override void HoverEnd()
            {
                base.HoverEnd();

                EndBouncingAnimation();
            }

            public void ChangeEnergyState(EnergyStates state) 
            {
                EnergyState = state;

                switch (state) 
                {
                    case EnergyStates.Empty:
                        Pip.BaseFrame.SetColor(EmptyColor);
                        break;
                    case EnergyStates.Energized:
                        Pip.BaseFrame.SetColor(EnergizedColor);
                        break;
                }
            }

            public void PlayBouncingAnimation() 
            {
                GetPropertyAnimationByID((int)PropertyAnimationIDs.Bounce)?.Play();
            }

            public void EndBouncingAnimation()
            {
                GetPropertyAnimationByID((int)PropertyAnimationIDs.Bounce)?.Reset();
            }
        }
    }
}
