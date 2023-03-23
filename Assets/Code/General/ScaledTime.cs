using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TimeScaling
{
    public struct SlowTime
    {
        public int framesPerTick;
        public int remainingFrames;
        private int currentFrame;
        public float speed;
        // TODO: create a tick-based and a time-based version of slow time
        public bool Frozen { get { return framesPerTick == 0; } }
        public bool Done { get { return currentFrame <= 0; } }

        public SlowTime(int framesPerTick, int remainingFrames)
        {
            if (framesPerTick < 0)
                framesPerTick = 0;

            this.framesPerTick = framesPerTick;
            this.remainingFrames = remainingFrames;
            this.speed = 1 / framesPerTick;
            this.currentFrame = 0;
        }

        public SlowTime(float speed, float duration)
        {
            if (speed <= 0)
                speed = 0;

            this.framesPerTick = speed != 0 ? (int)Mathf.Ceil(1 / speed) : 0;
            this.remainingFrames = (int)Mathf.Ceil(duration / Application.targetFrameRate);
            this.speed = speed;
            this.currentFrame = 0;
        }

        public bool Tick()
        {
            remainingFrames--;

            if (framesPerTick == 0)
                return remainingFrames <= 0;

            currentFrame++;
            if (currentFrame >= framesPerTick)
            {
                currentFrame = 0;
                return true;
            }
            return false;
        }
    }

    public class ScaledTime
    {
        [SerializeField] private List<SlowTime> slowTimes = new List<SlowTime>();
        [SerializeField] private bool slowed;
        [SerializeField] private float speed;
        public bool Slowed { get { return slowed; } }
        public float Speed { get { return slowed ? speed : 1f; } }

        public System.Func<bool> Frozen;

        public ScaledTime()
        {

        }

        public virtual void Slow(float speed, float duration)
        {
            SlowTime slowTime = new SlowTime(speed, duration);
            slowTimes.Add(slowTime);
            if (slowTimes.Count > 1)
                slowTimes.Sort(SlowTimeComparator);

            this.speed = slowTimes[0].speed;
            this.slowed = true;
            Time.timeScale = this.speed;
        }

        public int SlowTimeComparator(SlowTime sl1, SlowTime sl2)
        {
            int compare1 = sl1.speed.CompareTo(sl2.speed);
            if (compare1 != 0)
                return compare1;
            return sl1.endTime.CompareTo(sl2.endTime); ;
        }

        public virtual void UpdateSlowTime()
        {
            if (!slowed)
                return;

            if (Frozen.Invoke())
            {
                slowTimes.ForEach(st => st.ExtendDuration());
            }

            while (slowTimes.Count > 0 && Time.time >= slowTimes[0].endTime)
            {
                slowTimes.RemoveAt(0);
                if (slowTimes.Count == 0)
                {
                    this.slowed = false;
                    Time.timeScale = 1f;
                }
                else
                {
                    this.speed = slowTimes[0].speed;
                    Time.timeScale = slowTimes[0].speed;
                }
            }
        }
    }
}