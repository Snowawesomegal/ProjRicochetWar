using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Control Locks/Standard Locker")]
public class StandardControlLocker : ScriptableObject, ControlLocker
{
    [SerializeField] private ControlLock controlLock;
    public ControlLock Lock { get { return controlLock; } }
}
