using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public class TickingTimeController : SlowTimeController<TickingSlowTime>
    {
        public TickingTimeController() : base() { }
        public TickingTimeController(SlowUpdateType updateType) : base(updateType) { }

        public void Slow(int framesPerTick, int duration)
        {
            TickingSlowTime st = new TickingSlowTime(framesPerTick, duration);
            Slow(st);
        }

        public void Slow(int framesPerTick, int duration, IIdentifiable identifiable)
        {
            TickingSlowTime st = new TickingSlowTime(framesPerTick, duration);
            Slow(st, identifiable);
        }

        public void Slow(int framesPerTick, int duration, IIdentifiable identifiable, System.Action OnTick)
        {
            TickingSlowTime st = new TickingSlowTime(framesPerTick, duration, OnTick);
            Slow(st, identifiable);
        }

        public void Slow(float speed, int duration)
        {
            TickingSlowTime st = new TickingSlowTime(speed, duration);
            Slow(st);
        }

        public void Slow(float speed, int duration, IIdentifiable identifiable)
        {
            TickingSlowTime st = new TickingSlowTime(speed, duration);
            Slow(st, identifiable);
        }

        public void Slow(float speed, int duration, IIdentifiable identifiable, System.Action OnTick)
        {
            TickingSlowTime st = new TickingSlowTime(speed, duration, OnTick);
            Slow(st, identifiable);
        }
    }
}
