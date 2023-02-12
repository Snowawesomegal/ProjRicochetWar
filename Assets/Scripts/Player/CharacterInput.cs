using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput
{
    public enum InputStage
    {
        HELD,
        RELEASED
    }

    private ControlLock.Controls control;
    public ControlLock.Controls Control {
        get { return control; }
        private set {
            if (!ValidateSingleControl(value))
            {
                Debug.LogError("Error creating CharacterInput because control has more than 1 flag: " + value.ToString());
                value = 0;
            }
            control = value;
        }
    }

    private float duration;
    public float Duration { get { return duration; } private set { duration = value; } }

    private float inputTime;
    public float InputTime { get { return inputTime; } private set { inputTime = value; } }

    private InputStage phase;
    public InputStage Phase { get { return phase; } private set { phase = value; } }

    public CharacterInput(ControlLock.Controls control)
    {
        Control = control;
    }

    /// <summary>
    /// Validates a given control to make sure that only one control flag is present
    /// in the variable.
    /// </summary>
    /// <param name="control"> the controls to check </param>
    /// <returns></returns>
    public static bool ValidateSingleControl(ControlLock.Controls control)
    {
        int numbControls = 0;
        int comp = 1;
        int temp = (int)control;
        while (temp > 0)
        {
            if ((temp & comp) == comp)
            {
                numbControls++;
            }
            temp = temp >> 1;
        }
        return numbControls <= 1;
    }
}
