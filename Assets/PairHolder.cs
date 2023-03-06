using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairHolder : MonoBehaviour
{
    [SerializeField] public Pair<string, ControlLock.Controls> coolPair = new Pair<string, ControlLock.Controls>("hi", ControlLock.Controls.ATTACK);
}
