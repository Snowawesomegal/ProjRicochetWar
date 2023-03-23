using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public class FloatingTimeController : SlowTimeController<FloatingSlowTime>
    {
        public void Slow(float speed, float duration)
        {
            FloatingSlowTime st = new FloatingSlowTime(speed, duration, updateType);
            Slow(st);
        }

        public void Slow(float speed, float duration, IIdentifiable identifiable)
        {
            FloatingSlowTime st = new FloatingSlowTime(speed, duration, updateType);
            Slow(st, identifiable);
        }
    }
}
