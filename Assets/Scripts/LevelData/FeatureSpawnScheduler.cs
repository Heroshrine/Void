using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using Random = Unity.Mathematics.Random;

namespace Void.LevelData
{
    public sealed class FeatureSpawnScheduler
    {
        private const float k_EPS = 1e-6f;

        // --- Timebase ---
        private float _seconds;

        // -- Internals --
        private Random _random;
        private bool _ready;
        private readonly int _initialPlanCapacity;

        private readonly SortedDictionary<int, Plan> _plans;

        private sealed class Plan
        {
            public SchedulableFeature feature;
            public float rate;
            public float fractional; // carryover
            public float jitter;

            public float[] times;
            public int count;
            public int next;

            public Plan(SchedulableFeature id, float rate, float jitter, int initialCapacity)
            {
                feature = id;
                this.rate = rate;
                this.jitter = jitter;
                times = new float[initialCapacity];
                count = 0;
                next = 0;
            }
        }

        public FeatureSpawnScheduler(uint seed, int initialPlanCapacity = 8)
        {
            _random = new Random(seed == 0u ? 1 : seed);

            _seconds = 0;
            _ready = false;

            _plans = new SortedDictionary<int, Plan>();
            _initialPlanCapacity = initialPlanCapacity;
        }

        #region Plan Management

        public bool TryAddPlan(SchedulableFeature feature, float ratePerSecond, int initialCapacity = -1)
        {
            if (_plans.ContainsKey(feature.GetInstanceID())) return false;

            var cap = Mathf.Max(1, initialCapacity == -1 ? _initialPlanCapacity : initialCapacity);
            var jitter = Mathf.Max(feature.JitterSeconds, 0);

            _plans.Add(feature.GetInstanceID(), new Plan(feature, ratePerSecond, jitter, cap));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DoesPlanExist(SchedulableFeature feature) => _plans.ContainsKey(feature.GetInstanceID());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemovePlan(SchedulableFeature feature) => _plans.Remove(feature.GetInstanceID());


        public void ClearPlans()
        {
            _plans.Clear();
            _seconds = 0;
            _ready = false;
        }

        public bool TrySetPlanRate(SchedulableFeature feature, float ratePerSecond)
        {
            if (!_plans.TryGetValue(feature.GetInstanceID(), out var plan)) return false;
            plan.rate = ratePerSecond;
            return true;
        }

        public bool TrySetPlanJitter(SchedulableFeature feature, float jitterSeconds)
        {
            if (!_plans.TryGetValue(feature.GetInstanceID(), out var plan)) return false;
            plan.jitter = jitterSeconds;
            return true;
        }

        // expensive
        public void GetPlanIds(List<SchedulableFeature> outIds)
        {
            Assert.IsNotNull(outIds);
            outIds.Clear();
            foreach (var kvp in _plans) outIds.Add(kvp.Value.feature);
        }

        #endregion

        public int Update(float deltaTime, [NotNull] Action<SchedulableFeature, int> onDue)
        {
            Assert.IsNotNull(onDue);
            Assert.IsTrue(_plans.Count > 0, "No schedulable plans registered.");
            EnsurePlanned();

            _seconds += deltaTime;
            var rolledOver = _seconds >= 1f;
            var finalTotal = 0;

            if (rolledOver)
            {
                if (_seconds >= 2f)
                    Debug.LogWarning("Time rolled over by more than 1 second! Are we behind?");

                _seconds = Mathf.Repeat(_seconds, 1f);
            }

            foreach (var p in _plans.Values)
            {
                if (rolledOver)
                {
                    var count = GetPlanCount(p, p.rate);
                    BuildScheduleForNextSecond(p, count);
                }

                var due = 0;
                while (p.next < p.count && _seconds >= p.times[p.next])
                {
                    due++;
                    p.next++;
                }

                if (due > 0)
                    onDue(p.feature, due);

                finalTotal += due;
            }

            return finalTotal;
        }

        #region Helpers

        private void BuildScheduleForNextSecond(Plan p, int count)
        {
            p.count = 0;
            p.next = 0;

            if (count <= 0) return;

            EnsureCapacity(p, count);

            for (var i = 0; i < count; i++)
            {
                var t = (i + 1f) / (count + 1f);
                t += Jitter(p.jitter);
                p.times[i] = Mathf.Clamp(t, k_EPS, 1 - k_EPS);
            }

            p.count = count;
            Array.Sort(p.times, 0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsurePlanned()
        {
            if (_ready) return;

            foreach (var p in _plans.Values)
            {
                int count = GetPlanCount(p, p.rate);
                BuildScheduleForNextSecond(p, count);
            }

            _ready = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Jitter(float amplitude)
        {
            if (amplitude <= 0) return 0;

            var c = _random.NextFloat(-1f, 1f);
            return c * amplitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity(Plan p, int needed)
        {
            if (p.times.Length >= needed) return;
            var newCap = Mathf.NextPowerOfTwo(needed);
            Array.Resize(ref p.times, newCap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPlanCount(Plan p, float ratePerSecond)
        {
            if (ratePerSecond <= 0) return 0;

            var whole = Mathf.FloorToInt(ratePerSecond);
            var frac = ratePerSecond - whole;

            p.fractional += frac;
            var extra = Mathf.FloorToInt(p.fractional);
            p.fractional = Mathf.Max(0, p.fractional - extra);

            return whole + extra;
        }

        #endregion
    }
}