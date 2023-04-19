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

    public enum InputProcessStage
    {
        PENDING,
        PROCESSING,
        INTERRUPTED
    }

    [Flags]
    public enum InputType
    {
        DIRECTIONAL = 1,
        BUTTON = 2
    }

    public enum CardinalDirection
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT,
        NONE
    }



    [Serializable]
    public struct DirectedInput
    {
        [SerializeField] public CardinalDirection cardinalInput;
        [SerializeField] public Vector2 starting;
        [SerializeField] public Vector2 current;

        public DirectedInput(Vector2 starting)
        {
            this.cardinalInput = GetDirectionFromVector2(starting);
            this.starting = starting;
            this.current = starting;
        }

        public DirectedInput(CardinalDirection cardinalInput, Vector2 starting)
        {
            this.cardinalInput = cardinalInput;
            this.starting = starting;
            this.current = starting;
        }

        public DirectedInput(CardinalDirection cardinalInput, Vector2 starting, Vector2 current)
        {
            this.cardinalInput = cardinalInput;
            this.starting = starting;
            this.current = current;
        }

        public static DirectedInput Directionless()
        {
            return new DirectedInput(CardinalDirection.NONE, Vector2.zero);
        }

        public static CardinalDirection GetDirectionFromVector2(Vector2 dir)
        {
            if (dir.x == 0)
            {
                if (dir.y == 0)
                {
                    // both y and x are 0
                    return CardinalDirection.NONE;
                }

                // x is 0 but y is up or down
                return dir.y < 0 ? CardinalDirection.DOWN : CardinalDirection.UP;
            }

            if (dir.y == 0)
            {
                // x is not 0 but y is, so left or right
                return dir.x < 0 ? CardinalDirection.LEFT : CardinalDirection.RIGHT;
            }

            if (dir.y > 0)
            {
                // x is not 0 and y is up
                return dir.x < 0 ? CardinalDirection.UP_LEFT : CardinalDirection.UP_RIGHT;
            }

            // x is not 0 and y is down
            return dir.x < 0 ? CardinalDirection.DOWN_LEFT : CardinalDirection.DOWN_RIGHT;
        }

        public static Vector2 GetVectorFromDirection(CardinalDirection dir)
        {
            switch (dir)
            {
                case CardinalDirection.UP:
                    return Vector2.up;
                case CardinalDirection.UP_RIGHT:
                    return Vector2.up + Vector2.right;
                case CardinalDirection.RIGHT:
                    return Vector2.right;
                case CardinalDirection.DOWN_RIGHT:
                    return Vector2.down + Vector2.right;
                case CardinalDirection.DOWN:
                    return Vector2.down;
                case CardinalDirection.DOWN_LEFT:
                    return Vector2.down + Vector2.left;
                case CardinalDirection.LEFT:
                    return Vector2.left;
                case CardinalDirection.UP_LEFT:
                    return Vector2.up + Vector2.left;
                default:
                    return Vector2.zero;
            }
        }

        public static CardinalDirection GetSnappedDirectionFromVector2(Vector2 dir, float verticalThresholdDegrees)
        {
            if (dir == Vector2.zero)
                return CardinalDirection.NONE;

            verticalThresholdDegrees = Mathf.Clamp(verticalThresholdDegrees, 0, 90);
            float verticalThreshold = Mathf.Deg2Rad * verticalThresholdDegrees;

            float rad = Mathf.Atan2(dir.y, dir.x);
            float upMax = Mathf.PI - verticalThreshold;
            float upMin = verticalThreshold;
            float downMin = -Mathf.PI + verticalThreshold;
            float downMax = -verticalThreshold;
            CardinalDirection cardinal;

            if (rad >= upMin && rad <= upMax)
                cardinal = CardinalDirection.UP;
            else if (rad >= downMin && rad <= downMax)
                cardinal = CardinalDirection.DOWN;
            else if (rad <= Mathf.PI / 2 && rad >= -Mathf.PI / 2)
                cardinal = CardinalDirection.RIGHT;
            else
                cardinal = CardinalDirection.LEFT;

            // Debug.Log("Current vector: " + dir + ", radians: " + rad + ", Direction: " + cardinal + ", Threshold: " + ( Mathf.Rad2Deg * verticalThreshold));

            return cardinal;
        }

        public CardinalDirection GetSnappedStartingDirection(float verticalThresholdDegrees)
        {
            return GetSnappedDirectionFromVector2(starting, verticalThresholdDegrees);
        }

        public CardinalDirection GetSnappedCurrentDirection(float verticalThresholdDegrees)
        {
            return GetSnappedDirectionFromVector2(current, verticalThresholdDegrees);
        }
    }

    public const InputType COMPOSITE_INPUT_TYPE = (InputType.DIRECTIONAL | InputType.BUTTON);

    [SerializeField] private ControlLock.Controls cacheControl;
    public ControlLock.Controls CacheControl { get { return cacheControl; } private set { cacheControl = value; } }

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

    [SerializeField] private InputProcessStage processingStage = InputProcessStage.PENDING;
    public InputProcessStage ProcessingStage { get { return processingStage; } set { processingStage = value; } }

    [SerializeField] private DirectedInput direction;
    public DirectedInput Direction { get { return direction; } private set { direction = value; } }

    [SerializeField] private InputType controlType;
    public InputType ControlType { get { return controlType; } private set { controlType = value; } }

    public CharacterInput(ControlLock.Controls inputControl, ControlLock.Controls control, InputStage phase)
    {
        CacheControl = inputControl;
        Control = control;
        Phase = phase;
        ControlType = InputType.BUTTON;
        InputTime = Time.fixedTime;
        Direction = DirectedInput.Directionless();
    }

    public CharacterInput(ControlLock.Controls cacheControl, ControlLock.Controls control, InputStage phase, Vector2 direction)
    {
        CacheControl = cacheControl;

        // check the control type
        if (IsDirectionalControl(control) && !IsButtonControl(control))
        {
            // if this is strictly a directional control, ensure that
            ControlType = InputType.DIRECTIONAL;
        }
        else
        {
            // otherwise, become directional only if given a direction
            ControlType = direction == Vector2.zero ? InputType.BUTTON : InputType.DIRECTIONAL;
        }

        // if the input vector is equivalent to 0 and the control type is button, 
        // remove directional input from the controls
        if (ControlType == InputType.BUTTON)
            control = control & (~ControlLock.DIRECTIONAL_CONTROLS);

        Control = control;
        Phase = phase;
        Direction = new DirectedInput(direction);

        // if it's composite input, fix the control type flags
        if (CompositeInput)
            ControlType = COMPOSITE_INPUT_TYPE;

        InputTime = Time.fixedTime;
    }

    // used to create an exact character input (such as when combining inputs)
    // does not adjust given values
    public CharacterInput(ControlLock.Controls cacheControl, ControlLock.Controls control, InputStage phase, DirectedInput direction, InputType controlType)
    {
        CacheControl = cacheControl;
        Control = control;
        Phase = phase;
        Direction = direction;
        ControlType = controlType;
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

    public void UpdateDirection(Vector2 dir)
    {
        direction.current = dir;
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

    public bool IsPending()
    {
        return ProcessingStage == InputProcessStage.PENDING;
    }

    public bool IsProcessing()
    {
        return ProcessingStage == InputProcessStage.PROCESSING;
    }

    public bool IsInterrupted()
    {
        return ProcessingStage == InputProcessStage.INTERRUPTED;
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

    /// <summary>
    /// This method is volatile -- it returns this object, updating the values of this object to
    /// combine with the other given input object.
    /// </summary>
    /// <param name="input"> The input to combine this one with. </param>
    /// <returns> This object. </returns>
    public CharacterInput CombineWith(CharacterInput input)
    {
        // account for the direction changing
        DirectedInput dir = Direction;
        dir.current = input.Direction.current;

        /*
        // create another input with same controls, the new phase, adjusted dir, and the same control type
        CharacterInput target = new CharacterInput(Control, input.Phase, dir, ControlType);
        */

        // update the direction and phase of this input to match the recent update
        Phase = input.Phase;
        Direction = dir;

        /*
        // fix time variables to match this object's time variables
        target.InputTime = InputTime;
        target.Duration = Duration;
        target.DurationTime = DurationTime;
        target.latestFrameIncrement = latestFrameIncrement;
        */
        return this;
    }

    public static Pair<int, CharacterInput> CombineInputPair(Pair<int, CharacterInput> current, Pair<int, CharacterInput> next)
    {
        int leftVal = next.right.Phase == InputStage.RELEASED ? current.left : next.left;
        CharacterInput rightVal = current.right.CombineWith(next.right);
        return new Pair<int, CharacterInput>(leftVal, rightVal);
    }

    public override string ToString()
    {
        return "CharacterInput: [control=" + cacheControl + "], [direction=" + Direction.current + "], [phase=" + Phase + "], [processStage=" + processingStage + "]";
    }
        
}
