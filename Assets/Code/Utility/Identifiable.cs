using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIdentifiable
{
    private static uint nextID;
    public static uint GetNextID()
    {
        return nextID++;
    }

    public bool InitializedID { get; protected set; }
    public abstract uint ID { get; protected set; }
    public virtual string Identifier { get { return this.GetType().ToString() + ID; } }
    public virtual bool InitializeID()
    {
        if (InitializedID)
            return false;

        this.ID = GetNextID();
        InitializedID = true;
        return true;
    }
}
