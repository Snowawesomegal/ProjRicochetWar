using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public enum SlowUpdateType
    {
        FIXED,
        FRAME
    }

    public class FloatingSlowTime : SlowTime
    {
        private SlowUpdateType updateType;
        private float speed;
        private float endTime;
        public override float Speed { get { return speed; } }
        public override bool Done { get { return updateType switch { SlowUpdateType.FIXED => Time.fixedTime >= endTime, _ => Time.time >= endTime }; } }
        public override bool Frozen { get { return speed == 0; } }
        public SlowUpdateType UpdateType { get { return updateType; } }

        public FloatingSlowTime(float speed, float duration) : this(speed, duration, SlowUpdateType.FIXED) { }
        public FloatingSlowTime(float speed, float duration, SlowUpdateType updateType)
        {
            this.updateType = updateType;
            this.speed = speed;
            this.endTime = updateType switch
            {
                SlowUpdateType.FIXED => Time.fixedTime + duration,
                _ => Time.time + duration
            };
        }

        public override bool Tick(bool frozen)
        {
            if (frozen)
            {
                endTime += updateType switch { SlowUpdateType.FIXED => Time.fixedDeltaTime, _ => Time.deltaTime };
                return false;
            }

            float currentTime = updateType switch { SlowUpdateType.FIXED => Time.fixedTime, _ => Time.time };
            return currentTime >= endTime;
        }
    }
}