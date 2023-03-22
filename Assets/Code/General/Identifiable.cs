using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Identifiable
{
    public abstract bool Initialized { get; protected set; }
    public abstract uint ID { get; protected set; }
    public abstract string Name { get; }
    public virtual bool InitializeID(uint id)
    {
        if (Initialized)
            return false;

        ID = id;
        Initialized = true;
        return true;
    }
}