using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public class TickingSlowTime : SlowTime<TickingSlowTime>
    {
        private int framesPerTick;
        private int currentFrame;
        private int framesRemaining;
        private float speed;
        public System.Action OnTick;

        public override float Speed { get { return speed; } }
        public override bool Done { get { return framesRemaining <= 0; } }
        public override bool Frozen { get { return framesPerTick == 0; } }
        public int FramesPerTick { get { return framesPerTick; } }
        public int FramesRemaining { get { return framesRemaining; } }

        public TickingSlowTime(int framesPerTick, int frames)
        {
            if (framesPerTick < 0)
                framesPerTick = 0;

            this.framesPerTick = framesPerTick;
            this.framesRemaining = frames;
            this.currentFrame = 0;
            this.speed = framesPerTick == 0 ? 0 : 1 / framesPerTick;
        }
        public TickingSlowTime(int framesPerTick, int frames, System.Action OnTick) : this(framesPerTick, frames)
        {
            this.OnTick = OnTick;
        }

        public TickingSlowTime(float speed, int frames)
        {
            if (speed < 0)
                speed = 0;

            this.framesPerTick = (int)Mathf.Ceil(1 / speed);
            this.framesRemaining = frames;
            this.currentFrame = 0;
            this.speed = speed;
        }
        public TickingSlowTime(float speed, int frames, System.Action OnTick) : this(speed, frames)
        {
            this.OnTick = OnTick;
        }

        public void TickNextFrame()
        {
            currentFrame = -1;
        }

        public override bool Tick(bool frozen)
        {
            if (frozen)
                return false;

            framesRemaining--;
            if (framesPerTick == 0)
                return framesRemaining <= 0;

            currentFrame++;
            if (currentFrame % framesPerTick == 0)
            {
                currentFrame = 0;
                OnTick?.Invoke();
            }
            return framesRemaining <= 0;
        }

        public override int Compare(TickingSlowTime st1, TickingSlowTime st2)
        {
            int speed1 = st1.FramesPerTick == 0 ? int.MaxValue : st1.FramesPerTick;
            int speed2 = st2.FramesPerTick == 0 ? int.MaxValue : st2.FramesPerTick;
            int comparedSpeeds = speed2.CompareTo(speed1); // reversed because bigger should go in front (because bigger is slower)
            return comparedSpeeds != 0 ? comparedSpeeds : st1.framesRemaining.CompareTo(st2.framesRemaining);
        }

        public override int CompareTo(TickingSlowTime st)
        {
            return Compare(this, st);
        }
    }
}