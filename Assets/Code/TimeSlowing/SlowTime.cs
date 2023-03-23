using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public abstract class SlowTime <T> : IComparer<T> where T : SlowTime<T>
    {
        public abstract float Speed { get; }
        public abstract bool Frozen { get; }
        public abstract bool Done { get; }
        /// <summary>
        /// Ticks the slow time, updating any necessary values to trigger events
        /// or change the state to Don.
        /// </summary>
        /// <param name="frozen"> If the slow time should 'update' or delay its end time. When frozen is
        /// true, it delays the time this SlowTime is done. </param>
        /// <returns> True if this SlowTime is finished slowing and should be removed. </returns>
        public abstract bool Tick(bool frozen);
        public abstract int Compare(T st1, T st2);
        public abstract int CompareTo(T st);
    }
}