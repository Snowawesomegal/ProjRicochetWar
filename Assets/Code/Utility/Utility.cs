using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static bool EquivalentSigns(float numb1, float numb2)
    {
        if (numb1 == 0 && numb2 == 0)
            return true;
        if (numb1 > 0 && numb2 > 0)
            return true;
        if (numb1 < 0 && numb2 < 0)
            return true;
        return false;
    }
}
