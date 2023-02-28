using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EffectSound : AbstractSound
{
    [SerializeField] public string groupName = "";
    [SerializeField] public float groupWeight = 1f;
    [SerializeField] public bool HasGroup { get { return !string.IsNullOrWhiteSpace(groupName); } }

    public bool TryGetGroupName(out string name)
    {
        name = groupName;
        return HasGroup;
    }

    public override void OnEstablishSource()
    {
        // :)
    }
}
