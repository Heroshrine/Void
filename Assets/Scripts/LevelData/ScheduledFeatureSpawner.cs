using UnityEngine;
using Random = UnityEngine.Random;

namespace Void.LevelData
{
    public class ScheduledFeatureSpawner : MonoBehaviour
    {
        private FeatureSpawnScheduler _scheduler;

        // TODO: maybe instead, use a bounds + a handle to edit to cut down on physics cost
        [SerializeField] private BoxCollider2D _spawnableArea;
        [SerializeField] private SchedulableFeature[] _features;

        private void Awake()
        {
            _scheduler =
                new FeatureSpawnScheduler((uint)Random.Range(1, int.MaxValue - 1) +
                                          (uint)Random.Range(1, int.MaxValue - 1));

            foreach (var feature in _features)
                _scheduler.TryAddPlan(feature, feature.BaseRate);
        }

        private void Update()
        {
            _scheduler.Update(Time.deltaTime, SpawnFeatures);
        }

        private void SpawnFeatures(SchedulableFeature feature, int count)
        {
            //TODO: object pooling helpers for features. For now, just spawn them.
            for (var i = 0; i < count; i++)
                Instantiate(feature.Prefab, GetRandomPointInBounds(), Quaternion.identity);
        }

        private Vector2 GetRandomPointInBounds() =>
            _spawnableArea.transform.TransformPoint(new Vector2(
                Random.Range(-_spawnableArea.size.x / 2f, _spawnableArea.size.x / 2f),
                Random.Range(-_spawnableArea.size.y / 2f, _spawnableArea.size.y / 2f)));
    }
}