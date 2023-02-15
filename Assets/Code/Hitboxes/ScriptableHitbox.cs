using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hitbox", menuName = "Hitbox")]
public class ScriptableHitbox : ScriptableObject
{
    public float xsize;
    public float ysize;
    public float xoffset;
    public float yoffset;
    public bool changeToVertical;
    public float knockback;
    public float kbAngleNeg90to90;
    public bool angleIndependentOfMovement;
    public float damage;
    public int activeFrames;
}
