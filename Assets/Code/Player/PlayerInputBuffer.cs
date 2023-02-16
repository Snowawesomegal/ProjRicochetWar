using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBuffer
{
    private List<Pair<int, CharacterInput>>[] inputBuffers;
    public bool debugMessages = false;

    public static int GetBitNumb(ControlLock.Controls control)
    {
        int numb = -1;
        int temp = (int)control;
        while (temp > 0)
        {
            numb++;
            temp = temp >> 1;
        }
        return numb;
    }

    public void InitializeBuffers(ControlLock.Controls[] controlList)
    {
        int max = 0;
        foreach (ControlLock.Controls control in controlList)
        {
            int current = GetBitNumb(control);
            if (current > max)
                max = current;
        }
        inputBuffers = new List<Pair<int, CharacterInput>>[max + 1];
        foreach (ControlLock.Controls control in controlList)
        {
            int index = GetBitNumb(control);
            if (index < 0 || index > max)
            {
                if (debugMessages)
                    Debug.LogError("Error - " + control.ToString() + " is resulting in an out of bounds index for the input buffers: " + index);
                continue;
            }
            inputBuffers[index] = new List<Pair<int, CharacterInput>>();
        }
    }

    public bool TryGetBuffer(ControlLock.Controls control, out List<Pair<int, CharacterInput>> buffer)
    {
        int index = GetBitNumb(control);
        if (index < inputBuffers.Length)
        {
            buffer = inputBuffers[index];
            return true;
        }

        buffer = null;
        Debug.LogError("Error - tried accessing input buffer with controls: " + control.ToString() + ". Buffer does not exist.");
        return false;
    }

    public bool AcceptInput(ControlLock.Controls control, int bufferTime, InputAction.CallbackContext ctxt)
    {
        return AcceptInput(control, bufferTime, ctxt, false);
    }
    public bool AcceptInput(ControlLock.Controls control, int bufferTime, InputAction.CallbackContext ctxt, bool retrying)
    {
        if (TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer))
        {
            Vector2 direction;
            if ((control & ControlLock.DIRECTIONAL_CONTROLS) > 0)
            {
                direction = ctxt.ReadValue<Vector2>();
            } else
            {
                direction = GetCurrentDirectional();
            }
            CharacterInput.InputStage phase = ctxt.started || ctxt.performed ? CharacterInput.InputStage.HELD : CharacterInput.InputStage.RELEASED;
            CharacterInput characterInput = new CharacterInput(control, phase, direction);
            buffer.Add(new Pair<int, CharacterInput>(bufferTime, characterInput));
            if (debugMessages)
                Debug.Log("Accepted input to buffer: " + control.ToString() + " with phase: " + characterInput.Phase.ToString());
            return true;
        } else if (!retrying)
        {
            AcceptInput(control, bufferTime, ctxt, true);
        }
        return false;
    }

    public Vector2 GetCurrentDirectional()
    {
        if (TryGetBuffer(ControlLock.DIRECTIONAL_CONTROLS, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
        {
            Pair<int, CharacterInput> latestEntry = buffer[buffer.Count - 1];
            return latestEntry.right.Direction;
        }
        return Vector2.zero;
    }

    public bool AcceptBufferInput(ControlLock.Controls control, int bufferTime, CharacterInput input)
    {
        if (TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer))
        {
            buffer.Add(new Pair<int, CharacterInput>(bufferTime, input));
            return true;
        }
        return false;
    }

    public void CleanBuffer(List<Pair<int, CharacterInput>> buffer, ControlLock.Controls control)
    {
        for (int i = 0; i < buffer.Count - 1; i++)
        {
            Pair<int, CharacterInput> current = buffer[i];
            Pair<int, CharacterInput> next = buffer[i + 1];
            if (current.right.CanCombineInputs(next.right))
            {
                Pair<int, CharacterInput> combined = CharacterInput.CombineInputPair(current, next);
                buffer[i] = combined;
                buffer.RemoveAt(i + 1);
                if (debugMessages)
                    Debug.Log("Combining inputs in buffer: " + control.ToString() + ", new Count: " + buffer.Count);
            }
        }
    }

    public void MaintainBuffer(List<Pair<int, CharacterInput>> buffer, ControlLock.Controls control)
    {
        CleanBuffer(buffer, control);
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            Pair<int, CharacterInput> current = buffer[i];
            if (current.right.TryIncrementFrames() && current.right.Phase == CharacterInput.InputStage.RELEASED)
            {
                current.left = current.left - 1;
            }
            if (current.left <= 0)
            {
                if (debugMessages)
                    Debug.Log("Removing expired input from buffer: " + control.ToString());
                buffer.RemoveAt(i);
            }
        }
    }

    public bool GrabImmediateInput(List<Pair<int, CharacterInput>> buffer, out CharacterInput input)
    {
        if (buffer.Count > 0)
        {
            input = buffer[0].right;
            if (input.IsReleased())
            {
                buffer.RemoveAt(0);
                if (debugMessages)
                    Debug.Log("Popped released input from buffer.");
            }
            return true;
        }
        input = null;
        return false;
    }
}
