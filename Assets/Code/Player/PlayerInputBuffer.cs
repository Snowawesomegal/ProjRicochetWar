using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBuffer
{
    private Dictionary<ControlLock.Controls, Queue<Pair<int, InputAction.CallbackContext>>> inputBuffers = new Dictionary<ControlLock.Controls, Queue<Pair<int, InputAction.CallbackContext>>>();

    public bool TryGetBuffer(ControlLock.Controls control, out Queue<Pair<int, InputAction.CallbackContext>> buffer)
    {
        if (inputBuffers.ContainsKey(control))
        {
            buffer = inputBuffers[control];
            return buffer != null;
        }

        Debug.LogError("Error - tried accessing input buffer with controls: " + control.ToString() + ". Buffer does not exist.");
        buffer = null;
        return false;
    }

    //public void AcceptBufferInput(Queue<Pair<int, InputAction.CallbackContext>> buffer, Pair<int, InputAction.CallbackContext> input)
    //{

    //}

    //public void CleanBuffer(Queue<Pair<int, InputAction.CallbackContext>> buffer)
    //{

    //}
}
