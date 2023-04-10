using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pair<L, R>
{
    [SerializeField] public L left;
    [SerializeField] public R right;

    public Pair(L left, R right)
    {
        this.left = left;
        this.right = right;
    }
}