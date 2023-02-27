using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ControlLock
{
    [Flags]
    public enum Controls
    {
        NONE = 0,
        HORIZONTAL = 1,
        VERTICAL = 2,
        JUMP = 4,
        ATTACK = 8,
        SPECIAL = 16,
        DASH = 32,
        HEAVY = 64,
        MOVEMENT = 128
    }
    public const Controls DIRECTIONAL_CONTROLS = (Controls.HORIZONTAL | Controls.VERTICAL);
    public const Controls BUTTON_CONTROLS = (Controls.JUMP | Controls.ATTACK | Controls.SPECIAL);

    /// <summary>
    ///  Locks property is used to track which controls this ControlLock corresponds to.
    /// </summary>
    [SerializeField] private Controls locks;
    public Controls Locks { get { return locks; } private set { locks = value; } }

    /// <summary>
    /// Priority property is used to track the priority of this lock. This becomes relevant
    /// when multiple locks are going into affect that conflict with each other.
    /// </summary>
    [SerializeField] private int priority;
    public int Priority { get { return priority; } private set { priority = value; } }

    public ControlLock()
    {
        locks = 0;
    }

    public ControlLock(Controls locks)
    {
        this.locks = locks;
    }

    /// <summary>
    /// Checks if any of the target controls would be locked by 
    /// this object's locks.
    /// </summary>
    /// <param name="target"> the target controls that might be locked </param>
    /// <returns> true only if any of the target controls are affected </returns>
    public bool IsLocking(Controls target)
    {
        return (target & locks) > 0;
    }

    /// <summary>
    /// Checks if any of the target controls would be locked by 
    /// the designated locker.
    /// </summary>
    /// <param name="target"> the target controls that might be locked </param>
    /// <param name="locker"> the target locker which will be used as a reference </param>
    /// <returns> true only if any of the target controls are affected </returns>
    public static bool IsLocking(Controls target, Controls locker)
    {
        return (target & locker) > 0;
    }

    /// <summary>
    /// Check if all of the target controls would be locked by
    /// this object's locks.
    /// </summary>
    /// <param name="target"> the target controls that might be locked </param>
    /// <returns> true only if this object's locks affect all of the controls in target </returns>
    public bool IsLockingAll(Controls target)
    {
        return (target & locks) == target;
    }

    /// <summary>
    /// Check if all of the target controls would be locked by
    /// thie designated locker.
    /// </summary>
    /// <param name="target"> the target controls that might be locked </param>
    /// <param name="locker"> the target locker which will be used as a reference </param>
    /// <returns> true only if this object's locks affect all of the controls in target </returns>
    public static bool IsLockingAll(Controls target, Controls locker)
    {
        return (target & locker) == target;
    }
}
