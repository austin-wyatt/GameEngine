using Empyrean.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.ObjectDefinitions
{
    public abstract class ParticleSimulation
    {
        public List<Vector3> Points = new List<Vector3>();
        public abstract void Simulate();
    }

    public class TrackingSimulation : ParticleSimulation
    {
        Vector3 Start;
        Vector3 Destination;

        public float Acceleration = 1;
        public float InitialDirection = 0;
        public float RotationPerTick = 0.01f;
        public float MaximumVelocity = 20;
        public int StartDelay = 0;
        public int Timeout = 120;

        public TrackingSimulation(Vector3 start, Vector3 destination)
        {
            Start = start;
            Destination = destination;
        }

        public override void Simulate()
        {
            Points.Clear();

            float initialDistance = (Destination - Start).Length;

            float currDirection = InitialDirection;

            Vector3 velocity = new Vector3();
            Vector3 currAcceleration = new Vector3();

            float acceleration = Acceleration;

            currAcceleration.X = Acceleration * (float)MathHelper.Cos(currDirection);
            currAcceleration.Y = Acceleration * (float)MathHelper.Sin(currDirection);

            velocity.Add(ref velocity, ref currAcceleration);

            Vector3 currPoint = new Vector3(Start);

            Vector2 minMax = new Vector2(float.MaxValue, float.MinValue);

            int count = -1;

            while (true)
            {
                count++;

                Points.Add(currPoint);

                //velocity.Add(ref velocity, ref currAcceleration);

                currPoint.Add(ref currPoint, ref velocity);

                Vector3 lineToDest = Destination - currPoint;

                float distance = lineToDest.LengthFast;

                acceleration = Math.Clamp(initialDistance / distance * Acceleration, 0, MaximumVelocity);

                if (distance < 50f)
                {
                    Points.Add(Destination);
                    return;
                }

                //at 40 ticks per second, give up on the animation if it's going to take longer than Timeout / 40 seconds
                if(Points.Count > Timeout)
                {
                    return;
                }

                
                if (count < StartDelay)
                {
                    velocity.X = acceleration * (float)MathHelper.Cos(currDirection);
                    velocity.Y = acceleration * (float)MathHelper.Sin(currDirection);
                    continue;
                }

                lineToDest.Normalize();
                Vector3 velocityLine = new Vector3(velocity);
                velocityLine.Normalize();

                float dot = Vector3.Dot(lineToDest, velocityLine);
                float direction = Vector3.Dot(Vector3.Cross(velocityLine, lineToDest), new Vector3(0, 0, 1));
                float det = lineToDest.X * velocityLine.Y - lineToDest.Y * velocityLine.X; 

                float angle = (float)MathHelper.Atan2(det, dot) + MathHelper.Pi;

                minMax.X = minMax.X < angle ? minMax.X : angle;
                minMax.Y = minMax.Y > angle ? minMax.Y : angle;
                
                if(direction < 0)
                {
                    //currDirection -= RotationPerTick;

                    currDirection -= Math.Clamp(Math.Abs(angle), 0, RotationPerTick);
                }
                else if (direction > 0)
                {
                    //currDirection += RotationPerTick;

                    currDirection += Math.Clamp(Math.Abs(angle), 0, RotationPerTick);
                }

                velocity.X = acceleration * (float)MathHelper.Cos(currDirection);
                velocity.Y = acceleration * (float)MathHelper.Sin(currDirection);

                currAcceleration.X = 0;
                currAcceleration.Y = 0;
            }
        }
    }
}
