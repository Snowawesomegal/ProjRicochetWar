using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AngleMath
{
    /// <summary>
    /// Takes angle and returns the equivilent vector2 for the angle. Should probably be normalized before use.
    /// </summary>
    public static Vector2 Vector2FromAngle(float degreeAngle)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * degreeAngle), Mathf.Sin(Mathf.Deg2Rad * degreeAngle));
    }
}

public static class clmEx
{
    public static void RemoveAllLockersExcept(ControlLockManager clm, StandardControlLocker[] allLockers, StandardControlLocker[] except = null)
    {
        if (except == null)
        {
            clm.activeLockers.Clear();
        }
        else
        {
            foreach(StandardControlLocker i in allLockers)
            {
                if (System.Array.FindIndex(except, x => x == i) > -1)
                {
                    continue;
                }

                clm.RemoveLocker(i);
            }
        }
    }
}
