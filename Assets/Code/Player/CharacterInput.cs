using System;
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

    [Flags]
    public enum InputType
    {
        DIRECTIONAL = 1,
        BUTTON = 2
    }
    public const InputType COMPOSITE_INPUT_TYPE = (InputType.DIRECTIONAL | InputType.BUTTON);

    [SerializeField] private ControlLock.Controls control;
    public ControlLock.Controls Control {
        get { return control; }
        private set {
            if (!ValidateControl(value))
            {
                Debug.LogError("Error creating CharacterInput because control has no valid flags: " + value.ToString());
                value = 0;
            }
            CompositeInput = ValidateCompositeControl(value);
            control = value;
        }
    }

    [SerializeField] private bool compositeInput = false;
    public bool CompositeInput { get { return compositeInput; } private set { compositeInput = value; } }

    [SerializeField] private float latestFrameIncrement;
    [SerializeField] private int duration = 0;
    public int Duration { get { return duration; } private set { duration = value; } }

    [SerializeField] private float durationTime = 0;
    public float DurationTime { get { return Phase == InputStage.HELD ? Time.fixedTime - inputTime : durationTime; } private set { durationTime = value; } }

    [SerializeField] private float inputTime;
    public float InputTime { get { return inputTime; } private set { inputTime = value; } }

    [SerializeField] private InputStage phase;
    public InputStage Phase { get { return phase; } private set { phase = value; } }

    [SerializeField] private Vector2 direction = Vector2.zero;
    public Vector2 Direction { get { return direction; } private set { direction = value; } }

    [SerializeField] private InputType controlType;
    public InputType ControlType { get { return controlType; } private set { controlType = value; } }

    public CharacterInput(ControlLock.Controls control, InputStage phase)
    {
        Control = control;
        Phase = phase;
        ControlType = InputType.BUTTON;
        InputTime = Time.fixedTime;
    }

    public CharacterInput(ControlLock.Controls control, InputStage phase, Vector2 direction)
    {
        // check the control type
        if (IsDirectionalControl(control) && !IsButtonControl(control))
        {
            ControlType = InputType.DIRECTIONAL;
        }
        else
        {
            ControlType = direction == Vector2.zero ? InputType.BUTTON : InputType.DIRECTIONAL;
        }

        // if the input vector is equivalent to 0 and the control type is button, 
        // remove directional input from the controls
        if (ControlType == InputType.BUTTON)
            control = control & (~ControlLock.DIRECTIONAL_CONTROLS);

        Control = control;
        Phase = phase;
        Direction = direction;

        // if it's composite input, fix the control type flags
        if (CompositeInput)
            ControlType = COMPOSITE_INPUT_TYPE;

        InputTime = Time.fixedTime;
    }

    public static bool ValidateControl(ControlLock.Controls control)
    {
        return (int)control > 0;
    }

    public static int GetNumbControls(ControlLock.Controls control)
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
        return numbControls;
    }

    public static bool IsDirectionalControl(ControlLock.Controls control)
    {
        return (control & ControlLock.DIRECTIONAL_CONTROLS) > 0;
    }

    public static bool IsButtonControl(ControlLock.Controls control)
    {
        return (control & ControlLock.BUTTON_CONTROLS) > 0;
    }

    /// <summary>
    /// Validates a given control to make sure that only one control flag is present
    /// in the variable.
    /// </summary>
    /// <param name="control"> the controls to check </param>
    /// <returns></returns>
    public static bool ValidateSingleControl(ControlLock.Controls control)
    {
        return GetNumbControls(control) == 1;
    }

    public static bool ValidateCompositeControl(ControlLock.Controls control)
    {
        int numbDirectionals = GetNumbControls(control & ControlLock.DIRECTIONAL_CONTROLS);
        int numbButtons = GetNumbControls(control & ControlLock.BUTTON_CONTROLS);
        return numbDirectionals >= 1 && numbButtons == 1;
    }

    public void UpdateInput(InputStage phase)
    {
        Phase = phase;
    }

    public void UpdateInput(InputStage phase, float durationTime)
    {
        Phase = phase;
        DurationTime = durationTime;
    }

    public bool IsDirectional()
    {
        return (ControlType | InputType.DIRECTIONAL) > 0;
    }

    public bool IsButtton()
    {
        return (ControlType | InputType.BUTTON) > 0;
    }

    public bool IsComposite()
    {
        return compositeInput;
    }

    public bool IsHeld()
    {
        return Phase == InputStage.HELD;
    }

    public bool IsReleased()
    {
        return Phase == InputStage.RELEASED;
    }

    public bool IncrementFrames()
    {
        if (latestFrameIncrement != Time.fixedTime)
        {
            duration++;
            latestFrameIncrement = Time.fixedTime;
            return true;
        }
        return false;
    }

    public bool TryIncrementFrames()
    {
        if (Phase == InputStage.HELD && latestFrameIncrement != Time.fixedTime)
        {
            duration++;
            latestFrameIncrement = Time.fixedTime;
            return true;
        }
        return false;
    }

    public bool CanCombineInputs(CharacterInput input)
    {
        return Phase != InputStage.RELEASED;
    }

    public CharacterInput CombineWith(CharacterInput input)
    {
        // take the time and Direction values of this object and the phase of the given object
        // both objects have the same Control
        CharacterInput target = new CharacterInput(Control, input.Phase, Direction);
        target.InputTime = InputTime;
        target.Duration = Duration;
        target.DurationTime = DurationTime;
        target.latestFrameIncrement = latestFrameIncrement;
        return target;
    }

    public static Pair<int, CharacterInput> CombineInputPair(Pair<int, CharacterInput> current, Pair<int, CharacterInput> next)
    {
        int leftVal = next.right.Phase == InputStage.RELEASED ? current.left : next.left;
        CharacterInput rightVal = current.right.CombineWith(next.right);
        return new Pair<int, CharacterInput>(leftVal, rightVal);
    }
        
}
