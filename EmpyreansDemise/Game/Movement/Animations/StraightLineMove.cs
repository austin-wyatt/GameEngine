using Empyrean.Engine_Classes;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Movement.Animations
{
    public class StraightLineMove : MoveAnimation
    {
        public int MoveDelay = 1;

        public StraightLineMove(MoveContract contract) : base(contract) { }

        public override async Task EnactMovement(Unit objToMove)
        {
            if (MoveContract.Moves.Count == 0)
                return;

            TaskCompletionSource<bool> moveTask = new TaskCompletionSource<bool>();

            Vector3 diff = MoveContract.Moves[^1].Destination._position - MoveContract.Moves[0].Source._position;

            Vector3 basePos = MoveContract.Moves[0].Source._position;

            float len = diff.Length;

            const float STEPS_PER_UNIT_DISTANCE = 50;

            float steps = (int)(len / STEPS_PER_UNIT_DISTANCE);

            diff.X /= steps;
            diff.Y /= steps;

            PropertyAnimation anim = new PropertyAnimation();

            int tileCount = MoveContract.Moves.Count;
            int tileIndex = 0;

            float tileStepOffset = steps / tileCount;
            float stepsReciprocal = 1 / (float)Math.Truncate(steps);

            for (int i = 0; i < steps; i++)
            {
                Keyframe frame = new Keyframe(MoveDelay * i);

                int capturedIndex = i;
                float lerpedIndex = GMath.LnLerp(0, steps, i * stepsReciprocal);

                frame.Action = () =>
                {
                    if (lerpedIndex > tileStepOffset * tileIndex)
                    {
                        objToMove.SetTileMapPosition(MoveContract.Moves[tileIndex].Destination);

                        tileIndex++;
                    }

                    objToMove.SetPositionOffset(new Vector3(basePos.X + diff.X * lerpedIndex, basePos.Y + diff.Y * lerpedIndex, objToMove.Info.TileMapPosition._position.Z));
                };

                anim.Keyframes.Add(frame);
            }

            anim.Play();

            Window.Scene.HighFreqTick += anim.Tick;

            anim.OnFinish += () =>
            {
                Window.Scene.HighFreqTick -= anim.Tick;
                objToMove.SetPositionOffset(MoveContract.Moves[^1].Destination._position);
                objToMove.SetTileMapPosition(MoveContract.Moves[^1].Destination);

                moveTask.TrySetResult(true);
            };

            await moveTask.Task;
        }
    }
}
