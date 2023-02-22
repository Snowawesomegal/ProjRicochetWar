using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBuffer
{
    private List<Pair<int, CharacterInput>>[] inputBuffers;
    public bool debugMessages = false;

    public float previousDirectionalCache;
    private CharacterInput.DirectedInput cachedDirectional;
    public CharacterInput.DirectedInput CachedDirectional { 
        get { 
            if (Time.fixedTime != previousDirectionalCache)
            {
                CacheCurrentDirectional();
                previousDirectionalCache = Time.fixedTime;
            }
            return cachedDirectional;
        }
    }

    private CharacterInput[] cachedInputs;

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
        cachedInputs = new CharacterInput[max + 1];
        for (int i = 0; i < cachedInputs.Length; i++)
        {
            cachedInputs[i] = null;
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

    public void CacheInput(CharacterInput input)
    {
        cachedInputs[GetBitNumb(input.CacheControl)] = input;
    }

    public void ClearCachedInput(ControlLock.Controls control)
    {
        cachedInputs[GetBitNumb(control)] = null;
    }

    public bool TryGetCachedInput(ControlLock.Controls control, out CharacterInput input)
    {
        input = cachedInputs[GetBitNumb(control)];
        return input != null;
    }

    public CharacterInput GetCachedInput(ControlLock.Controls control)
    {
        return cachedInputs[GetBitNumb(control)];
    }

    public bool AcceptInput(ControlLock.Controls control, int bufferTime, InputAction.CallbackContext ctxt)
    {
        if (TryGetBuffer(control, out List<Pair<int, CharacterInput>> buffer))
        {
            Vector2 direction;
            if ((control & ControlLock.DIRECTIONAL_CONTROLS) > 0)
            {
                direction = ctxt.ReadValue<Vector2>();
            } else
            {
                direction = CachedDirectional.current;
            }

            ControlLock.Controls inputSpecificControl = control;
            if (direction.x != 0)
                inputSpecificControl |= ControlLock.Controls.HORIZONTAL;
            if (direction.y != 0)
                inputSpecificControl |= ControlLock.Controls.VERTICAL;

            CharacterInput.InputStage phase = ctxt.started || ctxt.performed ? CharacterInput.InputStage.HELD : CharacterInput.InputStage.RELEASED;
            CharacterInput characterInput = new CharacterInput(control, inputSpecificControl, phase, direction);
            buffer.Add(new Pair<int, CharacterInput>(bufferTime, characterInput));
            if (debugMessages)
                Debug.Log("Accepted input to buffer: " + control.ToString() + " with phase: " + characterInput.Phase.ToString() + " and direction: " + direction);
            return true;
        }
        return false;
    }

    public CharacterInput.DirectedInput GetCurrentDirectional()
    {
        if (TryGetBuffer(ControlLock.DIRECTIONAL_CONTROLS, out List<Pair<int, CharacterInput>> buffer) && buffer.Count > 0)
        {
            Pair<int, CharacterInput> latestEntry = buffer[buffer.Count - 1];
            if (latestEntry.right.Phase == CharacterInput.InputStage.HELD)
            {
                Vector2 dir = latestEntry.right.Direction.current;
                return new CharacterInput.DirectedInput(dir);
            } else
            {
                Vector2 prevDir = latestEntry.right.Direction.current;
                Vector2 currentDir = Vector2.zero;
                return new CharacterInput.DirectedInput(CharacterInput.CardinalDirection.NONE, prevDir, currentDir);
            }
        }
        return CharacterInput.DirectedInput.Directionless();
    }

    public void CacheCurrentDirectional()
    {
        cachedDirectional = GetCurrentDirectional();
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
        if (debugMessages)
            Debug.Log("Start - cleaning buffer. Current buffer size: " + buffer.Count);
        for (int i = 0; i < buffer.Count - 1; i++)
        {
            Pair<int, CharacterInput> current = buffer[i];
            Pair<int, CharacterInput> next = buffer[i + 1];
            if (current.right.CanCombineInputs(next.right))
            {
                if (debugMessages)
                    Debug.Log("Merging " + current.right.ToString() + " with " + next.right.ToString());
                Pair<int, CharacterInput> combined = CharacterInput.CombineInputPair(current, next);
                buffer[i] = combined;
                buffer.RemoveAt(i + 1);
                if (debugMessages)
                    Debug.Log("Combining inputs in buffer: " + control.ToString() + ", new Count: " + buffer.Count);
            }
        }
        if (debugMessages)
            Debug.Log("Done - cleaning buffer. Current buffer size: " + buffer.Count);
    }

    public void MaintainBuffer(List<Pair<int, CharacterInput>> buffer, ControlLock.Controls control, bool canExpire)
    {
        CleanBuffer(buffer, control);
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            Pair<int, CharacterInput> current = buffer[i];
            // check to see if this input is interrupted and released
            // if so, remove it from the inputs
            if (current.right.IsInterrupted() && current.right.IsReleased())
            {
                if (debugMessages)
                    Debug.Log("Removing interrupted input from buffer: " + control.ToString());
                buffer.RemoveAt(i);
                continue;
            }

            // for only the latest value, if it's still held down
            if (i == buffer.Count - 1 && current.right.Phase == CharacterInput.InputStage.HELD)
            {
                // update the current direction of the input to match the current directional
                current.right.UpdateDirection(CachedDirectional.current);
            }

            // increment the current frame held number
            if ((current.right.TryIncrementFrames() || current.right.IsReleased()) && canExpire)
            {
                // decrease the time left in buffer if it's held down and allowed to expire
                current.left = current.left - 1;
            }

            // if the current has expired, remove it
            if (current.left <= 0)
            {
                if (debugMessages)
                    Debug.Log("Removing expired input from buffer: " + control.ToString());
                buffer.RemoveAt(i);
            }
        }
    }

    public bool TryPopNextInput(List<Pair<int, CharacterInput>> buffer, out CharacterInput input)
    {
        if (buffer.Count > 0)
        {
            input = buffer[0].right;
            if (input.IsReleased())
            {
                buffer.RemoveAt(0);
                if (debugMessages)
                    Debug.Log("Popped released input from buffer. Current buffer size: " + buffer.Count);
            }
            return true;
        }
        input = null;
        return false;
    }

    public bool PeekNextInput(List<Pair<int, CharacterInput>> buffer, out CharacterInput input)
    {
        if (buffer.Count > 0)
        {
            input = buffer[0].right;
            return true;
        }
        input = null;
        return false;
    }
}
