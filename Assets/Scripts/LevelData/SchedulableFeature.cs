using UnityEngine;
using Void.Curves;

namespace Void.LevelData
{
    [CreateAssetMenu(fileName = "Schedulable Feature", menuName = "Void/Schedulable Feature", order = 0)]
    public class SchedulableFeature : ScriptableObject
    {
        [field: SerializeField] public Mineable Prefab { get; private set; }

        [field: Space, Header("Probability")]
        [field: SerializeField, Range(0, 1)]
        public float Probability { get; private set; } = 1f;

        [field: SerializeField]
        public BaseCurve RollDistributionCurve { get; private set; }

        [field: SerializeField,
                Tooltip("Boosts the spawn rate when the number of spawned objects is under this amount.")]
        public int BoostWhenBelow { get; private set; } = 4;

        [field: Space, Header("Spawn Behaviour")]
        [field: SerializeField, Range(0, 1)]
        public float JitterSeconds { get; private set; } = 0.1f;

        [field: SerializeField, Min(0)] public float BaseRate { get; private set; } = 0.2f;
    }
}