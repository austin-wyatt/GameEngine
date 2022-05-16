using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.ObjectDefinitions;
using Empyrean.Game.Objects.PropertyAnimations;
using Empyrean.Game.Tiles;
using Empyrean.Game.UI;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Units
{
    public static class UnitAnimations
    {
        public static async Task CreateStaminaRefillAnimation(Unit unit)
        {
            if (unit.Scene.Footer.CurrentUnit != unit) return;

            TaskCompletionSource<bool> animationSource = new TaskCompletionSource<bool>();

            int actionEnergy = (int)Math.Floor(unit.GetResF(ResF.ActionEnergy));

            float movementEnergy = unit.GetResF(ResF.MovementEnergy);

            int stamina = unit.GetResI(ResI.Stamina);
            int maxStamina = unit.GetResI(ResI.MaxStamina);

            int animationsStarted = 0;
            int animationsCompleted = 0;

            Vector4 projectileColor = _Colors.Tan - new Vector4(0, 0, 0, 0.75f);
            //Vector4 projectileColor = _Colors.Tan;
            float projectileScale = 0.05f;

            int currAnimStamina = stamina;

            if (stamina < maxStamina)
            {
                for (int i = actionEnergy - 1; i >= 0 && stamina < maxStamina; i--)
                {
                    int capturedStam = stamina;

                    #region create projectile
                    Vector3 initialPos = unit.Scene.ActionEnergyBar.Pips[i]._position;
                    Vector3 destination = unit.Scene.Footer._unitStaminaBar.Pips[stamina]._position;

                    initialPos.Z = -0.5f;
                    destination.Z = -0.5f;

                    GameObject arrow = new GameObject(Spritesheets.IconSheet, (int)IconSheetIcons.StaminaPip);
                    //GameObject arrow = new GameObject(Textures.Stone_1, 0);
                    arrow.BaseObject.BaseFrame.CameraPerspective = false;
                    arrow.ScaleXY(projectileScale, projectileScale * WindowConstants.AspectRatio);

                    arrow.BaseObject.EnableLighting = false;
                    arrow.BaseObject.RenderData.AlphaThreshold = 0.01f; 

                    arrow.SetColor(projectileColor);


                    GameObject.LoadTexture(arrow);

                    arrow.SetPosition(new Vector3(-1000, 0, 0));

                    TileMapManager.Scene._lowPriorityObjects.Add(arrow);

                    Random rng = new Random();
                    TrackingSimulation moveSim = new TrackingSimulation(initialPos, destination)
                    {
                        InitialDirection = (float)(rng.NextDouble() - MathHelper.Pi) * -2,
                        Acceleration = (float)rng.NextDouble() + 10f,
                        RotationPerTick = 0.1f,
                        MaximumVelocity = 35f,
                        StartDelay = 2
                    };

                    TrackingParticleAnimation anim = new TrackingParticleAnimation(arrow, initialPos, destination, sim: moveSim);

                    #endregion

                    unit.Scene.ActionEnergyBar.Pips[i].ChangeEnergyState(EnergyStates.Empty);

                    unit.PropertyAnimations.Add(anim);
                    anim.Play();
                    animationsStarted++;

                    anim.OnFinish += () =>
                    {
                        Sound sound;

                        if(rng.NextDouble() > 0.5)
                        {
                            sound = new Sound(Sounds.Pop1) { Gain = 0.05f };
                        }
                        else
                        {
                            sound = new Sound(Sounds.Pop2) { Gain = 0.05f };
                        }

                        sound.Play();

                        animationsCompleted++;

                        unit.PropertyAnimations.Remove(anim);

                        for(int j = capturedStam; j < unit.Scene.Footer._unitStaminaBar.Pips.Count 
                            && j < capturedStam + Unit.STAMINA_REGAINED_PER_ACTION; j++)
                        {
                            unit.Scene.Footer._unitStaminaBar.Pips[j].SetColor(StaminaBar.FULL_COLOR);
                            //unit.Scene.Footer._unitStaminaBar.Pips[currAnimStamina].SetColor(StaminaBar.FULL_COLOR);
                            currAnimStamina++;
                        }

                        TileMapManager.Scene._lowPriorityObjects.Remove(arrow);

                        if (animationsCompleted == animationsStarted)
                        {
                            animationSource.TrySetResult(true);
                        }
                    };

                    stamina += Unit.STAMINA_REGAINED_PER_ACTION;
                }


                while(movementEnergy >= Unit.MOVEMENT_PER_STAMINA && stamina < maxStamina)
                {
                    int capturedStam = stamina;

                    #region create projectile
                    Vector3 initialPos = unit.Scene.EnergyDisplayBar.Pips[(int)movementEnergy - Unit.MOVEMENT_PER_STAMINA]._position;
                    Vector3 destination = unit.Scene.Footer._unitStaminaBar.Pips[stamina]._position;

                    initialPos.Z = -0.5f;
                    destination.Z = -0.5f;

                    GameObject arrow = new GameObject(Spritesheets.IconSheet, (int)IconSheetIcons.StaminaPip);
                    arrow.BaseObject.BaseFrame.CameraPerspective = false;
                    arrow.ScaleXY(projectileScale, projectileScale * WindowConstants.AspectRatio);

                    arrow.BaseObject.EnableLighting = false;

                    arrow.SetColor(projectileColor);
                    arrow.BaseObject.RenderData.AlphaThreshold = 0.01f;


                    GameObject.LoadTexture(arrow);

                    arrow.SetPosition(new Vector3(-1000, 0, 0));

                    TileMapManager.Scene._lowPriorityObjects.Add(arrow);

                    Random rng = new Random();
                    TrackingSimulation moveSim = new TrackingSimulation(initialPos, destination)
                    {
                        InitialDirection = (float)(rng.NextDouble() - MathHelper.Pi) * -2,
                        Acceleration = (float)rng.NextDouble() + 10f,
                        RotationPerTick = 0.1f,
                        MaximumVelocity = 35f,
                        StartDelay = 2
                    };

                    TrackingParticleAnimation anim = new TrackingParticleAnimation(arrow, initialPos, destination, sim: moveSim);

                    #endregion

                    unit.Scene.EnergyDisplayBar.SetActiveEnergy(movementEnergy - Unit.MOVEMENT_PER_STAMINA);

                    unit.PropertyAnimations.Add(anim);
                    anim.Play();
                    animationsStarted++;

                    anim.OnFinish += () =>
                    {
                        Sound sound;

                        if (rng.NextDouble() > 0.5)
                        {
                            sound = new Sound(Sounds.Pop1) { Gain = 0.05f };
                        }
                        else
                        {
                            sound = new Sound(Sounds.Pop2) { Gain = 0.05f };
                        }

                        sound.Play();


                        animationsCompleted++;

                        unit.PropertyAnimations.Remove(anim);

                        unit.Scene.Footer._unitStaminaBar.Pips[capturedStam].SetColor(StaminaBar.FULL_COLOR);
                        //unit.Scene.Footer._unitStaminaBar.Pips[currAnimStamina].SetColor(StaminaBar.FULL_COLOR);
                        currAnimStamina++;

                        TileMapManager.Scene._lowPriorityObjects.Remove(arrow);

                        if (animationsCompleted == animationsStarted)
                        {
                            animationSource.TrySetResult(true);
                        }
                    };


                    movementEnergy -= Unit.MOVEMENT_PER_STAMINA;
                    stamina += 1;
                }

                if(animationsStarted == 0)
                {
                    animationSource.TrySetResult(true);
                }
            }
            else
            {
                animationSource.TrySetResult(true);
            }

            await animationSource.Task;
        }
    }
}
