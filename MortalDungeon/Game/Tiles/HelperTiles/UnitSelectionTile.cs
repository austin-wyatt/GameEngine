using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.GameObjects
{
    public class UnitSelectionTile : GameObject
    {
        public Unit BoundUnit;
        public Vector3 UnitOffset;

        public PropertyAnimation _selectAnimation;
        public PropertyAnimation _targetAnimation;

        private Vector4 _baseColor;

        public enum UnitSelectionAnimations 
        {
            Select,
            Target
        }

        public UnitSelectionTile(Unit unit, Vector3 positionOffset)
        {
            Name = "UnitSelectionTile " + unit.ObjectID;
            BoundUnit = unit;
            UnitOffset = positionOffset;


            RenderableObject idle1 = new RenderableObject(new SpritesheetObject(17, Spritesheets.TestSheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
            RenderableObject idle2 = new RenderableObject(new SpritesheetObject(18, Spritesheets.TestSheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { idle1, idle2 },
                Frequency = 45,
                Repeats = -1,
                GenericType = (int)UnitSelectionAnimations.Target
            };

            RenderableObject select = new RenderableObject(new SpritesheetObject(27, Spritesheets.TestSheet).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            Animation Select = new Animation()
            {
                Frames = new List<RenderableObject>() { select },
                Frequency = 0,
                Repeats = -1,
                GenericType = (int)UnitSelectionAnimations.Select
            };


            List<Animation> List = new List<Animation>()
            {
                Idle,
                Select
            };

            BaseObject unitSelection = new BaseObject(List, 0, "UnitSelectionTile", unit.Position, EnvironmentObjects.BASE_TILE.Bounds);
            unitSelection.BaseFrame.CameraPerspective = true;

            AddBaseObject(unitSelection);

            SetPosition(unit.Position + UnitOffset);
            //base.SetScale(1 / WindowConstants.AspectRatio, 1, 1);

            VisionManager.Scene.Tick += Tick;
        }

        public override void CleanUp()
        {
            base.CleanUp();

            VisionManager.Scene.Tick -= Tick;
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position + UnitOffset);
        }

        public override void SetColor(Vector4 color, SetColorFlag setColorFlag = SetColorFlag.Base)
        {
            base.SetColor(color, setColorFlag);

            _baseColor = color;

            CreateAnimations();
        }

        public void Select() 
        {
            _selectAnimation.Reset();

            SetRender(true);
            SetPosition(BoundUnit.Position);

            _selectAnimation.Play();
            _selectAnimation.BaseColor = _baseColor;
            _selectAnimation.BaseTranslation = BaseObjects[0].BaseFrame.Transformations.ExtractTranslation();

            BaseObjects[0].SetAnimation((int)UnitSelectionAnimations.Select);
        }

        public void Deselect()
        {
            SetRender(false);
            _selectAnimation.Reset();
        }

        public void Target() 
        {
            _targetAnimation.Reset();

            SetRender(true);
            SetPosition(BoundUnit.Position);

            _targetAnimation.Play();
            _targetAnimation.BaseColor = _baseColor;
            _targetAnimation.BaseTranslation = BaseObjects[0].BaseFrame.Transformations.ExtractTranslation();

            BaseObjects[0].SetAnimation((int)UnitSelectionAnimations.Target);
        }

        public void Untarget() 
        {
            SetRender(false);
            _targetAnimation.Reset();
        }


        //initialize the select and target property animations
        private void CreateAnimations() 
        {
            PropertyAnimation selectColorShift = new PropertyAnimation(BaseObjects[0].BaseFrame) { Repeat = true };

            Vector4 shiftedColor = _baseColor - new Vector4(0.15f, 0.15f, 0.15f, 0);

            int shiftDelay = 5;
            int shifts = 20;

            Vector4 deltaColor = (_baseColor - shiftedColor) / (shifts / 2);

            Vector4 currColor = new Vector4(_baseColor);

            PropertyAnimations.Clear();

            for (int i = 0; i < shifts; i++)
            {
                Keyframe temp = new Keyframe(i * shiftDelay);

                temp.Action = () =>
                {
                    if (temp.ActivationTick == 0) 
                    {
                        currColor = new Vector4(_selectAnimation.BaseColor);
                        //base.SetScale(1 / WindowConstants.AspectRatio, 1, 1);
                        base.SetScale(1, 1, 1);
                    }

                    if (temp.ActivationTick < shiftDelay * shifts / 2)
                    {
                        currColor -= deltaColor;
                        ScaleAddition(-0.005f);
                    }
                    else
                    {
                        currColor += deltaColor;
                        ScaleAddition(0.005f);
                    }

                    base.SetColor(currColor);
                };

                selectColorShift.Keyframes.Add(temp);
            }

            PropertyAnimations.Add(selectColorShift);

            _selectAnimation = selectColorShift;


            int shiftDelayTarget = 10;
            int shiftsTarget = 20;

            PropertyAnimation targetColorShift = new PropertyAnimation(BaseObjects[0].BaseFrame) { Repeat = true };

            deltaColor = (_baseColor - shiftedColor) / (shiftsTarget / 2);

            Vector4 tarCurrColor = new Vector4(_baseColor);

            for (int i = 0; i < shiftsTarget; i++)
            {
                Keyframe temp = new Keyframe(i * shiftDelayTarget);

                temp.Action = () =>
                {
                    if (temp.ActivationTick == 0)
                    {
                        tarCurrColor = new Vector4(_targetAnimation.BaseColor);
                    }

                    if (temp.ActivationTick < shiftDelayTarget * shiftsTarget / 2)
                    {
                        tarCurrColor -= deltaColor;
                    }
                    else
                    {
                        tarCurrColor += deltaColor;
                    }

                    base.SetColor(tarCurrColor);
                };

                targetColorShift.Keyframes.Add(temp);
            }

            PropertyAnimations.Add(targetColorShift);


            _targetAnimation = targetColorShift;
        }
    }
}
