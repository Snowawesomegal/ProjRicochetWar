using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimationEvents : MonoBehaviour
{
    Control1 c1;
    public GameObject owner; // must have Control1 (must be player)

    // Start is called before the first frame update
    void Start()
    {
        c1 = owner.GetComponent<Control1>();

        foreach(Transform i in transform)
        {
            if (i.TryGetComponent(out HitboxInfo hbi))
            {
                Debug.Log("set hbi owner to owner (" + owner + ").");
                hbi.owner = owner;
            }
        }
    }

    public void PlaySoundFromAnimator(string name)
    {
        c1.am.PlaySound(name);
    }

    public void PlaySoundGroupFromAnimator(string name)
    {
        c1.am.PlaySoundGroup(name);
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
