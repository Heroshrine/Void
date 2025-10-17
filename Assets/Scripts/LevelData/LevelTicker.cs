using System;
using UnityEngine;

namespace Void.LevelData
{
    public class LevelTicker : MonoBehaviour
    {
        public const int TICK_RATE = 20;
        public const float TICK_INTERVAL = 1f / TICK_RATE;
        private const int k_CATCHUP_TICKS = 3;

        private static LevelTicker s_tickerInstance;

        private static LevelTicker TickerInstance
        {
            get
            {
                if (s_tickerInstance) return s_tickerInstance;

                s_tickerInstance = FindFirstObjectByType<LevelTicker>();
                if (!s_tickerInstance)
                    s_tickerInstance = new GameObject("Level Ticker").AddComponent<LevelTicker>();

                return s_tickerInstance;
            }
        }

        public static event Action OnTick
        {
            add => TickerInstance.OnLocalTick += value;

            remove
            {
                if (s_tickerInstance)
                    s_tickerInstance.OnLocalTick -= value;
            }
        }

        public static event Action OnTickerDestroyed
        {
            add => TickerInstance.OnLocalDestroyed += value;
            remove
            {
                if (s_tickerInstance)
                    s_tickerInstance.OnLocalDestroyed -= value;
            }
        }

        private event Action OnLocalTick;
        private event Action OnLocalDestroyed;

        private float _tickAccumulator;

        private void Update()
        {
            _tickAccumulator += Time.deltaTime;

            if (_tickAccumulator < TICK_INTERVAL) return;

            var tickCount = 0;
            while (_tickAccumulator >= TICK_INTERVAL && tickCount < k_CATCHUP_TICKS)
            {
                OnLocalTick?.Invoke();
                _tickAccumulator -= TICK_INTERVAL;
                tickCount++;
            }
        }

        private void OnDestroy()
        {
            OnLocalDestroyed?.Invoke();
        }
    }
}