using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSlowing
{
    public class TargetedSlowTime <T> where T : SlowTime<T>
    {
        public T slowTime;
        public List<string> targets;

        public TargetedSlowTime(T slowTime, params IIdentifiable[] targets)
        {
            this.targets = new List<string>();
            this.slowTime = slowTime;
            AddTargets(targets);
        }

        public void AddTargets(params IIdentifiable[] targets)
        {
            foreach (IIdentifiable target in targets)
                if (!this.targets.Contains(target.Identifier))
                    this.targets.Add(target.Identifier);
        }

        public void RemoveTargets(params IIdentifiable[] targets)
        {
            foreach (IIdentifiable target in targets)
                this.targets.Remove(target.Identifier);
        }

        public bool ApplysToTarget(IIdentifiable identifiable)
        {
            return targets.Contains(identifiable.Identifier);
        }

        public bool ApplysToTarget(string target)
        {
            return targets.Contains(target);
        }

        public int CompareTo(TargetedSlowTime<T> tst)
        {
            return slowTime.CompareTo(tst.slowTime);
        }
    }

    public class TargetedSlowInfo <T> where T : SlowTime<T>
    {
        private float timeScale = 1f;
        private bool slowed = false;
        public System.Action<float> OnSlow;
        public float TimeScale { get { return slowed ? timeScale : 1f; } }
        public bool Slowed { get { return slowed; } }
        public TargetedSlowInfo() { }
        public TargetedSlowInfo(System.Action<float> onSlow)
        {
            this.OnSlow = onSlow;
        }

        public void Slow(float timeScale)
        {
            this.timeScale = timeScale;
            slowed = true;
            OnSlow?.Invoke(timeScale);
        }
        public void NotSlowed()
        {
            slowed = false;
            OnSlow?.Invoke(1f);
        }
    }

    public class SlowTimeController <T> where T : SlowTime<T>
    {
        protected List<T> slowTimes;
        protected List<TargetedSlowTime<T>> targetedSlowTimes;
        protected Dictionary<string, TargetedSlowInfo<T>> targetedSlowSubscribers;

        protected SlowUpdateType updateType;
        public SlowUpdateType UpdateType { get { return updateType; } }

        protected bool frozen;
        public bool Frozen { get { return frozen || (slowed && timeScale == 0); } set { if (value != frozen) { frozen = value; OnFreeze?.Invoke(frozen); } } }
        public event System.Action<bool> OnFreeze;

        protected float timeScale = 1f;
        protected bool slowed = false;
        public float TimeScale { get { return frozen ? 0 : slowed ? timeScale : 1f; } }
        public bool Slowed { get { return slowed; } }

        public SlowTimeController() : this(SlowUpdateType.FIXED) { }
        public SlowTimeController(SlowUpdateType updateType)
        {
            slowTimes = new List<T>();
            targetedSlowTimes = new List<TargetedSlowTime<T>>();
            targetedSlowSubscribers = new Dictionary<string, TargetedSlowInfo<T>>();
            this.updateType = updateType;
        }

        public void SubscribeTargetedSlow(IIdentifiable identifiable)
        {
            if (!targetedSlowSubscribers.ContainsKey(identifiable.Identifier))
                targetedSlowSubscribers.Add(identifiable.Identifier, new TargetedSlowInfo<T>());
        }

        public System.Action<float> SubscribeTargetedSlow(IIdentifiable identifiable, System.Action<float> onSlow)
        {
            if (targetedSlowSubscribers.TryGetValue(identifiable.Identifier, out TargetedSlowInfo<T> tsi))
            {
                System.Action<float> currentOnSlow = tsi.OnSlow;
                targetedSlowSubscribers[identifiable.Identifier].OnSlow = onSlow;
                return currentOnSlow;
            }
            else
            {
                targetedSlowSubscribers.Add(identifiable.Identifier, new TargetedSlowInfo<T>(onSlow));
                return null;
            }
        }

        public bool UnsubscribeTargetedSlow(IIdentifiable identifiable)
        {
            return targetedSlowSubscribers.Remove(identifiable.Identifier);
        }

        public void RemoveSlows(IIdentifiable identifiable)
        {
            foreach (TargetedSlowTime<T> tst in targetedSlowTimes)
            {
                tst.RemoveTargets(identifiable);
            }

            if (targetedSlowSubscribers.TryGetValue(identifiable.Identifier, out TargetedSlowInfo<T> tsi))
            {
                tsi.NotSlowed();
            }
        }

        public void Slow(T st)
        {
            slowed = true;
            if (slowTimes.Count == 0 || slowTimes[slowTimes.Count - 1].CompareTo(st) <= 0)
            {
                slowTimes.Add(st);
                timeScale = st.Speed;
                return;
            }

            if (slowTimes[0].CompareTo(st) >= 0)
            {
                slowTimes.Insert(0, st);
                timeScale = st.Speed;
                return;
            }

            int index = slowTimes.BinarySearch(st);
            if (index < 0)
                index = ~index;
            slowTimes.Insert(index, st);
            timeScale = slowTimes[0].Speed;
        }

        public void Slow(T st, params IIdentifiable[] targets)
        {
            TargetedSlowTime<T> tst = new TargetedSlowTime<T>(st, targets);
            if (targetedSlowTimes.Count == 0 || targetedSlowTimes[targetedSlowTimes.Count - 1].CompareTo(tst) <= 0)
            {
                targetedSlowTimes.Add(tst);
            }
            else if (targetedSlowTimes[0].CompareTo(tst) >= 0)
            {
                targetedSlowTimes.Insert(0, tst);
            }
            else
            {
                int index = targetedSlowTimes.BinarySearch(tst);
                if (index < 0)
                    index = ~index;
                targetedSlowTimes.Insert(index, tst);
            }
            UpdateTargetedSlowTimes(tst.targets);
        }

        protected void UpdateTargetedSlowTimes(List<string> targets)
        {
            foreach (string target in targets)
            {
                if (targetedSlowSubscribers.TryGetValue(target, out TargetedSlowInfo<T> tsi))
                {
                    bool updated = false;
                    foreach (TargetedSlowTime<T> tst in targetedSlowTimes)
                    {
                        if (tst.ApplysToTarget(target))
                        {
                            if (tsi.TimeScale != tst.slowTime.Speed)
                                tsi.Slow(tst.slowTime.Speed);
                            updated = true;
                            break;
                        }
                    }
                    if (!updated)
                        tsi.NotSlowed();
                }
            }
        }

        public float GetTimeScale(IIdentifiable identifiable)
        {
            if (targetedSlowSubscribers.TryGetValue(identifiable.Identifier, out TargetedSlowInfo<T> tsi))
                return tsi.TimeScale * TimeScale;
            return TimeScale;
        }

        public virtual void Tick()
        {
            for (int i = 0; i < slowTimes.Count; i++)
            {
                if (slowTimes[i].Tick(frozen))
                {
                    slowTimes.RemoveAt(i);
                    if (i == 0)
                    {
                        if (slowTimes.Count > 0)
                            timeScale = slowTimes[0].Speed;
                        else
                            slowed = false;
                    }
                    i--;
                }
            }

            List<string> updateTargets = new List<string>();
            for (int i = 0; i < targetedSlowTimes.Count; i++)
            {
                if (targetedSlowTimes[i].slowTime.Tick(frozen))
                {
                    foreach (string target in targetedSlowTimes[i].targets)
                        if (!updateTargets.Contains(target))
                            updateTargets.Add(target);
                    targetedSlowTimes.RemoveAt(i);
                    i--;
                }
            }
            UpdateTargetedSlowTimes(updateTargets);
        }

        protected virtual void Update()
        {
            if (updateType == SlowUpdateType.FRAME)
                Tick();
        }

        protected virtual void FixedUpdate()
        {
            if (updateType == SlowUpdateType.FIXED)
                Tick();
        }
    }
}