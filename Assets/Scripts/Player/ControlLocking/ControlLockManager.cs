using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlLockManager : MonoBehaviour
{
    /// <summary>
    /// Used to track the active ControlLockers for this manager.
    /// </summary>
    [SerializeField] private List<ControlLocker> activeLockers = new List<ControlLocker>();
    /// <summary>
    /// Used to reference the composite of control locks made by
    /// combining the locks from all active ControlLockers.
    /// </summary>
    public ControlLock.Controls CompositeLock { 
        get { 
            ControlLock.Controls controls = ControlLock.Controls.NONE;
            foreach (ControlLocker cl in activeLockers)
            {
                controls |= cl.Lock.Locks;
            }
            return controls;
        }
    }

    /// <summary>
    /// Adds a designated ControlLocker to the list of active lockers.
    /// This will add all locks present in the ControlLocker.Lock object
    /// to the CompositeLock of this object.
    /// </summary>
    /// <param name="cl"> the ControlLocker to add </param>
    public void AddLocker(ControlLocker cl)
    {
        activeLockers.Add(cl);
    }

    /// <summary>
    /// Removes a designated ControlLocker from the list of active lockers.
    /// This will potentially remove all locks present in the ControlLocker.Lock
    /// object from the CompositeLock of this object. If there are other lockers
    /// that lock certain controls, then those controls will remain locked until
    /// the corresponding lockers are also removed.
    /// </summary>
    /// <param name="cl"> the ControlLocker to remove </param>
    /// <returns> true if the ControlLocker was successfully removed </returns>
    public bool RemoveLocker(ControlLocker cl)
    {
        return activeLockers.Remove(cl);
    }

    /// <summary>
    /// Used to check if all of the designated controls are compatible with
    /// the currently locked input in the CompositeLock of this object.
    /// </summary>
    /// <param name="controls"> the controls to be checked for compatibility </param>
    /// <returns> 
    /// true if the CompositeLock doesn't block any of the controls passed in 
    /// </returns>
    public bool ControlsAllowed(ControlLock.Controls controls)
    {
        return !ControlLock.IsLocking(controls, CompositeLock);
    }
}
