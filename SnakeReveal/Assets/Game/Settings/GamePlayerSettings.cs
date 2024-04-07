using System;
using UnityEngine;

namespace Game.Settings
{
    [Serializable]
    public class GamePlayerSettings
    {
        [SerializeField] private GamePlayerSettingsContainer _container;

        private const string KeyPrefix = nameof(GamePlayerSettings) + ".";

        private const string SwipeThresholdKey = KeyPrefix + nameof(SwipeThreshold);

        [SerializeField] private float _swipeThreshold = 1f;

        public float SwipeThreshold
        {
            get => _swipeThreshold;
            set => _swipeThreshold = value;
        }

        public GamePlayerSettings(GamePlayerSettings defaults)
        {
            _container = defaults._container;
            OverrideWithDefaults(defaults);
            OverrideWithPlayerPrefs();
        }

        public void OverrideWithDefaults(GamePlayerSettings defaults)
        {
            SwipeThreshold = defaults.SwipeThreshold;
        }

        public void OverrideWithPlayerPrefs()
        {
            _swipeThreshold = PlayerPrefs.GetFloat(SwipeThresholdKey, _swipeThreshold);
        }

        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetFloat(SwipeThresholdKey, _swipeThreshold);
        }
    }
}