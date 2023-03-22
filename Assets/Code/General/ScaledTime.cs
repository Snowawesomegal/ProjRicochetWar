using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledTime
{
    [SerializeField] public const float MIN_TIME_SCALE = 0;

    [SerializeField] private List<Pair<float, float>> slowTimes = new List<Pair<float, float>>();
    [SerializeField] private bool slowed;
    [SerializeField] private float speed;
    public bool Slowed { get { return slowed; } }
    public float Speed { get { return slowed ? speed : 1f; } }

    public System.Func<bool> Frozen;

    public ScaledTime()
    {

    }

    public virtual void Slow(float speed, float duration)
    {
        if (speed <= MIN_TIME_SCALE)
            speed = MIN_TIME_SCALE;

        float slowEndTime = Time.time + duration * speed;
        Pair<float, float> slowTime = new Pair<float, float>(speed, slowEndTime);
        slowTimes.Add(slowTime);
        if (slowTimes.Count > 1)
            slowTimes.Sort(SlowTimeComparator);

        this.speed = slowTimes[0].left;
        this.slowed = true;
        Time.timeScale = this.speed;
    }

    public int SlowTimeComparator(Pair<float, float> slowPair1, Pair<float, float> slowPair2)
    {
        int compare1 = slowPair1.left.CompareTo(slowPair2.left);
        if (compare1 != 0)
            return compare1;
        return slowPair1.right.CompareTo(slowPair2.right); ;
    }

    public virtual void UpdateSlowTime()
    {
        if (!slowed)
            return;

        if (Frozen.Invoke())
        {
            slowTimes.ForEach(slowPair => slowPair.right += Time.deltaTime);
        }

        while (slowTimes.Count > 0 && Time.time >= slowTimes[0].right)
        {
            slowTimes.RemoveAt(0);
            if (slowTimes.Count == 0)
            {
                this.slowed = false;
                Time.timeScale = 1f;
            }
            else
            {
                this.speed = slowTimes[0].left;
                Time.timeScale = slowTimes[0].left;
            }
        }
    }
}
