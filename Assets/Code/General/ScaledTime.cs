using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TimeScaling
{
    public struct SlowTime
    {
        public float speed;
        public float endTime;
        public SlowTime(float speed, float duration)
        {
            if (speed < ScaledTime.MIN_TIME_SCALE)
                speed = ScaledTime.MIN_TIME_SCALE;
            this.speed = speed;
            this.endTime = Time.time + duration * speed;
        }
        public void ExtendDuration()
        {
            this.endTime += Time.deltaTime;
        }
    }

    public class ScaledTime
    {
        [SerializeField] public const float MIN_TIME_SCALE = 0;

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