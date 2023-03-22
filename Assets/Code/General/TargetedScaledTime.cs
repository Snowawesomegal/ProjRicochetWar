using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeScaling
{
    public class TargetedScaledTime : MonoBehaviour
    {
        private ScaledTime generalTime = new ScaledTime();
        private List<Pair<SlowTime, List<Identifiable>>> targetedSlowTimes = new List<Pair<SlowTime, List<Identifiable>>>();

        public bool Slowed { get; }
        public bool Speed { get; }
    }
}
