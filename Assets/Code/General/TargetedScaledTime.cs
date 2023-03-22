using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetedScaledTime : MonoBehaviour
{
    private ScaledTime generalTime = new ScaledTime();
    private List<Pair<Pair<float, float>, List<string>>> mappedSlowTimes = new List<Pair<Pair<float, float>, List<string>>>();
}
