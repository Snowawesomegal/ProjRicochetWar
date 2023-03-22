using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiablesManager : MonoBehaviour
{
    private static uint nextID = 0;
    public static uint AssignID(Identifiable identifiable)
    {
        identifiable.InitializeID(GetNextID());
        return identifiable.ID;
    }
    public static uint GetNextID()
    {
        return nextID++;
    }
}
